#include "Noise/CloudNoise.hlsl"
#include "CloudDataSampler.hlsl"
#include "URP_CustomLighting/CustomLighting.hlsl"

void RampValue_float(float Ramps, float value, float brigthness, out float outputValue){
	float temp = value * Ramps;
	float r = round(temp + brigthness);
	outputValue = r / Ramps;
}

float RampValue(float Ramps, float value){
	float r = round(value * Ramps);
	return r / Ramps;
}

/**
Godrays from Unity shadows only.
*/
void ShadowDataGodrays_float(float3 WorldPos, int TotalSteps, float3 Step,
	float shadowStrength, float Ramps,
	out float ShadowAtten)
{
	float3 currentPos = WorldPos;
	float cumulativeShadowAtten = 0;
	float shadowMultiplier = ((length(Step)) / 0.5f) / TotalSteps;

	for (int i = 0; i < TotalSteps; i++) 
	{
		float ShadowAttenuationHere = 0;

		MainLightShadows_float(currentPos, half4(1, 1, 1, 1), ShadowAttenuationHere);
		cumulativeShadowAtten += shadowMultiplier * (1 - ShadowAttenuationHere);
		currentPos += Step;
	}

	ShadowAtten = RampValue(Ramps, cumulativeShadowAtten * shadowStrength);
}


float2 SnapUvY(float2 uv, float snapValue){
	float snappedUvY = ceil(uv.y * snapValue) / snapValue;
	return float2(uv.x, snappedUvY);
}

float GetRandomValue(float2 position, float randomStrength) {
    float random = frac(sin(dot(position, float2(12.9898, 78.233))) * 43758.5453);
	return (random - 0.5)*randomStrength;
}

/**
Godrays from simulated clouds and Unity's shadows both in such way that they combine.
This method is the core of the entire system and is the most sophisticated part of the shader.

@param[out] GodrayValue[0, 1] - the final godray value for this pixel 
*/
void CombinedGodrays_float(float3 WorldPos, float3 pixelWorldPositionOnNearClipPlane, int RaymarchSteps, float3 Step,
	Texture2D CloudDataTexture, float pixelLengthInUv,
	float3 planeOrigo, float3 anchorForward, float3 anchorPosition, float3 anchorUp, float3 sunForward,
	float planeWidthWorldUnits, float planeHeightWorldUnits, float planeWidthPixels, float planeHeightPixels, float depth,
	float shadowStrength, float Ramps, float ShadowRamps,
	Texture2D PreCalcTexture, float PreCalcN, float PreCalcWorldUnitSizeN,

	out float GodrayValue)
{
	float minPixelValue = 1000000;
	float cumulativeShadow = 0;

	float3 currentPos = WorldPos;
	float2 currentUV = GetUvOnCloudTexture(currentPos, sunForward, anchorPosition, anchorUp, planeOrigo, anchorForward, planeWidthWorldUnits, planeHeightWorldUnits);
	float2 stepUV = GetUvOnCloudTexture(currentPos + Step, sunForward, anchorPosition, anchorUp, planeOrigo, anchorForward, planeWidthWorldUnits, planeHeightWorldUnits) 
			- currentUV;

	float2 firstUv = currentUV;

	float2 pixelStep = normalize(stepUV) * pixelLengthInUv;
	float shadowMultiplier = ((length(Step)) / 0.5f) / RaymarchSteps;

	// First step: Unity shadow raymarch
	RaymarchSteps = min(RaymarchSteps, 512); // Can prevent compile crash
	for (int i = 0; i < RaymarchSteps; i++)
	{
		float AmountOfLightHere = 0;

		// float3 randomizedShadowPos = currentPos + Step * GetRandomValue(currentPos.xy, 0.1f);
		MainLightShadows_float(currentPos, half4(1, 1, 1, 1), AmountOfLightHere);

		float unityShadowHere = 1 - AmountOfLightHere;

		float CloudShadowHere = CloudDataUvSplitSample(CloudDataTexture, firstUv, currentUV, stepUV, pixelStep, planeWidthPixels, planeHeightPixels);

		cumulativeShadow += unityShadowHere * shadowMultiplier;

		float actualShadowHere = CloudShadowHere + unityShadowHere;
		minPixelValue = min(minPixelValue, actualShadowHere);

		currentPos += Step;
		currentUV += stepUV;
	}
	
	// currentUV -= stepUV * 0.5f ; // Technically this goes slightly less than half step too far backwards, but it does not matter.
	float stepAdjustmentFactor = RaymarchSteps / (RaymarchSteps + 1e-6);
	currentUV -= stepUV * 0.5f * stepAdjustmentFactor;
	
	// Prepare values for steps the second and third steps...
	float2 preCalcUvStep = float2(stepUV.x, normalize(stepUV).y * (1/PreCalcN));
	float2 firstCorrectSnappedUv = SnapUvY(firstUv + preCalcUvStep*2, PreCalcN);
	float2 thisSnappedUv = SnapUvY(currentUV, PreCalcN);
	float2 targetSnappedUv = max(firstCorrectSnappedUv, thisSnappedUv);
	float2 hereToNextUv = targetSnappedUv - currentUV;


	// Second step: Bridge the gap between Unity shadow raymarch and precomputed texture iteration.
	float stepsToNextAlignedY = length(hereToNextUv) / length(pixelStep);
	stepsToNextAlignedY = min(stepsToNextAlignedY, 512); // Can prevent compile crash
	for (int j = 0; j < stepsToNextAlignedY; j++)
	{
		float CloudShadowHere = 1 - CloudDataTexture[float2(currentUV.x * planeWidthPixels, currentUV.y * planeHeightPixels)].r;
		minPixelValue = min(minPixelValue, CloudShadowHere);
		currentUV += pixelStep;
	}


	// Third step: Pre calc raymarch
	float requiredIterations = PreCalcN * (1-currentUV.y);
	requiredIterations = min(requiredIterations, 256); // Can prevent compile crash
	for (int k = 0; k < requiredIterations; k++)
	{
		float preCalcValue = 1-PreCalcTexture[float2(currentUV.x*planeWidthPixels, currentUV.y*PreCalcN)].r;
		minPixelValue = min(minPixelValue, preCalcValue);
		currentUV += preCalcUvStep;
	}


	// Final step: Process the results
	GodrayValue = minPixelValue; // Godray value for this pixel

	GodrayValue = RampValue(Ramps, GodrayValue); // Stylize godray
	GodrayValue = clamp(GodrayValue, 0, 1); // Clamp it so shadows can be applied properly
	
	cumulativeShadow = RampValue(ShadowRamps, cumulativeShadow * shadowStrength); // Stylize shadows
	GodrayValue += (cumulativeShadow * shadowStrength); // Apply shadows on top of godray for stylistic reasons
}





float3 WorldPosToShadowProjectionPos(float3 objPosition, float3 sunDirection) {
    float3 offsetVec = (objPosition.y / -sunDirection.y) * sunDirection;
    float3 shadowPosition = float3(objPosition.x + offsetVec.x, 0, objPosition.z + offsetVec.z);
    return shadowPosition;
}

/**
Takes in world position, gives out freshly computed cloud value. Not recommended to be used in raymarch.
This is used for constructing the cloud data texture.

@param[in] worldPos - world position whose cloud data will be computed
@param[out] sampleValue - the computed cloud data value
*/
void CloudDataFromWorldPos_float(float3 worldPos, float3 sunAngle, float scale,
	float cloudChange, float2 noiseStep, float coverage, float2 cloudMovement,
	float time,
	out float sampleValue)
{
	float3 shadowPosition = WorldPosToShadowProjectionPos(worldPos, sunAngle);
	float2 uv = float2(shadowPosition.x, shadowPosition.z);
	uv += cloudMovement * time;
	FastCloudNoise(uv, scale, cloudChange, noiseStep, coverage, time, sampleValue);
}
