# Blender batch script — 주인공·펫 심플 메시를 FBX로보냅니다.
# 사용: blender --background --python Tools/Blender/make_haneul_pet.py
# (이 PC에 Blender CLI가 없으면 Unity CoastFigureMesh 절차형 3D가 폴백으로 동작합니다.)

import bpy
import math
from pathlib import Path

OUT = Path(__file__).resolve().parents[2] / "Assets" / "_CoastRun" / "Art" / "Figures"
OUT.mkdir(parents=True, exist_ok=True)


def clear():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)


def mat(name, rgba):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    bsdf = m.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs[0].default_value = (*rgba[:3], 1.0)
    return m


def part(name, prim, loc, scale, material):
    if prim == "UV_SPHERE":
        bpy.ops.mesh.primitive_uv_sphere_add(radius=1, location=loc)
    elif prim == "CUBE":
        bpy.ops.mesh.primitive_cube_add(size=1, location=loc)
    elif prim == "CYLINDER":
        bpy.ops.mesh.primitive_cylinder_add(radius=1, depth=2, location=loc)
    else:
        bpy.ops.mesh.primitive_uv_sphere_add(radius=1, location=loc)
    ob = bpy.context.active_object
    ob.name = name
    ob.scale = scale
    if material:
        if ob.data.materials:
            ob.data.materials[0] = material
        else:
            ob.data.materials.append(material)
    return ob


def export_fbx(path):
    bpy.ops.export_scene.fbx(filepath=str(path), use_selection=False, apply_scale_options="FBX_SCALE_ALL")


def build_haneul():
    clear()
    skin = mat("Skin", (1.0, 0.82, 0.70, 1))
    hair = mat("Hair", (0.35, 0.22, 0.14, 1))
    shirt = mat("Shirt", (0.55, 0.82, 0.95, 1))
    shorts = mat("Shorts", (0.35, 0.45, 0.55, 1))
    pack = mat("Pack", (0.20, 0.55, 0.55, 1))
    part("Body", "UV_SPHERE", (0, 0.72, 0), (0.38, 0.55, 0.28), shirt)
    part("Head", "UV_SPHERE", (0, 1.28, 0), (0.42, 0.42, 0.42), skin)
    part("Hair", "UV_SPHERE", (0, 1.40, -0.02), (0.46, 0.28, 0.48), hair)
    part("Legs", "UV_SPHERE", (0, 0.28, 0), (0.36, 0.28, 0.26), shorts)
    part("Pack", "CUBE", (0, 0.85, -0.18), (0.28, 0.32, 0.12), pack)
    export_fbx(OUT / "Haneul.fbx")


def build_pet(kind, color):
    clear()
    m = mat(kind, (*color, 1))
    part("Body", "UV_SPHERE", (0, 0.22, 0), (0.4, 0.32, 0.42), m)
    part("Head", "UV_SPHERE", (0, 0.42, 0.08), (0.28, 0.28, 0.28), m)
    export_fbx(OUT / f"Pet_{kind}.fbx")


if __name__ == "__main__":
    build_haneul()
    build_pet("Sparrow", (0.85, 0.55, 0.25))
    build_pet("BlackPig", (0.25, 0.18, 0.16))
    build_pet("WildGoose", (0.55, 0.62, 0.70))
    build_pet("BikerThug", (0.95, 0.55, 0.70))
    print("Exported figures to", OUT)
