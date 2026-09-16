import bpy
bpy.ops.wm.read_factory_settings(use_empty=True)
path=r"C:\dev\game\Assets\Resources\CoastRun\Models\Obs3_Cone.fbx"
bpy.ops.import_scene.fbx(filepath=path)
for o in bpy.data.objects:
    if o.type=="MESH":
        print(o.name, "mats", [m.name if m else None for m in o.data.materials], "dims", list(o.dimensions))
