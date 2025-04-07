#include "Noise/CloudNoise.hlsl"
#include "ProjectionLib.hlsl"

/**
Samples cloud data around an individual uv. 

Takes into consideration the difference in how accurately shadows are sampled and how accurately clouds CAN BE sampled.
Samples clouds as accurately as possible by sampling them around the targetUV based on which shadows are sampled.

		Sampling clouds is significantly cheaper than sampling Unity shadows.
		In addition, always sampling clouds accurately facilitates the use of low accuracy for shadow sampling
		which has significant boost on performance.
*/
float CloudDataUvSplitSample(Texture2D DataTexture, float2 firstUv, float2 targetUv, float2 shadowStep, float2 pixelStep,
	float planeWidthPixels, float planeHeightPixels)
{
    // Calculate the number of pixel steps that fit within one shadow step, rounded to the nearest larger odd number
    uint pixelStepsInShadowStep = max(floor(length(shadowStep) / length(pixelStep)), 1);
	pixelStepsInShadowStep++;
	if (pixelStepsInShadowStep % 2 == 0) {
		pixelStepsInShadowStep += 1;
	}
    pixelStepsInShadowStep = max(1, pixelStepsInShadowStep);

    float minValue = 100000;

    int halfSteps = (pixelStepsInShadowStep - 1) / 2;

    for (int i = -halfSteps; i <= halfSteps; i++) {

        float2 sampleUv = targetUv + pixelStep * i;
		sampleUv = float2(sampleUv.x, max(firstUv.y, sampleUv.y));

        float sampleValue =	1-DataTexture[float2(sampleUv.x*planeWidthPixels, sampleUv.y*planeHeightPixels)].r;

		minValue = min(minValue, sampleValue);
    }

    return minValue;
}


/**
Sample on position on the cloud data texture.
*/
float CloudDataWorldPosSample(Texture2D DataTexture, SamplerState ShadowSampler,
	float3 sampledWorldPos, float3 sunForward, float3 anchorPosition, float3 anchorUp, float3 planeOrigo, float3 anchorForward,
	float planeWidthWorldUnits, float planeHeightWorldUnits)
{
	float2 uv = PositionToUV(sampledWorldPos, sunForward, anchorPosition,
		anchorUp, planeOrigo, anchorForward,
		planeWidthWorldUnits, planeHeightWorldUnits);

	uv = float2(1 - uv.x, 1 - uv.y);
	float sampledVal = DataTexture.Sample(ShadowSampler, uv).r;

	return 1 - (sampledVal);
}