using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CritterVolumetricLighting
{
	public static class MainlightCookieHandler
	{
		private static Light _mainlight;
		public static Material mainlightCookieWriterMat;
		public static Vector2 additionalCookieSizeWorldUnit;
		private static RenderTexture _cookieTexture;
		private static VolumetricLightManager _vLManager;
		private static UniversalAdditionalLightData _lightData;
		private static Vector3 _targetPosition;
		
		
		private static bool hasInit = false;
		
		public static void Refresh()
		{
			if (hasInit == false)
			{
				Init();	
			} else 
			{
				UpdateCookie(false);
			}
		}
		
		public static void ApplySettings()
		{
			if (hasInit == false)
			{
				Init();
			} else 
			{
				UpdateCookie(true);
			}
		}
		
		/**
		Initialize values
		*/
		public static void Init()
		{
			MaterialCheck();
			FindMainLight();
			VlManagerCheck();
			UpdateCookie(true);
			hasInit = true;
		}
		
		public static void CheckRemoveCookie()
		{
			if (_cookieTexture != null)
			{
				_cookieTexture.Release();
				GameObject.Destroy(_cookieTexture);
				_cookieTexture = null;
			}
		}
		

		
		/**
		Called continiuously to write the data
		*/
		public static void DrawCookie()
		{
			_targetPosition = VolumetricLightResources.GetCloudShadowCenterPosition();
			_mainlight.transform.position = _targetPosition;
			
			UpkeepMaterialValues();
			Graphics.Blit(null, _cookieTexture, mainlightCookieWriterMat);
		}
		

		private static void UpkeepMaterialValues()
		{
			Transform mainCameraTrans = _vLManager.mainCamera.transform;
			mainlightCookieWriterMat.SetVector("_Cookie_Position", _targetPosition);

			mainlightCookieWriterMat.SetVector("_PlaneRight", _mainlight.transform.right);
			mainlightCookieWriterMat.SetVector("_PlaneForward", _mainlight.transform.up);
						
			mainlightCookieWriterMat.SetFloat("_PlaneWidthWorldUnits", _vLManager.planeWidthWorldUnits + additionalCookieSizeWorldUnit.x);
			mainlightCookieWriterMat.SetFloat("_PlaneLengthWorldUnits", _vLManager.planeHeightWorldUnits + additionalCookieSizeWorldUnit.y);
			mainlightCookieWriterMat.SetFloat("_CloudScale", _vLManager.cloudScale);
			mainlightCookieWriterMat.SetFloat("_Cloud_Change", _vLManager.cloudChange);
			mainlightCookieWriterMat.SetFloat("_CloudCoverage", _vLManager.cloudCoverage);
			mainlightCookieWriterMat.SetVector("_CloudStep", _vLManager.cloudStep);
			mainlightCookieWriterMat.SetVector("_CloudMovement", _vLManager.cloudMovement);
			mainlightCookieWriterMat.SetFloat("_Ramps", _vLManager.lightRamps);

			mainlightCookieWriterMat.SetFloat("_DataWriterMultiplier", _vLManager.dataWriterMultiplier);

			mainlightCookieWriterMat.SetFloat("_DataWriterAdditive", _vLManager.dataWriterAdditive);
			mainlightCookieWriterMat.SetFloat("_Brigthness", _vLManager.brightness);
		}
		
		
		private static void FindMainLight()
		{
			_mainlight = VolumetricLightResources.FindMainLight();

			if (_mainlight == null)
			{
				Debug.LogWarning("No main light found in the scene.");
				return;
			}

			_lightData = _mainlight.GetComponent<UniversalAdditionalLightData>();
		}
		
		
		private static void UpdateCookie(bool increaseAccuracy)
		{
			if (increaseAccuracy)
			{
				CheckRemoveCookie();

				float pixelLengthWorldUnits = _vLManager.planeHeightWorldUnits / _vLManager.cloudDataResolution;
				float pixelWidthWorldUnits = _vLManager.planeWidthWorldUnits / VolumetricLightResources.GetTextureWidth(_vLManager.cloudDataResolution);
				
				Vector2Int additionalCookieSizePixel = new Vector2Int(Mathf.CeilToInt(additionalCookieSizeWorldUnit.x / pixelWidthWorldUnits), Mathf.CeilToInt(additionalCookieSizeWorldUnit.y / pixelLengthWorldUnits));
				_cookieTexture = VolumetricLightResources.CreateCookieMap(additionalCookieSizePixel);
				
				_mainlight.cookie = _cookieTexture;
			}
			
			_lightData.lightCookieSize = new Vector2(_vLManager.planeWidthWorldUnits + additionalCookieSizeWorldUnit.x, _vLManager.planeHeightWorldUnits + additionalCookieSizeWorldUnit.y);
			_lightData.lightCookieOffset = new Vector2(0, 0);
		}
		

		private static void MaterialCheck()
		{
			if (mainlightCookieWriterMat == null)
			{
				mainlightCookieWriterMat = Resources.Load<Material>("MainlightCookieWriter");
			}	
		}
		
		
		private static void VlManagerCheck()
		{
			if (_vLManager == null)
			{
				_vLManager = (VolumetricLightManager)Object.FindFirstObjectByType(typeof(VolumetricLightManager));
				if (_vLManager == null)
				{
					Debug.LogError("VolumetricLightManager not found in the scene.");
				}
			}
		}
	}
}
