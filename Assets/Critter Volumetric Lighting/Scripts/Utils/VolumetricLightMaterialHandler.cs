using UnityEngine;

namespace CritterVolumetricLighting
{
	/**
	Helper library for VolumetricLightManager.
	This class is used push values to godray materials.
	*/
	public static class VolumetricLightMaterialHandler
	{
		private static VolumetricLightManager _vLManager;
		
		public static void Init(VolumetricLightManager vLManager)	
		{
			_vLManager = vLManager;
			
			ApplySettings();
			UpkeepProperties();
			
			_vLManager.godrayMat.SetTexture("_CloudDataTexture", _vLManager.cloudDataTexture);
		}
		
		
		public static void UpkeepProperties()
		{
			UpkeepDataWriterProp();
			UpkeepGodrayMatProp();			
		}
		
		public static void DrawCloudData()
		{
			Graphics.Blit(null, _vLManager.cloudDataTexture, _vLManager.cloudDataWriterMat);
		}
		
		public static void ApplySettings()
		{
			UpdateDataWriterProp();
			UpdateGodrayMatProp();
		}
		
		//////////////////////////////////////////////////////
		//////////////////    DATA WRITER   //////////////////
		//////////////////////////////////////////////////////

		private static void UpdateDataWriterProp()
		{
			_vLManager.cloudDataWriterMat.SetFloat("_PlaneWidthWorldUnits", _vLManager.planeWidthWorldUnits);
			_vLManager.cloudDataWriterMat.SetFloat("_PlaneLengthWorldUnits", _vLManager.planeHeightWorldUnits);
			_vLManager.cloudDataWriterMat.SetFloat("_CloudScale", _vLManager.cloudScale);
			_vLManager.cloudDataWriterMat.SetFloat("_Cloud_Change", _vLManager.cloudChange);
			_vLManager.cloudDataWriterMat.SetVector("_CloudStep", _vLManager.cloudStep);
			_vLManager.cloudDataWriterMat.SetVector("_CloudMovement", _vLManager.cloudMovement);
			_vLManager.cloudDataWriterMat.SetFloat("_Ramps", _vLManager.lightRamps);
						
			_vLManager.cloudDataWriterMat.SetFloat("_DataWriterMultiplier", _vLManager.dataWriterMultiplier);
			_vLManager.cloudDataWriterMat.SetFloat("_DataWriterAdditive", _vLManager.dataWriterAdditive);
			_vLManager.cloudDataWriterMat.SetFloat("_Brigthness", _vLManager.brightness);

		}
		
		private static void UpkeepDataWriterProp()
		{	
			_vLManager.cloudDataWriterMat.SetVector("_PlaneRight", -_vLManager.anchorTrans.right);
			_vLManager.cloudDataWriterMat.SetVector("_CameraPos", _vLManager.mainCamera.transform.position);
			_vLManager.cloudDataWriterMat.SetVector("_PlaneForward", -_vLManager.anchorTrans.forward);
			float cameraSunDot = Vector3.Dot(_vLManager.mainCamera.transform.forward, _vLManager.mainLightTrans.forward);
			_vLManager.cloudDataWriterMat.SetFloat("_CloudCoverage", _vLManager.cloudCoverage);
			
			Vector3 planeMid = _vLManager.anchorTrans.position +
				_vLManager.anchorTrans.forward * _vLManager.planeHeightWorldUnits / 2;

			_vLManager.cloudDataWriterMat.SetVector("_AnchorMid", planeMid);
		}
		
		//////////////////////////////////////////////////////
		//////////////////    GODRAY MAT    //////////////////
		//////////////////////////////////////////////////////
		
		private static void UpdateGodrayMatProp()
		{
			_vLManager.godrayMat.SetFloat("_planeWidthWorldUnits", _vLManager.planeWidthWorldUnits);
			_vLManager.godrayMat.SetFloat("_planeHeightWorldUnits", _vLManager.planeHeightWorldUnits);
			_vLManager.godrayMat.SetFloat("_planeWidthPixels", _vLManager.cloudDataTexture.width);
			_vLManager.godrayMat.SetFloat("_planeHeightPixels", _vLManager.cloudDataTexture.height);
			_vLManager.godrayMat.SetFloat("_ScreenWidthWorldUnits", _vLManager.screenWidthWorldUnits);
			_vLManager.godrayMat.SetFloat("_ScreenHeightWorldUnits", _vLManager.screenHeightWorldUnits);
			_vLManager.godrayMat.SetFloat("_pixelUvLen", 1.0f / _vLManager.cloudDataTexture.height);
			_vLManager.godrayMat.SetFloat("_NearPlane", _vLManager.mainCamera.nearClipPlane);
			_vLManager.godrayMat.SetFloat("_CloudNearClipPlane", _vLManager.cloudAreaNearClipPlane);
			_vLManager.godrayMat.SetFloat("_FarPlane", _vLManager.mainCamera.farClipPlane);

			if (_vLManager.mainCamera.targetTexture != null) {
				_vLManager.godrayMat.SetFloat("ScreenWidthPixels", _vLManager.mainCamera.targetTexture.width);
				_vLManager.godrayMat.SetFloat("ScreenHeightPixels", _vLManager.mainCamera.targetTexture.height);
			} else {
				_vLManager.godrayMat.SetFloat("ScreenWidthPixels", Screen.width);
				_vLManager.godrayMat.SetFloat("ScreenHeightPixels", Screen.height);
			}

			_vLManager.godrayMat.SetFloat("_StepSize", _vLManager.shadowMarchStepSize);
			_vLManager.godrayMat.SetFloat("_Steps", _vLManager.shadowMarchSteps);
			_vLManager.godrayMat.SetColor("_GodrayColor", _vLManager.lightColor);
			_vLManager.godrayMat.SetFloat("_Cells", _vLManager.lightRamps);
			_vLManager.godrayMat.SetFloat("_ShadowRamps", _vLManager.shadowRamps);
			_vLManager.godrayMat.SetFloat("_FadeStart", _vLManager.fadeStart);
			_vLManager.godrayMat.SetFloat("_FadeStrength", _vLManager.fadeStrength);
			_vLManager.godrayMat.SetFloat("_ShadowStrength", _vLManager.shadowStrength*1.25f);
			_vLManager.godrayMat.SetFloat("_Strength", _vLManager.lightStrength);
			_vLManager.godrayMat.SetFloat("_DirectionalFading", _vLManager.directionalFading);
		}
		
		private static void UpkeepGodrayMatProp()
		{
			_vLManager.godrayMat.SetVector("_planeOrigo", _vLManager.planeOrigo);
			_vLManager.godrayMat.SetVector("_anchorForward", _vLManager.anchorTrans.forward);
			_vLManager.godrayMat.SetVector("_anchorPosition", _vLManager.anchorTrans.position);
			_vLManager.godrayMat.SetVector("_anchorUp", _vLManager.anchorTrans.up);
			_vLManager.godrayMat.SetVector("_CameraPos", _vLManager.mainCamera.transform.position);
			_vLManager.godrayMat.SetVector("_CameraForward", _vLManager.mainCamera.transform.forward);
			_vLManager.godrayMat.SetVector("_CameraRight", _vLManager.mainCamera.transform.right);
			_vLManager.godrayMat.SetVector("_CameraUp", _vLManager.mainCamera.transform.up);
			
			// Hardcoded smoothnes for directional disappearance. It is hardcoded to quite high value because smooth disappearance is difficult to achieve without running into weird clipping or premature disappearance...
			const float disappearanceSpeed = 12f; // Higher value = more snappy disappearance, lower value = smoother disappearance
			
			float cameraSunDot = 1-Vector3.Dot(_vLManager.mainCamera.transform.forward, _vLManager.mainLightTrans.forward);
			float normalizedVisibility = Mathf.Clamp01((cameraSunDot + 1) / 2.0f);
			float directionalShadowEffect = Mathf.Clamp01((normalizedVisibility - _vLManager.directionalDisappearance) * disappearanceSpeed);
			_vLManager.godrayMat.SetFloat("_DirectionalShadowEffect", directionalShadowEffect);

		}
	}
}