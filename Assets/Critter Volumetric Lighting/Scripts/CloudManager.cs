using UnityEngine;

namespace CritterVolumetricLighting
{
	public class CloudManager : MonoBehaviour
	{	
		[Header("General")]
		[SerializeField] float cloudCoverage = 0.6f;
		[SerializeField] float cloudScale = 0.025f;
		[SerializeField] float cloudChange = 0.005f; // 0.015f
		[SerializeField] Vector2 cloudMovement = new Vector2(-0.25f, 0.25f);
		[SerializeField] Vector2 cloudStep = new Vector2(13, 17);

		[Header("Visuals")]
		[SerializeField] int shades = 5;
		[SerializeField] float cloudStrength = 2f;
		[SerializeField] float brightness = -1f;
		[SerializeField] float miminumDarkness = 0.2f;
		
		[Header("Materials with clouds")]
		[SerializeField] public Material[] Materials = new Material[0];
		[SerializeField] [Tooltip(VolumetricLightResources.TT_OVERRIDEMATVALUES)] public bool overrideMatValues = true;

		[Header("Main light cookie clouds")]
		[SerializeField] [Tooltip(VolumetricLightResources.TT_COOKIECLOUDS)] public bool cookieClouds = false;
		[SerializeField] [Tooltip(VolumetricLightResources.TT_ADDITIONALCOOKIESIZE)] public Vector2 additionalCookieSize = new Vector2(20, 20);

		private VolumetricLightManager _VLManager;
		private Material mainlightCookieWriterMat;
		private bool _hasStarted = false;
		private bool _cookieCloudsUsed = false;

		/**
		Push cloud values to VLManager and also to materials if 'overrideMatValues' is true. Refresh cookie clouds if used.
		*/
		public void ApplyChanges()
		{
			if (overrideMatValues && Materials.Length > 0)
				SetMaterialValues();
			
			SyncVLManagerClouds();
			if (cookieClouds)
				MainlightCookieHandler.Refresh();

		}
		
		public void SyncDisplayChanges()
		{
			if (cookieClouds)
				MainlightCookieHandler.ApplySettings();
		}
		
		
		private void SetMaterialValues()
		{
			foreach (Material mat in Materials)
			{
				mat.SetFloat("_Cloud_Density", cloudScale);
				mat.SetVector("_Cloud_Movement", cloudMovement);
				mat.SetFloat("_Cloud_Strength", cloudStrength);
				mat.SetFloat("_Cloud_Cover", cloudCoverage);
				mat.SetFloat("_Cloud_Change", cloudChange);
				mat.SetVector("_Cloud_Step", cloudStep);
				mat.SetFloat("_Shades", shades);
				mat.SetFloat("_Brightness", brightness);
				mat.SetFloat("_MinimumDarkness", miminumDarkness);
			}
		}
		
		
		private void SyncVLManagerClouds()
		{				
			_VLManager.dataWriterAdditive = ((1.12f + (brightness / shades)) - cloudStrength)+0.005f;
			_VLManager.dataWriterMultiplier = cloudStrength;
			_VLManager.brightness = 0.25f * (-0.49f*shades+1.95f);
			
			_VLManager.lightRamps = (int)shades;	
			_VLManager.cloudCoverage = cloudCoverage;
			_VLManager.cloudScale = cloudScale;
			_VLManager.cloudChange = cloudChange;
			_VLManager.cloudMovement = cloudMovement;
			_VLManager.cloudStep = cloudStep;
		}

		void OnEnable()
		{			
			if (_hasStarted)
			{
				_VLManager.SetCloudManager(this, true);
				if (cookieClouds)
				{
					MainlightCookieHandler.Init();
				}
			}
		}


		[HideInInspector] private Vector2 previousAdditionalSize = new Vector2(-9999999, -9999999);
		void Update()
		{
			if (cookieClouds)
			{
				if (previousAdditionalSize != additionalCookieSize)
				{
					// MainlightCookieHandler.additiveAdjustment = VolumetricLightResources.GetMainlightCookieMaterialAdditiveAdjustValue(shades);

					MainlightCookieHandler.additionalCookieSizeWorldUnit = additionalCookieSize;
					MainlightCookieHandler.ApplySettings();
					previousAdditionalSize = additionalCookieSize;
				}
				
				MainlightCookieHandler.DrawCookie();
				_cookieCloudsUsed = true;
			} else if (_cookieCloudsUsed)
			{
				previousAdditionalSize = new Vector2(-9999999, -9999999);
				MainlightCookieHandler.CheckRemoveCookie();
				_cookieCloudsUsed = false;
			}
		}
		
		void OnDisable()
		{
			_VLManager.SetCloudManager(this, false);
		}


		void Start()
		{
			_VLManager = FindFirstObjectByType<VolumetricLightManager>();
			ApplyChanges();
			_hasStarted = true;
			_VLManager.SetCloudManager(this, true);
		}
	}
}