using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CritterVolumetricLighting
{
	/**
	Helper library for VolumetricLightManager.
	This class is responsible for the anchor transform that is used to precompute some values and angles for the godray shader. 
	*/
	public static class CloudAnchor
	{
		public static Vector3 planeOrigo = Vector3.zero;

		
		private static VolumetricLightManager _vLManager;
		private static Camera _mainCamera;
		private static Material _godrayMatRef;
		private static Transform _sunTrans;
		private static Transform _anchorTrans;

		private static Vector3 _projectionDirUvX;
		private static Vector3 _projectionDirUvY;
		private static Vector3 _sunProjectionOnScreen;

		private static float _radius;
		private static float _textureLengthWorldUnits;
		private static float _projectionWidth;
		private static float _projectionLen;
		
		
		public static void Init(Transform sunTransParam, Transform anchorTransParam, Camera mainCameraParam, float radiusParam, float textureLengthParam, ref Material godrayMat, VolumetricLightManager vLManager)
		{	
			CheckDebugReferences(true);
			_godrayMatRef = godrayMat;
			_sunTrans = sunTransParam;
			_anchorTrans = anchorTransParam;
			_mainCamera = mainCameraParam;
			_radius = radiusParam;
			_textureLengthWorldUnits = textureLengthParam;
			_vLManager = vLManager;

			UpdateAnchorScale();
		}
		
		
		public static void UpdateAnchorScale()
		{
			_anchorTrans.localScale = new Vector3(VolumetricLightResources.GetCameraDiagonalWorldUnits() / 10, 1, VolumetricLightResources.GetPlaneHeightWorldUnits() / 10);
		}
		
		public static void UpkeepAnchor()
		{
			SetAnchorRotationAndPosition();
			CalculateAnchorValues();
		}
		
		public static void DrawGodrayArea()
		{
			CheckDebugReferences();
			float screenHeightWorldUnits = _mainCamera.orthographicSize * 2;
			float screenWidthWorldUnits =  screenHeightWorldUnits * _mainCamera.aspect;
			Vector3 halfRight = _mainCamera.transform.right * screenWidthWorldUnits * 0.5f;
			Vector3 halfUp = _mainCamera.transform.up * screenHeightWorldUnits * 0.5f;
			
			Vector3 godrayNearCLipPosition = _mainCamera.transform.position 
				+ _mainCamera.transform.forward * _vLManager.cloudAreaNearClipPlane;
			Vector3 godrayFarClipPosition = _mainCamera.transform.position 
				+ _mainCamera.transform.forward * _vLManager.cloudAreaFarClipPlane;
			Vector3 cameraNearClipPosition = _mainCamera.transform.position 
				+ _mainCamera.transform.forward * _mainCamera.nearClipPlane;
			Vector3 cameraFarClipPosition = _mainCamera.transform.position 
				+ _mainCamera.transform.forward * _mainCamera.farClipPlane;


			Vector3 godrayNearClipTopLeft = godrayNearCLipPosition + halfUp - halfRight;
			Vector3 godrayNearClipTopRight = godrayNearCLipPosition  + halfUp + halfRight;
			Vector3 godrayNearClipBottomLeft = godrayNearCLipPosition - halfUp - halfRight;
			Vector3 godrayNearClipBottomRight = godrayNearCLipPosition - halfUp + halfRight;
			
			Vector3 godrayFarClipTopLeft = godrayFarClipPosition + halfUp - halfRight;
			Vector3 godrayFarClipTopRight = godrayFarClipPosition  + halfUp + halfRight;
			Vector3 godrayFarClipBottomLeft = godrayFarClipPosition - halfUp - halfRight;
			Vector3 godrayFarClipBottomRight = godrayFarClipPosition - halfUp + halfRight;
			
			Vector3 cameraNearClipTopLeft = cameraNearClipPosition + halfUp - halfRight;
			Vector3 cameraNearClipTopRight = cameraNearClipPosition  + halfUp + halfRight;
			Vector3 cameraNearClipBottomLeft = cameraNearClipPosition - halfUp - halfRight;
			Vector3 cameraNearClipBottomRight = cameraNearClipPosition - halfUp + halfRight;
			
			Vector3 cameraFarClipTopLeft = cameraFarClipPosition + halfUp - halfRight;
			Vector3 cameraFarClipTopRight = cameraFarClipPosition  + halfUp + halfRight;
			Vector3 cameraFarClipBottomLeft = cameraFarClipPosition - halfUp - halfRight;
			Vector3 cameraFarClipBottomRight = cameraFarClipPosition - halfUp + halfRight;
			
			Gizmos.color = Color.green;
			Gizmos.DrawLine(godrayNearClipTopLeft, godrayNearClipTopRight);
			Gizmos.DrawLine(godrayNearClipTopRight, godrayNearClipBottomRight);
			Gizmos.DrawLine(godrayNearClipBottomRight, godrayNearClipBottomLeft);
			Gizmos.DrawLine(godrayNearClipBottomLeft, godrayNearClipTopLeft);
			
			Gizmos.color = Color.red;
			Gizmos.DrawLine(godrayFarClipTopLeft, godrayFarClipTopRight);
			Gizmos.DrawLine(godrayFarClipTopRight, godrayFarClipBottomRight);
			Gizmos.DrawLine(godrayFarClipBottomRight, godrayFarClipBottomLeft);
			Gizmos.DrawLine(godrayFarClipBottomLeft, godrayFarClipTopLeft);
			
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(godrayNearClipTopLeft, cameraFarClipTopLeft);
			Gizmos.DrawLine(godrayNearClipTopRight, cameraFarClipTopRight);
			Gizmos.DrawLine(godrayNearClipBottomRight, cameraFarClipBottomRight);
			Gizmos.DrawLine(godrayNearClipBottomLeft, cameraFarClipBottomLeft);
			
			Gizmos.color = Color.gray;
			Gizmos.DrawLine(cameraNearClipTopLeft, cameraNearClipTopRight);
			Gizmos.DrawLine(cameraNearClipTopRight, cameraNearClipBottomRight);
			Gizmos.DrawLine(cameraNearClipBottomRight, cameraNearClipBottomLeft);
			Gizmos.DrawLine(cameraNearClipBottomLeft, cameraNearClipTopLeft);
			Gizmos.DrawLine(cameraNearClipTopLeft, godrayNearClipTopLeft);
			Gizmos.DrawLine(cameraNearClipTopRight, godrayNearClipTopRight);
			Gizmos.DrawLine(cameraNearClipBottomRight, godrayNearClipBottomRight);
			Gizmos.DrawLine(cameraNearClipBottomLeft, godrayNearClipBottomLeft);

			Gizmos.DrawLine(cameraFarClipTopLeft, cameraFarClipTopRight);
			Gizmos.DrawLine(cameraFarClipTopRight, cameraFarClipBottomRight);
			Gizmos.DrawLine(cameraFarClipBottomRight, cameraFarClipBottomLeft);
			Gizmos.DrawLine(cameraFarClipBottomLeft, cameraFarClipTopLeft);
			Gizmos.DrawLine(cameraFarClipTopLeft, godrayFarClipTopLeft);
			Gizmos.DrawLine(cameraFarClipTopRight, godrayFarClipTopRight);
			Gizmos.DrawLine(cameraFarClipBottomRight, godrayFarClipBottomRight);
			Gizmos.DrawLine(cameraFarClipBottomLeft, godrayFarClipBottomLeft);
		}

		public static void SetAnchorRotationAndPosition()
		{
			float screenHeightWorldUnits = _mainCamera.orthographicSize * 2;
			float screenWidthWorldUnits =  screenHeightWorldUnits * _mainCamera.aspect;
			Vector3 halfRight = _mainCamera.transform.right * screenWidthWorldUnits * 0.5f;
			Vector3 halfUp = _mainCamera.transform.up * screenHeightWorldUnits * 0.5f;
			
			Vector3 godrayNearCLipPosition = _mainCamera.transform.position 
				+ _mainCamera.transform.forward * _vLManager.cloudAreaNearClipPlane;
			Vector3 godrayFarClipPosition = _mainCamera.transform.position 
				+ _mainCamera.transform.forward * _vLManager.cloudAreaFarClipPlane;

			_anchorTrans.position = _mainCamera.transform.position
				+ _vLManager.cloudAreaNearClipPlane * _mainCamera.transform.forward;

			Vector3 fromLowNearToUpFar = 
					((godrayFarClipPosition - halfUp + halfRight) 
				- (godrayNearCLipPosition - halfUp + halfRight)).normalized;

			Vector3 upSameAngle = Vector3.Cross(fromLowNearToUpFar, _mainCamera.transform.right);
			Vector3 upDifferentAngle = -_vLManager.mainLightTrans.forward;
			
			float dot = 1-Vector3.Dot(upSameAngle, upDifferentAngle);
			float normalizedDot = Mathf.Clamp01((dot + 1) / 2.0f);
			
			Vector3 upVec = Vector3.Lerp(upSameAngle, upDifferentAngle, normalizedDot);
			_anchorTrans.rotation = Quaternion.LookRotation(_mainCamera.transform.forward, upVec);
		}
		
		
		private static void CheckDebugReferences(bool reset = false)
		{
			if (reset)
			{
				_vLManager = null;
				_mainCamera = null;
			}
			
			if (_vLManager == null)
			{
				_vLManager = GameObject.FindFirstObjectByType<VolumetricLightManager>();
			}
			
			if (_mainCamera == null)
			{
				Camera[] cameras = GameObject.FindObjectsByType<Camera>(FindObjectsSortMode.None);
				foreach (Camera cam in cameras)
				{
					if (cam.tag == "MainCamera")
					{
						_mainCamera = cam;
						break;
					}
				}
			}
			
		}
		
		private static void CalculateAnchorValues()
		{
			float widthWorldUnits = _radius*2;
			Vector3 origo = _anchorTrans.right * -0.5f * widthWorldUnits + _anchorTrans.position;
			_vLManager.planeOrigo = origo;
			_sunProjectionOnScreen = ProjectVectorOntoPlane(_sunTrans.forward, _mainCamera.transform.forward);

			_godrayMatRef.SetVector("_SunProjectionOnScreen", _sunProjectionOnScreen);
			_godrayMatRef.SetVector("_planeOrigo", origo);
			_godrayMatRef.SetVector("_anchorForward", _anchorTrans.forward);
			_godrayMatRef.SetVector("_anchorPosition", _anchorTrans.position);
			_godrayMatRef.SetVector("_anchorUp", _anchorTrans.up);
			_godrayMatRef.SetFloat("_planeWidthWorldUnits", VolumetricLightResources.GetCameraDiagonalWorldUnits());
			_godrayMatRef.SetFloat("_planeHeightWorldUnits", VolumetricLightResources.GetPlaneHeightWorldUnits());
			
			/////// Uncomment the line below to debug the area for which cloud data is computed ///////
			// DebugProjections(origo, widthWorldUnits);
		}
		
		
		/**
		Used to debug area for which cloud data is computed.
		This method is currently never called.
		*/
		private static void DebugProjections(Vector3 origo, float widthWorldUnits)
		{
			Vector3 bottomRight = origo + _anchorTrans.right * widthWorldUnits;
			Vector3 topLeft = origo + _anchorTrans.forward * _textureLengthWorldUnits;
			Vector3 topRight = bottomRight + _anchorTrans.forward * _textureLengthWorldUnits;
			Vector3 origoPP = ProjectionPosition(origo, _sunTrans.forward);
			Vector3 bottomRightPP = ProjectionPosition(bottomRight, _sunTrans.forward); 
			Vector3 topLeftPP = ProjectionPosition(topLeft, _sunTrans.forward);
			Vector3 yDirUvPP = (topLeftPP - origoPP).normalized;
			Vector3 xDirUvPP = (bottomRightPP - origoPP).normalized;
			_projectionWidth = (bottomRightPP-origoPP).magnitude;
			_projectionLen = (topLeftPP-origoPP).magnitude;
			_projectionDirUvX = xDirUvPP;
			_projectionDirUvY = yDirUvPP;

			// Basic debug (Projection vectors)
			Debug.DrawLine(origoPP, topLeftPP, Color.blue);
			Debug.DrawLine(origoPP, bottomRightPP, Color.blue);
			Debug.DrawLine(topLeftPP, topLeftPP+xDirUvPP*_projectionWidth, Color.blue);
			Debug.DrawLine(bottomRightPP, bottomRightPP+yDirUvPP*_projectionLen, Color.blue);
			
			// Basic debug (World vectors)
			Debug.DrawLine(origo, topLeft, Color.red);
			Debug.DrawLine(origo, bottomRight, Color.red);
			Debug.DrawLine(topLeft, topLeft+_anchorTrans.right*widthWorldUnits, Color.red);
			Debug.DrawLine(bottomRight, bottomRight+_anchorTrans.forward*_textureLengthWorldUnits, Color.red);
			Debug.DrawLine(origoPP, origo, Color.yellow);
			Debug.DrawLine(bottomRightPP, bottomRight, Color.yellow);
			Debug.DrawLine(topLeftPP, topLeft, Color.yellow);
			Debug.DrawLine(bottomRightPP+yDirUvPP*_projectionLen, bottomRight+_anchorTrans.forward*_textureLengthWorldUnits, Color.yellow);
			Debug.DrawLine(_vLManager.mainCamera.transform.position, _vLManager.mainCamera.transform.position + _sunProjectionOnScreen * 3, Color.red);
		}
		
		private static Vector3 ProjectVectorOntoPlane(Vector3 vectorToProject, Vector3 planeNormal)
		{
			Vector3 projectionOntoNormal = Vector3.Dot(vectorToProject, planeNormal) * planeNormal;
			Vector3 projectionOntoPlane = vectorToProject - projectionOntoNormal;
			return projectionOntoPlane.normalized;
		}
		
		private static Vector3 ProjectionPosition(Vector3 objPosition, Vector3 sunDirection)
		{
			Vector3 offsetVec = (objPosition.y / -sunDirection.y) * sunDirection;
			Vector3 shadowPosition = new Vector3(objPosition.x + offsetVec.x, 0, objPosition.z + offsetVec.z);
			return shadowPosition;
		}
	}
}