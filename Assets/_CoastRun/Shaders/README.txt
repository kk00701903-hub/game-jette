# Shaders moved (82차)
# Custom CoastRun shaders live under Assets/Resources/CoastRun/Shaders/
# so Android APK always includes them for Shader.Find at runtime.
#
# Also listed in Project Settings > Graphics > Always Included Shaders:
#   CoastRun/ChromaUnlit, UnlitCurved, ToonLit, InkOutline, UIDesaturate
#   URP Lit, Simple Lit, Unlit, Particles/Unlit
#
# Runtime Material creation must go through CoastMaterials.Require / NewMat /
# CreateParticle — never `new Material(Shader.Find(...))` without a null check.
