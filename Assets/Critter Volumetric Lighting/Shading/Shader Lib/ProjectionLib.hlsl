float3 FindIntersectionWithPlane(float3 inspectDirection, float3 inspectPoint, float3 planePoint, float3 planeNormal)
{
    float denominator = dot(planeNormal, inspectDirection);

            // In theory, this check is good practice but we should avoid 'if' statements in shaders.
            // In rigorous testing, the check did not seem to make a difference so therefore it was commented out.
    // if (abs(denominator) < 0.00001f) {
    //     return inspectPoint;
    // }

    float3 startToPlanePoint = planePoint - inspectPoint;
    float t = dot(planeNormal, startToPlanePoint) / denominator;
    float3 intersection = inspectPoint + inspectDirection * t;
    return intersection;
}

/**
Custom tailored function to reverse engineer UV coordinates from a given position on a plane,
based on how they were placed there in the first place within the ShaderGraph that draws the rendertexture.
*/
float2 ReverseEngineerUV(float3 positionOnPlane, float3 planeOrigo, float3 anchorPosition, float3 anchorForward,
    float planeWidthWorldUnits, float planeHeightWorldUnits)
{	
    float3 posOnRelAxisX = FindIntersectionWithPlane(-anchorForward, positionOnPlane, anchorPosition, -anchorForward);

    float3 compX = (posOnRelAxisX - planeOrigo);
    float3 compY = (positionOnPlane - posOnRelAxisX);

    float compXLen = length(compX);
    float compYLen = length(compY);
    
    float xUV = compXLen / planeWidthWorldUnits;
    float yUV = compYLen / planeHeightWorldUnits;

    float2 uv = float2(xUV, yUV);
    return uv;
}

float2 PositionToUV(float3 inputPosition, float3 sunForward, float3 anchorPosition, float3 anchorUp, float3 planeOrigo, float3 anchorForward,
    float planeWidthWorldUnits, float planeHeightWorldUnits)
{
    float3 pointOnPlane = FindIntersectionWithPlane(-sunForward, inputPosition, anchorPosition, anchorUp);

    return ReverseEngineerUV(pointOnPlane, planeOrigo, anchorPosition, anchorForward, planeWidthWorldUnits, planeHeightWorldUnits);
}

void ProjectVectorOntoPlane_float(float3 vectorToProject, float3 planeNormal, out float3 projectionOnPlane)
{
    float3 projectionOntoNormal = dot(vectorToProject, planeNormal) * planeNormal;
    float3 projectionOntoPlane = vectorToProject - projectionOntoNormal;
    projectionOnPlane = normalize(projectionOntoPlane);
}


float2 GetUvOnCloudTexture(float3 sampledWorldPos, float3 sunForward, float3 anchorPosition, float3 anchorUp, float3 planeOrigo, float3 anchorForward,
	float planeWidthWorldUnits, float planeHeightWorldUnits)
{
	float2 uv = PositionToUV(sampledWorldPos, sunForward, anchorPosition,
			anchorUp, planeOrigo, anchorForward,
			planeWidthWorldUnits, planeHeightWorldUnits);

	return float2(1 - uv.x, 1 - uv.y);
}