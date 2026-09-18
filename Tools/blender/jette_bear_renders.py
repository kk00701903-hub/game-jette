# -*- coding: utf-8 -*-
"""JETTE bear — UI renders (transparent PNG) from the rigged model.

Run:  blender -b Tools/blender/jette_bear_rig.blend --python jette_bear_renders.py -- <out_dir>
Outputs:
  UI_RunOver_Sad.png   764×855  — 주저앉은 곰(한 곡 실패 카드)
  UI_Face_Girl.png     512×512  — 얼굴 초상(완주 카드 금테 · 피버 버튼)
  UI_Face_Ring.png     254×258
  UI_Face_Butler.png   512×512
  bear_preview_pose.png         — 확인용
"""
import bpy, math, sys, os
from mathutils import Vector, Matrix, Euler

argv = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
OUT = argv[0] if argv else r'C:\dev\game-jette\Tools\blender\out'
os.makedirs(OUT, exist_ok=True)

scene = bpy.context.scene
arm = bpy.data.objects['JetteBear']
mesh = bpy.data.objects['JetteBear_Mesh']

# ---------------------------------------------------------------- pose helpers
def rot_world(pb, axis, deg):
    """Rotate pose bone about a WORLD axis (x/y/z) by deg, composed onto its current pose rotation.
    Uses the bone's rest orientation so the axis is meaningful regardless of bone roll."""
    ax = {'x': Vector((1, 0, 0)), 'y': Vector((0, 1, 0)), 'z': Vector((0, 0, 1))}[axis]
    Rw = Matrix.Rotation(math.radians(deg), 3, ax)
    Rr = (arm.matrix_world.to_3x3() @ pb.bone.matrix_local.to_3x3())
    Rl = Rr.inverted() @ Rw @ Rr
    pb.rotation_mode = 'QUATERNION'
    pb.rotation_quaternion = (pb.rotation_quaternion.to_matrix() @ Rl).to_quaternion()

def reset_pose():
    for pb in arm.pose.bones:
        pb.rotation_mode = 'QUATERNION'
        pb.rotation_quaternion = (1, 0, 0, 0)
        pb.location = (0, 0, 0)

def pose_sit():
    """주저앉은 곰 — 무릎 세우고 팔로 감싸 안기, 고개 숙임. (rot_world 는 나중 호출이 먼저 적용된다: 총회전 = R_a @ R_b)"""
    reset_pose()
    P = arm.pose.bones
    P['Hips'].location = (0, -0.21, 0.0)               # 엉덩이를 바닥으로(뼈 로컬 Y = 세계 +Z)
    for side in ('Left', 'Right'):
        s = 1 if side == 'Left' else -1
        rot_world(P[side + 'UpLeg'], 'x', -112)         # 허벅지 앞·위로
        rot_world(P[side + 'UpLeg'], 'z', s * 8)
        rot_world(P[side + 'Leg'], 'x', 118)            # 정강이 아래로(무릎 세움)
        rot_world(P[side + 'Arm'], 'x', 50)             # (나중 적용) 앞으로 내민 팔을 아래로
        rot_world(P[side + 'Arm'], 'z', -s * 70)        # (먼저 적용) 팔을 앞으로
        rot_world(P[side + 'ForeArm'], 'z', -s * 60)    # 무릎을 감싸듯 안으로
    rot_world(P['Spine1'], 'x', 12)                     # 상체 숙임
    rot_world(P['Head'], 'y', 4)
    rot_world(P['Head'], 'x', 9)                        # 고개 살짝 숙임 — 안전모 앞 JETTE 가 보이게

def pose_face():
    reset_pose()
    P = arm.pose.bones
    for side, s in (('Left', 1), ('Right', -1)):
        rot_world(P[side + 'Arm'], 'y', s * 78)   # 팔 내리기
    rot_world(P['Head'], 'y', -6)

# ---------------------------------------------------------------- render setup
def setup(w, h):
    for eng in ('BLENDER_EEVEE', 'BLENDER_EEVEE_NEXT', 'BLENDER_WORKBENCH'):
        try:
            scene.render.engine = eng; break
        except Exception: continue
    print('engine', scene.render.engine)
    try:
        scene.eevee.taa_render_samples = 32
    except Exception: pass
    scene.render.resolution_x = w; scene.render.resolution_y = h
    scene.render.resolution_percentage = 100
    scene.render.film_transparent = True
    scene.render.image_settings.file_format = 'PNG'; scene.render.image_settings.color_mode = 'RGBA'
    scene.view_settings.view_transform = 'Standard'
    # lights (replace whatever the rig blend had)
    for o in [o for o in bpy.data.objects if o.type in ('LIGHT', 'CAMERA')]:
        bpy.data.objects.remove(o, do_unlink=True)
    bpy.ops.object.light_add(type='SUN', location=(1.5, -2.5, 4))
    sun = bpy.context.object; sun.data.energy = 1.4; sun.data.angle = math.radians(12)
    sun.rotation_euler = Euler((math.radians(50), math.radians(10), math.radians(25)))
    bpy.ops.object.light_add(type='AREA', location=(-2.2, -2.6, 1.6))
    fill = bpy.context.object; fill.data.energy = 110; fill.data.size = 3; fill.data.color = (0.85, 0.9, 1.0)
    fill.rotation_euler = Euler((math.radians(70), 0, math.radians(-40)))
    bpy.ops.object.light_add(type='AREA', location=(0.5, 2.5, 2.2))
    rim = bpy.context.object; rim.data.energy = 70; rim.data.size = 2
    rim.rotation_euler = Euler((math.radians(-60), 0, 0))
    world = scene.world or bpy.data.worlds.new('W'); scene.world = world
    world.use_nodes = True
    bg = world.node_tree.nodes.get('Background')
    if bg: bg.inputs[0].default_value = (0.8, 0.85, 0.95, 1); bg.inputs[1].default_value = 0.25

def camera(loc, look_at, lens=55, ortho=None):
    bpy.ops.object.camera_add(location=loc)
    cam = bpy.context.object
    d = Vector(look_at) - Vector(loc)
    cam.rotation_euler = d.to_track_quat('-Z', 'Y').to_euler()
    cam.data.lens = lens
    if ortho:
        cam.data.type = 'ORTHO'; cam.data.ortho_scale = ortho
    scene.camera = cam
    return cam

def render(path):
    scene.render.filepath = path
    bpy.ops.render.render(write_still=True)
    print('PNG ->', path)

# ---------------------------------------------------------------- 1) sitting sad bear
setup(764, 855)
pose_sit()
bpy.context.view_layer.update()
cam = camera((0.22, -1.95, 0.80), (0.0, -0.05, 0.43), lens=62)   # 시안: 정면 조금 위에서 — 얼굴과 안전모 JETTE 둘 다
render(os.path.join(OUT, 'UI_RunOver_Sad.png'))
bpy.data.objects.remove(cam, do_unlink=True)

# ---------------------------------------------------------------- 2) face portraits
setup(512, 512)
pose_face()
bpy.context.view_layer.update()
cam = camera((0.0, -1.9, 0.86), (0.0, 0.0, 0.80), lens=70)
render(os.path.join(OUT, 'UI_Face_Girl.png'))
render(os.path.join(OUT, 'UI_Face_Butler.png'))
scene.render.resolution_x = 254; scene.render.resolution_y = 258
render(os.path.join(OUT, 'UI_Face_Ring.png'))
bpy.data.objects.remove(cam, do_unlink=True)

# ---------------------------------------------------------------- 3) preview (pose check, opaque)
setup(600, 800)
pose_sit(); bpy.context.view_layer.update()
scene.render.film_transparent = False
cam = camera((1.5, -1.9, 0.7), (0.0, 0.0, 0.35), lens=50)
render(os.path.join(OUT, 'bear_preview_pose.png'))
