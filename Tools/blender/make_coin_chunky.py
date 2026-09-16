import bpy, math, os
bpy.ops.object.select_all(action="SELECT"); bpy.ops.object.delete(use_global=False)
for b in list(bpy.data.meshes): bpy.data.meshes.remove(b)
for b in list(bpy.data.materials): bpy.data.materials.remove(b)

def make_coin(name, rgb, metal=0.95, rough=0.2):
    bpy.ops.mesh.primitive_cylinder_add(vertices=48, radius=0.5, depth=0.42, location=(0,0,0))
    coin = bpy.context.active_object; coin.name = name
    coin.rotation_euler = (0, math.radians(90), 0); bpy.ops.object.transform_apply(rotation=True)
    bev = coin.modifiers.new("Bevel","BEVEL"); bev.width=0.025; bev.segments=3; bev.limit_method="ANGLE"
    bpy.ops.object.modifier_apply(modifier="Bevel")
    def star(x, flip):
        bpy.ops.mesh.primitive_circle_add(vertices=10, radius=0.28, fill_type="NGON", location=(x,0,0))
        s = bpy.context.active_object
        s.rotation_euler=(0,math.radians(90),0); bpy.ops.object.transform_apply(rotation=True)
        for i,v in enumerate(s.data.vertices):
            if i%2: v.co.y*=0.42; v.co.z*=0.42
        bpy.ops.object.mode_set(mode="EDIT"); bpy.ops.mesh.select_all(action="SELECT")
        bpy.ops.mesh.extrude_region_move(TRANSFORM_OT_translate={"value":((-0.05 if flip else 0.05),0,0)})
        bpy.ops.object.mode_set(mode="OBJECT"); return s
    a,b = star(0.21,False), star(-0.21,True)
    bpy.ops.object.select_all(action="DESELECT"); coin.select_set(True); a.select_set(True); b.select_set(True)
    bpy.context.view_layer.objects.active=coin; bpy.ops.object.join()
    bpy.ops.object.origin_set(type="ORIGIN_GEOMETRY", center="BOUNDS"); coin.location=(0,0,0)
    mat=bpy.data.materials.new(name+"_Mat"); mat.use_nodes=True
    bsdf=mat.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value=(*rgb,1)
    if "Metallic" in bsdf.inputs: bsdf.inputs["Metallic"].default_value=metal
    if "Roughness" in bsdf.inputs: bsdf.inputs["Roughness"].default_value=rough
    coin.data.materials.clear(); coin.data.materials.append(mat)
    return coin

gold=make_coin("Coin_Gold",(1.0,0.82,0.2)); silver=make_coin("Coin_Silver",(0.82,0.88,0.96),0.88,0.26)
root=r"C:\dev\game\Assets\Resources\CoastRun\Models"
for obj,fn in ((gold,"Coin_Gold.fbx"),(silver,"Coin_Silver.fbx")):
    bpy.ops.object.select_all(action="DESELECT"); obj.select_set(True); bpy.context.view_layer.objects.active=obj
    bpy.ops.export_scene.fbx(filepath=os.path.join(root,fn), use_selection=True, apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z", axis_up="Y", bake_space_transform=True, object_types={"MESH"}, use_mesh_modifiers=True,
        add_leaf_bones=False, bake_anim=False)
    print(fn, [round(x,3) for x in obj.dimensions])
