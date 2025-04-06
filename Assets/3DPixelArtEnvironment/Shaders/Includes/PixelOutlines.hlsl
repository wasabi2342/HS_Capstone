#ifndef PIXEL_OUTLINES_INCLUDED
#define PIXEL_OUTLINES_INCLUDED

#include "Outlines.hlsl"

#if SHADERGRAPH_PREVIEW
TEXTURE2D(_CameraOpaqueTexture);
SAMPLER(sampler_CameraOpaqueTexture);
#endif

float4 GetColor(float2 UV)
{
    return SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, UV);
}

void OutlineColors_float(float2 UV, float DepthThreshold, float NormalsThreshold, float3 NormalEdgeBias, float DepthEdgeStrength, float NormalEdgeStrength, out float4 Out)
{
    float Outline;
    Outline_float(UV, DepthThreshold, NormalsThreshold, NormalEdgeBias, DepthEdgeStrength, NormalEdgeStrength, Outline);
    Out = Outline * GetColor(UV);
}

#endif