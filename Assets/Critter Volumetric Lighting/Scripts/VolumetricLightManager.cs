using UnityEngine;

namespace CritterVolumetricLighting
{
	/**
	This script is the interface for godray values.
	*/
	public class VolumetricLightManager : MonoBehaviour
	{		
		[Header("Light Settings")]
		[SerializeField] [Tooltip(VolumetricLightResources.TT_LIGHTCOLOR)]
		public Color lightColor = new Color(1f, 1f, 0.825f, 0);
		[SerializeField] [Tooltip(VolumetricLightResources.TT_LIGHTSTRENGTH)]
		public float lightStrength = 1f;
		[HideInInspector] public int lightRamps = 5;
		
		[Header("Fade Settings")]
		[SerializeField] [Tooltip(VolumetricLightResources.TT_FADESTART)]
		public float fadeStart = 0.4f;
		[SerializeField] [Tooltip(VolumetricLightResources.TT_FADESTRENGTH)]
		public float fadeStrength = 0.33f;
		[SerializeField] [Tooltip(VolumetricLightResources.TT_DIRECTIONALFADING)]
		public float directionalFading = 1f;
		
		[Header("Shadow Settings")]
		[SerializeField] [Tooltip(VolumetricLightResources.TT_SHADOWSTRENGTH)]
		public float shadowStrength = 0.6f;
		[SerializeField] [Tooltip(VolumetricLightResources.TT_SHADOWRAMPS)]
		public int shadowRamps = 5;

		[Header("Cloud display conditions")]
		[SerializeField] [Tooltip(VolumetricLightResources.TT_DIRECTIONALDISAPPEARANCE)]
		public float directionalDisappearance = 0.725f;
		[SerializeField] [Tooltip(VolumetricLightResources.TT_CLOUDAREANEARCLIPPLANE)]
		public float cloudAreaNearClipPlane = 5;
		[SerializeField] [Tooltip(VolumetricLightResources.TT_CLOUDAREAFARCLIPPLANE)]
		public float cloudAreaFarClipPlane = 65f; // 50
		[SerializeField] [Tooltip(VolumetricLightResources.TT_SHOWCLOUDAREAINEDITOR)]
		public bool showCloudAreaInEditor = true;

		[Header("Quality settings")]
		[Tooltip(VolumetricLightResources.TT_STEPSIZE)] [SerializeField]
		public float shadowMarchStepSize = 0.18f; // 0.33
		[Tooltip(VolumetricLightResources.TT_SHADOWSTEPS)] [SerializeField]
		public int shadowMarchSteps = 48; // 24
		[Tooltip(VolumetricLightResources.TT_CLOUDDATARESOLUTION)] [SerializeField]
		public int cloudDataResolution = 700; // 700
		
		[Header("Apply changes")]
		[Tooltip(VolumetricLightResources.TT_AUTOAPPLYSETTINGS)] [SerializeField]
		public bool autoApplySettings = true;

		////// Non-setting properties //////

		// Cloud manager
		private CloudManager _cloudManager;
		
		// Render textures
		[HideInInspector] public RenderTexture _preComputedCloudTexture;
		[HideInInspector] public RenderTexture cloudDataTexture;
		

		// Materials
		[HideInInspector] public Material godrayMat;
		[HideInInspector] public Material cloudDataWriterMat;
		
		// Other properties
		[HideInInspector] public Camera mainCamera;
		[HideInInspector] public Transform mainLightTrans;
		[HideInInspector] public Transform anchorTrans;

		[HideInInspector] public Vector3 planeOrigo;
		[HideInInspector] public Vector2 cloudMovement;
		[HideInInspector] public Vector2 cloudStep;

		[HideInInspector] public float planeWidthWorldUnits;
		[HideInInspector] public float planeHeightWorldUnits;
		[HideInInspector] public float screenWidthWorldUnits;
		[HideInInspector] public float screenHeightWorldUnits;
		[HideInInspector] public float cloudCoverage;
		[HideInInspector] public float cloudScale;
		[HideInInspector] public float cloudChange;
		
		[HideInInspector] public float dataWriterMultiplier = 2.7f;
		[HideInInspector] public float dataWriterAdditive = 0f;
		[HideInInspector] public float brightness = 0.25f;
		
		private float _initializedOrthoSize;
		private float _initializedAspectRatio;
		private bool _cloudManagerExists = false;
		
		////// Unity methods //////
		
		void Awake()
		{
			VolumetricLightResources.Init(this);
			
			anchorTrans = new GameObject("CloudAnchor").transform;
			anchorTrans.SetParent(transform);
			
			cloudDataWriterMat = Resources.Load<Material>("CloudDataWriterMat");
			godrayMat = Resources.Load<Material>("VolumetricScreenSpaceLightingMat");

			InitializeVolumetricLights();
		}

		void OnEnable()
		{
			godrayMat.SetFloat("_Enable", 1);
		}
		void OnDisable()
		{
			godrayMat.SetFloat("_Enable", 0);
		}
		

		void Update()
		{				
			if (autoApplySettings)
				ApplySettings();
		
			// Ensure that values are correct
			ValidateCriticalValues();
			CheckDisplayChanges();
			CloudAnchor.UpkeepAnchor();
			
			// Draw cloud data
			VolumetricLightMaterialHandler.UpkeepProperties();
			VolumetricLightMaterialHandler.DrawCloudData();

			// Draw precomputed buffer
			PreComputedCloudBufferHandler.RunCS();
		}
		
		
		
		////// Public methods //////
		
		/**
		Applies godray and cloud settings.
		*/
		public void ApplySettings()
		{
			planeWidthWorldUnits = VolumetricLightResources.GetCameraDiagonalWorldUnits();
			planeHeightWorldUnits = VolumetricLightResources.GetPlaneHeightWorldUnits();
			
			if (_cloudManagerExists) {
				_cloudManager.ApplyChanges();
			}
			
			VolumetricLightMaterialHandler.ApplySettings();
		}
		
		/**
		Called by CloudManager to inform VolumetricLightManager if clouds should be computed or not.
		*/
		public void SetCloudManager(CloudManager cloudManager, bool exists)
		{
			_cloudManagerExists = exists;
			if (_cloudManagerExists)
			{
				_cloudManager = cloudManager;
				godrayMat.EnableKeyword("_USECLOUDS");
			} else 
			{
				godrayMat.DisableKeyword("_USECLOUDS");
			}
		}
		
		/**
		Creates rendertextures and computes constant values from scratch.
		This is used when the scene is loaded or when the screen aspect ratio or resolution changes at runtime for example.
		*/
		public void InitializeVolumetricLights()
		{	
			mainCamera = Camera.main;		
			if (cloudDataTexture != null) {
				cloudDataTexture.Release();
				cloudDataTexture = null;
			}
			
			cloudDataTexture = VolumetricLightResources.CreateCloudDataMap();
			mainLightTrans = VolumetricLightResources.FindMainLightTrans();
			
			planeWidthWorldUnits = VolumetricLightResources.GetCameraDiagonalWorldUnits();
			planeHeightWorldUnits = VolumetricLightResources.GetPlaneHeightWorldUnits();
			screenHeightWorldUnits = mainCamera.orthographicSize * 2;
			screenWidthWorldUnits =  screenHeightWorldUnits * mainCamera.aspect;
		
			// Ready to initialize support classes
			_preComputedCloudTexture = PreComputedCloudBufferHandler.Init(ref cloudDataTexture, this);
			CloudAnchor.Init(mainLightTrans, anchorTrans, mainCamera, VolumetricLightResources.GetCameraDiagonalWorldUnits() / 2, VolumetricLightResources.GetPlaneHeightWorldUnits(), ref godrayMat, this);
			VolumetricLightMaterialHandler.Init(this);
			
			_initializedOrthoSize = mainCamera.orthographicSize;
			_initializedAspectRatio = mainCamera.aspect;
			ApplySettings();
		}
		
		
		
		////// Private methods //////
		
		private void ValidateCriticalValues()
		{
			shadowMarchStepSize = Mathf.Max(0.01f, shadowMarchStepSize);
			cloudDataResolution = Mathf.Max(64, cloudDataResolution);
		}
		
		private void CheckDisplayChanges()
		{
			bool resetRequired = _initializedOrthoSize != mainCamera.orthographicSize 
									|| _initializedAspectRatio != mainCamera.aspect
									|| cloudDataTexture.height != cloudDataResolution;
									
			if (resetRequired) {
				InitializeVolumetricLights();

				if (_cloudManagerExists) {
					_cloudManager.SyncDisplayChanges();
				}
			}
			
		}
			
		private void OnDrawGizmos()
		{
			if (showCloudAreaInEditor) {
				CloudAnchor.DrawGodrayArea();
			}
		}

	}
}
