#ifndef CLOUD_NOISE_INCLUDED
#define CLOUD_NOISE_INCLUDED

#include "SimplexNoise3D.hlsl"

void CloudNoise_half(float2 UV, float Scale, float VerticalSpeed, float2 Step, float Coverage, float Time, out float Sample)
{
    // FBX Calculations (Lacunarity, Octaves, Amplitude)
    float n = snoise(float3(UV * Scale, Time * VerticalSpeed));
    n += 0.5 * snoise(float3((UV * 2.0 - Step) * Scale, Time * VerticalSpeed));
    n += 0.25 * snoise(float3((UV * 4.0 - 2.0 * Step) * Scale, Time * VerticalSpeed));
    n += 0.125 * snoise(float3((UV * 8.0 - 3.0 * Step) * Scale, Time * VerticalSpeed));
    n += 0.0625 * snoise(float3((UV * 16.0 - 4.0 * Step) * Scale, Time * VerticalSpeed));
    n += 0.03125 * snoise(float3((UV * 32.0 - 5.0 * Step) * Scale, Time * VerticalSpeed));
    
    Sample = Coverage + 0.5 * n;
    return;
}
void CloudNoise_float(float2 UV, float Scale, float VerticalSpeed, float2 Step, float Coverage, float Time, out float Sample)
{
    // FBX Calculations (Lacunarity, Octaves, Amplitude)
    float n = snoise(float3(UV * Scale, Time * VerticalSpeed));
    n += 0.5 * snoise(float3((UV * 2.0 - Step) * Scale, Time * VerticalSpeed));
    n += 0.25 * snoise(float3((UV * 4.0 - 2.0 * Step) * Scale, Time * VerticalSpeed));
    n += 0.125 * snoise(float3((UV * 8.0 - 3.0 * Step) * Scale, Time * VerticalSpeed));
    n += 0.0625 * snoise(float3((UV * 16.0 - 4.0 * Step) * Scale, Time * VerticalSpeed));
    n += 0.03125 * snoise(float3((UV * 32.0 - 5.0 * Step) * Scale, Time * VerticalSpeed));
    
    Sample = Coverage + 0.5 * n;
    return;
}

#endif