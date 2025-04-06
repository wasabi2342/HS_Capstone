#ifndef OUTLINES_INCLUDED
#define OUTLINES_INCLUDED

TEXTURE2D(_CameraDepthTexture);
SAMPLER(sampler_CameraDepthTexture);

TEXTURE2D(_CameraNormalsTexture);
SAMPLER(sampler_CameraNormalsTexture);

float GetDepth(float2 UV)
{
    return SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, UV).x;
}

float3 GetNormal(float2 UV)
{
    float3 worldNormal = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, UV).xyz;
    return mul((float3x3) UNITY_MATRIX_V, worldNormal) * 2.0 - 1.0;
}

float4 _CameraNormalsTexture_TexelSize;
void Outline_float(float2 UV, float DepthThreshold, float NormalsThreshold, float3 NormalEdgeBias, float DepthEdgeStrength, float NormalEdgeStrength, out float Outline)
{ 
    float2 texelSize = _CameraNormalsTexture_TexelSize.xy;
    
    float depth = GetDepth(UV);
    float3 normal = GetNormal(UV);
    
    float2 uvs[4];
    
    uvs[0] = UV + float2(0.0, texelSize.y);
    uvs[1] = UV - float2(0.0, texelSize.y);
    uvs[2] = UV + float2(texelSize.x, 0.0);
    uvs[3] = UV - float2(texelSize.x, 0.0);
    
    // Get Depth Edge Indicator
    float depths[4];
    
    float depthDifference = 0.0;
    [unroll]
    for (int i = 0; i < 4; i++)
    {
        depths[i] = GetDepth(uvs[i]);
        depthDifference += depth - depths[i]; // vs depths[i] - depth
    }
    float depthEdge = step(DepthThreshold, depthDifference);
    
    // Get Normal Edge Indicator
    float3 normals[4];
    float dotSum = 0.0;
    [unroll]
    for (int j = 0; j < 4; j++)
    {
        normals[j] = GetNormal(uvs[j]);
        float3 normalDiff = normal - normals[j];
        
        // Edge pixels should yield to faces closer to the bias direction.
        float normalBiasDiff = dot(normalDiff, NormalEdgeBias);
        float normalIndicator = smoothstep(-.01, .01, normalBiasDiff); // step(0, normalBiasDiff);
        
        dotSum += dot(normalDiff, normalDiff) * normalIndicator; // * depthIndicator;
    }
    float indicator = sqrt(dotSum);
    float normalEdge = step(NormalsThreshold, indicator);
    
    // Refuse normal outline if the depthEdge is negative and make it a depth edge if its above the threshold
    Outline = depthDifference < 0 ? 0 : (depthEdge > 0.0 ? (DepthEdgeStrength * depthEdge) : (NormalEdgeStrength * normalEdge));
}
#endif