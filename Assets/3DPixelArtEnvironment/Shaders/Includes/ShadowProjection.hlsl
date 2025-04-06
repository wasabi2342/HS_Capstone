void ShadowProjection_float(float3 objPos, float3 SunDir, out float2 shadowPos) {
    float multiplier = (objPos.y / -SunDir.y);
    float3 offsetVec = multiplier * SunDir;
    shadowPos = float2(objPos.x + offsetVec.x, objPos.z + offsetVec.z);
}
