using UnityEngine;
using System.Collections.Generic;

namespace CritterVolumetricLighting
{
	public static class ProjectionLib
	{	
		public static Vector3 FindIntersectionWithPlane(Vector3 inspectDirection, Vector3 inspectPoint, Vector3 planePoint, Vector3 planeNormal)
		{
			float denominator = Vector3.Dot(planeNormal, inspectDirection);

			if (Mathf.Approximately(denominator, 0f))
			{
				return inspectPoint;
			}
			Vector3 startToPlanePoint = planePoint - inspectPoint;
			float t = Vector3.Dot(planeNormal, startToPlanePoint) / denominator;
			Vector3 intersection = inspectPoint + inspectDirection * t;
			return intersection;
		}
		
		
		private static Vector2 ReverseEngineerUV(Vector3 positionOnPlane, Vector3 planeOrigo, Vector3 anchorPosition, Vector3 anchorForward,
		float planeWidthWorldUnits, float planeHeightWorldUnits)
		{	
			Vector3 posOnRelAxisX = FindIntersectionWithPlane(-anchorForward, positionOnPlane, anchorPosition, -anchorForward);

			Vector3 compX = (posOnRelAxisX - planeOrigo);
			Vector3 compY = (positionOnPlane - posOnRelAxisX);
			
			float compXLen = compX.magnitude;
			float compYLen = compY.magnitude;
			
			float xUV = compXLen / planeWidthWorldUnits;
			float yUV = compYLen / planeHeightWorldUnits;

			Vector2 uv = new Vector2(xUV, yUV);
			
			return uv;
		}

		public static Vector2 PositionToUV(Vector3 inputPosition, Vector3 sunForward, Vector3 anchorPosition, Vector3 anchorUp, Vector3 planeOrigo, Vector3 anchorForward,
		float planeWidthWorldUnits, float planeHeightWorldUnits)
		{
			Vector3 pointOnPlane = FindIntersectionWithPlane(-sunForward, inputPosition, anchorPosition, anchorUp);

			return ReverseEngineerUV(pointOnPlane, planeOrigo, anchorPosition, anchorForward, planeWidthWorldUnits, planeHeightWorldUnits);
		}
		
		public static Vector3 ProjectVectorOntoPlane(Vector3 vectorToProject, Vector3 planeNormal)
		{
			Vector3 projectionOntoNormal = Vector3.Dot(vectorToProject, planeNormal) * planeNormal;
			Vector3 projectionOntoPlane = vectorToProject - projectionOntoNormal;
			return projectionOntoPlane.normalized;
		}
		
		public static Vector3 ProjectPointOntoPlane(Vector3 point, Vector3 planeNormal, Vector3 planePoint)
		{
			Vector3 displacement = point - planePoint;
			float distance = Vector3.Dot(displacement, planeNormal.normalized);
			return point - planeNormal.normalized * distance;
		}
		
		public static Vector2Int CalculateLongestFrustumAxis(List<Vector3> frustumPoints)
		{
			Vector2Int longestAxis = Vector2Int.zero;
			float longestDistance = 0f;
			for (int i = 0; i <= 1; i++)
			{
				float distance2 = Vector3.Distance(frustumPoints[2+i], frustumPoints[4+i]);
				float distance3 = Vector3.Distance(frustumPoints[2+i], frustumPoints[6+i]);
				
				if (distance2 > longestDistance) {
					longestDistance = distance2;
					longestAxis = new Vector2Int(2+i, 4+i);
				}
				
				if (distance3 > longestDistance) {
					longestDistance = distance3;
					longestAxis = new Vector2Int(2+i, 6+i);
				}
			}
			
			return longestAxis;
		}
		
		static public Vector3 CalculateRightVector(Vector3 forward)
		{
			Vector3 upWorld = Vector3.up;
			Vector3 right = Vector3.Cross(forward, upWorld);
			
			if (right.sqrMagnitude < 0.0001) // If right vector is nearly zero due to parallel vectors
			{
				// Use another vector that is guaranteed not to be parallel
				right = Vector3.Cross(forward, Vector3.right);
				if (right.sqrMagnitude < 0.0001) // Check again
				{
					right = Vector3.Cross(forward, Vector3.forward);
				}
			}

			return right.normalized;
		}

		static public Vector3 CalculateUpVector(Vector3 forward, Vector3 directionalLightForward)
		{
			Vector3 right = CalculateRightVector(forward);
			Vector3 up = Vector3.Cross(right, forward);

			// Align 'up' more closely with the light's forward direction without breaking the orthogonality
			Vector3 projectedLightForward = Vector3.ProjectOnPlane(directionalLightForward, forward);
			float angle = Vector3.Angle(up, projectedLightForward);

			if (angle > 0.0001 && angle < 179.9999) // Avoid aligning if already aligned or directly opposite
			{
				up = projectedLightForward.normalized;
			}

			return up;
		}
	}
}