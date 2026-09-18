# -*- coding: utf-8 -*-
"""JETTE bear — chibi vinyl-toy mascot, humanoid-rigged for Unity (Mixamo clip retarget).

Run:  blender -b --python jette_bear_rig.py -- <out_fbx> <preview_png>
Character faces -Y (Blender front), Z up, ~1.0 m tall.  Rigid per-part skinning
(each part 100 % on one bone) with ball joints so Mixamo run/jump/hit bend cleanly.
"""
import bpy, bmesh, math, sys, os
from mathutils import Vector

argv = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
OUT_FBX = argv[0] if argv else r'C:\dev\game-jette\Assets\Resources\CoastRun\Rig\JetteBear.fbx'
OUT_PNG = argv[1] if len(argv) > 1 else r'C:\dev\game-jette\Tools\blender\jette_bear_preview.png'
FONT_BOLD = r'C:\Windows\Fonts\arialbd.ttf'
FONT_KO = r'C:\Windows\Fonts\malgunbd.ttf'

# ---------------------------------------------------------------- scene reset
bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.unit_settings.system = 'METRIC'
scene.unit_settings.scale_length = 1.0

# ---------------------------------------------------------------- materials
MATS = {}
def mat(name, rgb, rough=0.45, spec=0.5):
    if name in MATS: return MATS[name]
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    bsdf = m.node_tree.nodes.get('Principled BSDF')
    bsdf.inputs['Base Color'].default_value = (*rgb, 1.0)
    bsdf.inputs['Roughness'].default_value = rough
    m.diffuse_color = (*rgb, 1.0)
    MATS[name] = m
    return m

FUR     = mat('JT_Fur',     (0.93, 0.72, 0.46), 0.7)
MUZZLE  = mat('JT_Muzzle',  (0.99, 0.94, 0.86), 0.7)
HELMET  = mat('JT_Helmet',  (1.00, 0.76, 0.12), 0.25)
BLUE    = mat('JT_Blue',    (0.52, 0.72, 0.92), 0.35)
WHITE   = mat('JT_White',   (0.97, 0.97, 0.98), 0.35)
BLACK   = mat('JT_Black',   (0.04, 0.04, 0.05), 0.25)
INK     = mat('JT_Ink',     (0.08, 0.32, 0.80), 0.4)
PINK    = mat('JT_Pink',    (1.00, 0.62, 0.76), 0.6)
MOUTH   = mat('JT_Mouth',   (0.93, 0.42, 0.52), 0.5)
STRAP   = mat('JT_Strap',   (0.22, 0.22, 0.24), 0.5)
SOLE    = mat('JT_Sole',    (0.84, 0.85, 0.88), 0.5)
PAWPAD  = mat('JT_PawPad',  (0.98, 0.80, 0.58), 0.7)

PARTS = []   # (object, bone_name)

def finish(ob, material, bone, smooth=True):
    ob.data.materials.clear()
    ob.data.materials.append(material)
    if smooth:
        for p in ob.data.polygons: p.use_smooth = True
    PARTS.append((ob, bone))
    return ob

def sphere(name, center, radius, scale=(1, 1, 1), seg=32, rings=16):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg, ring_count=rings, radius=radius, location=center)
    ob = bpy.context.object; ob.name = name
    ob.scale = scale
    bpy.ops.object.transform_apply(scale=True)
    return ob

def cylinder(name, center, radius, depth, rot=(0, 0, 0), verts=32, scale=(1, 1, 1)):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=radius, depth=depth, location=center, rotation=rot)
    ob = bpy.context.object; ob.name = name
    ob.scale = scale
    bpy.ops.object.transform_apply(scale=True, rotation=True)
    return ob

def torus(name, center, major, minor, rot=(0, 0, 0), scale=(1, 1, 1), seg=48, ring=12):
    bpy.ops.mesh.primitive_torus_add(major_segments=seg, minor_segments=ring, major_radius=major, minor_radius=minor, location=center, rotation=rot)
    ob = bpy.context.object; ob.name = name
    ob.scale = scale
    bpy.ops.object.transform_apply(scale=True, rotation=True)
    return ob

def box(name, center, size, rot=(0, 0, 0), bevel=0.0):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=center, rotation=rot)
    ob = bpy.context.object; ob.name = name
    ob.scale = size
    bpy.ops.object.transform_apply(scale=True, rotation=True)
    if bevel > 0:
        b = ob.modifiers.new('Bevel', 'BEVEL'); b.width = bevel; b.segments = 3
    return ob

def join(objs, name):
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs: o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    bpy.ops.object.join()
    ob = bpy.context.object; ob.name = name
    return ob

def capsule(name, a, b, radius):
    """Round-ended sausage from point a to point b (world coords)."""
    a, b = Vector(a), Vector(b)
    d = b - a; L = d.length
    mid = (a + b) * 0.5
    rot = d.to_track_quat('Z', 'Y').to_euler()
    cyl = cylinder(name + '_c', mid, radius, L, rot=rot)
    s1 = sphere(name + '_a', a, radius)
    s2 = sphere(name + '_b', b, radius)
    return join([cyl, s1, s2], name)

def cut_below(ob, z):
    """Delete geometry below world z (open dome)."""
    bm = bmesh.new(); bm.from_mesh(ob.data)
    mw = ob.matrix_world
    geom = bm.verts[:] + bm.edges[:] + bm.faces[:]
    res = bmesh.ops.bisect_plane(bm, geom=geom, plane_co=mw.inverted() @ Vector((0, 0, z)), plane_no=(0, 0, 1), clear_outer=False, clear_inner=True)
    bm.to_mesh(ob.data); bm.free()

def shrink_strip(name, center, size, target, offset=0.006, thick=0.012, subdiv=12, rot=(0, 0, 0)):
    """Flat strip wrapped onto `target` surface (placket, stripes, tag)."""
    bpy.ops.mesh.primitive_plane_add(size=1.0, location=center, rotation=rot)
    ob = bpy.context.object; ob.name = name
    ob.scale = (size[0], size[1], 1)
    bpy.ops.object.transform_apply(scale=True, rotation=True)
    bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.mesh.subdivide(number_cuts=subdiv); bpy.ops.object.mode_set(mode='OBJECT')
    sw = ob.modifiers.new('Wrap', 'SHRINKWRAP'); sw.target = target; sw.wrap_method = 'NEAREST_SURFACEPOINT'; sw.offset = offset
    so = ob.modifiers.new('Solid', 'SOLIDIFY'); so.thickness = thick; so.offset = 1.0
    return ob

def text_mesh(name, text, font_path, size, center, rot=(0, 0, 0), extrude=0.004, align='CENTER'):
    bpy.ops.object.text_add(location=center, rotation=rot)
    ob = bpy.context.object; ob.name = name
    ob.data.body = text
    ob.data.size = size
    ob.data.extrude = extrude
    ob.data.align_x = align; ob.data.align_y = 'CENTER'
    try:
        ob.data.font = bpy.data.fonts.load(font_path)
    except Exception as e:
        print('font load failed', font_path, e)
    bpy.ops.object.convert(target='MESH')
    ob = bpy.context.object
    return ob

# ---------------------------------------------------------------- proportions (metres)
Z_HIP = 0.30
Z_NECK = 0.62
HEAD_C = Vector((0, 0.0, 0.79)); HEAD_R = 0.26
BODY_C = Vector((0, 0, 0.43))

# ---------------------------------------------------------------- HEAD group
head = sphere('Head', HEAD_C, HEAD_R, scale=(1.04, 0.96, 0.98))
finish(head, FUR, 'Head')

# ears (peek out under the helmet, high on the sides)
for sx, tag in ((1, 'L'), (-1, 'R')):
    e = sphere('Ear_' + tag, (sx * 0.268, 0.045, 0.885), 0.08, scale=(1, 0.75, 1), seg=20, rings=12)
    finish(e, FUR, 'Head')
    ei = sphere('EarIn_' + tag, (sx * 0.268, -0.005, 0.885), 0.046, scale=(1, 0.5, 1), seg=16, rings=10)
    finish(ei, PAWPAD, 'Head')

# muzzle, nose, mouth
mz = sphere('Muzzle', (0, -0.215, 0.725), 0.125, scale=(1.0, 0.72, 0.78))
finish(mz, MUZZLE, 'Head')
nose = sphere('Nose', (0, -0.30, 0.76), 0.048, scale=(1.15, 0.8, 0.9), seg=20, rings=12)
finish(nose, BLACK, 'Head')
mouth = sphere('Mouth', (0, -0.295, 0.695), 0.034, scale=(1.3, 0.5, 0.8), seg=16, rings=10)
finish(mouth, MOUTH, 'Head')

# eyes: big black + two highlights
for sx, tag in ((1, 'L'), (-1, 'R')):
    ey = sphere('Eye_' + tag, (sx * 0.115, -0.225, 0.805), 0.058, scale=(1.0, 0.75, 1.15), seg=24, rings=14)
    finish(ey, BLACK, 'Head')
    h1 = sphere('EyeHi_' + tag, (sx * 0.135, -0.27, 0.83), 0.016, seg=12, rings=8)
    finish(h1, WHITE, 'Head')
    h2 = sphere('EyeHi2_' + tag, (sx * 0.095, -0.268, 0.785), 0.011, seg=10, rings=6)
    finish(h2, WHITE, 'Head')
    # blush
    ck = sphere('Cheek_' + tag, (sx * 0.20, -0.185, 0.715), 0.05, scale=(1.0, 0.35, 0.8), seg=16, rings=10)
    finish(ck, PINK, 'Head')

# helmet: dome + brim + bill + text
helm = sphere('Helmet', (0, 0.01, 0.90), 0.318, scale=(1.0, 1.0, 0.88), seg=48, rings=24)
cut_below(helm, 0.915)
so = helm.modifiers.new('Solid', 'SOLIDIFY'); so.thickness = 0.02; so.offset = -1
finish(helm, HELMET, 'Head')
brim = torus('HelmetBrim', (0, 0.01, 0.915), 0.318, 0.022, scale=(1.0, 1.0, 0.6))
finish(brim, HELMET, 'Head')
bill = sphere('HelmetBill', (0, -0.24, 0.91), 0.20, scale=(1.35, 0.9, 0.06), seg=32, rings=8)
finish(bill, HELMET, 'Head')
# "JETTE" — 앞·양옆·뒤 네 곳(사용자: 「헬멧에 jette 잘 보이게」 — 러닝은 뒤에서 보므로 뒤에도).
def helmet_text(name, center, rot, size=0.125):
    t = text_mesh(name, 'JETTE', FONT_BOLD, size, center, rot=rot, extrude=0.0)
    bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.mesh.subdivide(number_cuts=3); bpy.ops.object.mode_set(mode='OBJECT')
    sw = t.modifiers.new('Wrap', 'SHRINKWRAP'); sw.target = helm; sw.wrap_method = 'NEAREST_SURFACEPOINT'; sw.offset = 0.007
    so = t.modifiers.new('Solid', 'SOLIDIFY'); so.thickness = 0.012; so.offset = 1
    finish(t, INK, 'Head', smooth=False)
helmet_text('HelmetText', (0, -0.30, 1.02), (math.radians(66), 0, 0))
helmet_text('HelmetText_L', (0.31, 0.01, 0.995), (math.radians(66), 0, math.radians(90)), size=0.10)
helmet_text('HelmetText_R', (-0.31, 0.01, 0.995), (math.radians(66), 0, math.radians(-90)), size=0.10)
helmet_text('HelmetText_B', (0, 0.32, 1.005), (math.radians(66), 0, math.radians(180)), size=0.115)

# chin strap (yellow ring, tilted so the buckle sits under the chin) + side clips
strap = torus('ChinStrap', (0, -0.035, 0.80), 0.268, 0.013, rot=(math.radians(62), 0, 0), seg=64, ring=10)
finish(strap, HELMET, 'Head')
bk = box('Buckle', (0, -0.16, 0.565), (0.075, 0.03, 0.045), bevel=0.006)
finish(bk, STRAP, 'Head', smooth=False)
for sx, tag in ((1, 'L'), (-1, 'R')):
    c = box('Clip_' + tag, (sx * 0.262, -0.02, 0.77), (0.03, 0.05, 0.06), bevel=0.005)
    finish(c, STRAP, 'Head', smooth=False)

# ---------------------------------------------------------------- BODY group (Spine1)
body = sphere('Body', BODY_C, 0.205, scale=(1.0, 0.86, 0.92))
finish(body, BLUE, 'Spine1')
# white raglan panel down the front/back, collar, placket, buttons, pocket, tag
collar = torus('Collar', (0, -0.005, 0.585), 0.115, 0.03, scale=(1.05, 0.95, 0.6))
finish(collar, WHITE, 'Spine1')
placket = shrink_strip('Placket', (0, -0.20, 0.44), (0.05, 0.30), body, thick=0.01, subdiv=16)
finish(placket, WHITE, 'Spine1')
for i, z in enumerate((0.52, 0.44, 0.36)):
    b = sphere('Button_%d' % i, (0, -0.21, z), 0.017, scale=(1, 0.5, 1), seg=14, rings=8)
    finish(b, WHITE, 'Spine1')
back = shrink_strip('BackStripe', (0, 0.19, 0.45), (0.10, 0.28), body, thick=0.01, subdiv=16)
finish(back, WHITE, 'Spine1')
pocket = shrink_strip('Pocket', (0.11, -0.18, 0.47), (0.075, 0.07), body, offset=0.004, thick=0.012, subdiv=6)
finish(pocket, BLUE, 'Spine1')
tag = shrink_strip('NameTag', (-0.105, -0.17, 0.485), (0.085, 0.032), body, offset=0.006, thick=0.012, subdiv=6)
finish(tag, WHITE, 'Spine1')
tagtxt = text_mesh('NameTagText', '(주)제때', FONT_KO, 0.022, (-0.105, -0.205, 0.485), rot=(math.radians(90), 0, 0), extrude=0.0)
bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
bpy.ops.mesh.subdivide(number_cuts=2); bpy.ops.object.mode_set(mode='OBJECT')
sw = tagtxt.modifiers.new('Wrap', 'SHRINKWRAP'); sw.target = body; sw.wrap_method = 'NEAREST_SURFACEPOINT'; sw.offset = 0.02
so = tagtxt.modifiers.new('Solid', 'SOLIDIFY'); so.thickness = 0.004; so.offset = 1
finish(tagtxt, INK, 'Spine1', smooth=False)
# waist: white belt line
waist = torus('Waist', (0, 0, 0.30), 0.147, 0.018, scale=(1.0, 0.86, 0.7))
finish(waist, WHITE, 'Hips')

# ---------------------------------------------------------------- ARMS (T-pose, out along ±X)
SH = 0.545          # shoulder height
for sx, side in ((1, 'Left'), (-1, 'Right')):
    x0 = sx * 0.17; x1 = sx * 0.295; x2 = sx * 0.405
    up = capsule('UpperArm_' + side, (x0, 0, SH), (x1, 0, SH), 0.075)
    finish(up, BLUE, side + 'Arm')
    band = torus('ArmBand_' + side, (sx * 0.235, 0, SH), 0.076, 0.009, rot=(0, math.radians(90), 0), scale=(1, 1, 1))
    finish(band, WHITE, side + 'Arm')
    fo = capsule('ForeArm_' + side, (x1, 0, SH), (x2, 0, SH), 0.072)
    finish(fo, BLUE, side + 'ForeArm')
    cuff = torus('Cuff_' + side, (sx * 0.39, 0, SH), 0.073, 0.010, rot=(0, math.radians(90), 0))
    finish(cuff, WHITE, side + 'ForeArm')
    paw = sphere('Paw_' + side, (sx * 0.455, 0, SH), 0.072, scale=(1.05, 0.95, 0.95))
    finish(paw, FUR, side + 'Hand')

# ---------------------------------------------------------------- LEGS
for sx, side in ((1, 'Left'), (-1, 'Right')):
    x = sx * 0.105
    th = capsule('Thigh_' + side, (x, 0, Z_HIP), (x, 0, 0.19), 0.085)
    finish(th, BLUE, side + 'UpLeg')
    sh = capsule('Shin_' + side, (x, 0, 0.19), (x, 0, 0.13), 0.078)
    finish(sh, BLUE, side + 'Leg')
    bootc = sphere('Boot_' + side, (x, -0.015, 0.085), 0.10, scale=(0.95, 1.25, 0.8))
    finish(bootc, WHITE, side + 'Foot')
    boots = cylinder('Sole_' + side, (x, -0.02, 0.018), 0.105, 0.03, verts=32, scale=(0.95, 1.3, 1))
    finish(boots, SOLE, side + 'Foot')

# ---------------------------------------------------------------- apply modifiers, join into one mesh
bpy.ops.object.select_all(action='DESELECT')
for ob, bone in PARTS:
    bpy.context.view_layer.objects.active = ob
    for m in list(ob.modifiers):
        try: bpy.ops.object.modifier_apply(modifier=m.name)
        except Exception as e: print('modifier apply failed', ob.name, m.name, e)
    # rigid vertex group
    vg = ob.vertex_groups.new(name=bone)
    vg.add(list(range(len(ob.data.vertices))), 1.0, 'REPLACE')

mesh_objs = [ob for ob, _ in PARTS]
bear = join(mesh_objs, 'JetteBear_Mesh')
bpy.ops.object.shade_smooth()
# ---------------------------------------------------------------- ARMATURE (Mixamo bone names → Unity Humanoid auto-map)
bpy.ops.object.armature_add(enter_editmode=True, location=(0, 0, 0))
arm = bpy.context.object; arm.name = 'JetteBear'; arm.data.name = 'JetteBearArmature'
eb = arm.data.edit_bones
for b in list(eb): eb.remove(b)

def bone(name, head, tail, parent=None, connect=False):
    b = eb.new(name); b.head = head; b.tail = tail
    if parent: b.parent = eb[parent]; b.use_connect = connect
    return b

bone('Hips',   (0, 0, Z_HIP), (0, 0, 0.38))
bone('Spine',  (0, 0, 0.38), (0, 0, 0.46), 'Hips', True)
bone('Spine1', (0, 0, 0.46), (0, 0, 0.53), 'Spine', True)
bone('Spine2', (0, 0, 0.53), (0, 0, 0.585), 'Spine1', True)
bone('Neck',   (0, 0, 0.585), (0, 0, Z_NECK), 'Spine2', True)
bone('Head',   (0, 0, Z_NECK), (0, 0, 0.95), 'Neck', True)
bone('HeadTop_End', (0, 0, 0.95), (0, 0, 1.05), 'Head', True)
for sx, side in ((1, 'Left'), (-1, 'Right')):
    bone(side + 'Shoulder', (sx * 0.06, 0, 0.565), (sx * 0.17, 0, SH), 'Spine2')
    bone(side + 'Arm',      (sx * 0.17, 0, SH), (sx * 0.295, 0, SH), side + 'Shoulder', True)
    bone(side + 'ForeArm',  (sx * 0.295, 0, SH), (sx * 0.405, 0, SH), side + 'Arm', True)
    bone(side + 'Hand',     (sx * 0.405, 0, SH), (sx * 0.50, 0, SH), side + 'ForeArm', True)
    bone(side + 'UpLeg',    (sx * 0.105, 0, Z_HIP), (sx * 0.105, 0.005, 0.19), 'Hips')
    bone(side + 'Leg',      (sx * 0.105, 0.005, 0.19), (sx * 0.105, 0, 0.06), side + 'UpLeg', True)
    bone(side + 'Foot',     (sx * 0.105, 0, 0.06), (sx * 0.105, -0.10, 0.02), side + 'Leg', True)
    bone(side + 'ToeBase',  (sx * 0.105, -0.10, 0.02), (sx * 0.105, -0.16, 0.02), side + 'Foot', True)
bpy.ops.object.mode_set(mode='OBJECT')

# bind
bear.parent = arm
md = bear.modifiers.new('Armature', 'ARMATURE'); md.object = arm
bpy.ops.object.select_all(action='DESELECT')
arm.select_set(True); bear.select_set(True); bpy.context.view_layer.objects.active = arm

# ---------------------------------------------------------------- export
os.makedirs(os.path.dirname(OUT_FBX), exist_ok=True)
bpy.ops.export_scene.fbx(
    filepath=OUT_FBX, use_selection=True, object_types={'ARMATURE', 'MESH'},
    apply_scale_options='FBX_SCALE_ALL', axis_forward='-Z', axis_up='Y',
    add_leaf_bones=False, bake_anim=False, use_mesh_modifiers=True, mesh_smooth_type='FACE',
    path_mode='COPY', embed_textures=False, armature_nodetype='NULL',
    primary_bone_axis='Y', secondary_bone_axis='X', use_armature_deform_only=True)
print('FBX ->', OUT_FBX)

# ---------------------------------------------------------------- preview render (3/4 front)
bpy.ops.object.camera_add(location=(1.9, -2.6, 1.05), rotation=(math.radians(80), 0, math.radians(36)))
cam = bpy.context.object; cam.data.lens = 60; scene.camera = cam
bpy.ops.object.light_add(type='SUN', location=(2, -3, 5), rotation=(math.radians(45), math.radians(20), math.radians(30)))
bpy.context.object.data.energy = 3.0
bpy.ops.object.light_add(type='AREA', location=(-2.5, -2.5, 2.0)); bpy.context.object.data.energy = 400; bpy.context.object.data.size = 3
scene.render.engine = 'BLENDER_WORKBENCH'
scene.display.shading.light = 'STUDIO'; scene.display.shading.color_type = 'MATERIAL'
scene.display.shading.show_shadows = True; scene.display.shading.show_cavity = True
scene.render.resolution_x = 720; scene.render.resolution_y = 960
scene.render.film_transparent = False
world = bpy.data.worlds.new('W'); scene.world = world; world.color = (0.82, 0.82, 0.84)
scene.render.filepath = OUT_PNG
bpy.ops.render.render(write_still=True)
print('PNG ->', OUT_PNG)
# second view: straight front
cam.location = (0, -3.1, 0.62); cam.rotation_euler = (math.radians(87), 0, 0)
scene.render.filepath = OUT_PNG.replace('.png', '_front.png')
bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(os.path.dirname(OUT_PNG), 'jette_bear_rig.blend'))
