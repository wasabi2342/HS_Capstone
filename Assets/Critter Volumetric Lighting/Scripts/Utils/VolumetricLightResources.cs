using UnityEngine;

namespace CritterVolumetricLighting
{
	static public class VolumetricLightResources
	{
		// Tooltips
		public const string TT_LIGHTCOLOR = "Color of volumetric light.";
		public const string TT_LIGHTSTRENGTH = "Strength of volumetric light.";
		
		public const string TT_FADESTART = "Distance from the light source where volumetric light begins to fade.";
		public const string TT_FADESTRENGTH = "Strength of the fade effect of volumetric light.";
		public const string TT_DIRECTIONALFADING = "Controls fading of light in the direction of the light source.";

		public const string TT_SHADOWSTRENGTH = "Strength of Unity shadows. Note that clouds are always fully occluded by objects. This value can be used for stylized visuals where shadows are shown always on top of volumetric light.";
		public const string TT_SHADOWRAMPS = "Number of distinct 'ramps' for shadows.";

		public const string TT_DIRECTIONALDISAPPEARANCE = "How light reacts to the angle between camera and light source. Higher values cause the light to disappear sooner as the camera angle approaches the main light angle."+
			" This value prevents situations in which beams of light would 'go through' the screen causing weird clipping. This also mimics the natural behaviour of light from the observer's perspective in relation to the sun.";
		public const string TT_CLOUDAREANEARCLIPPLANE = "Distance from the camera where calculations for clouds begin. The visual appearance of clouds is also affected by 'Cloud Display Distance Threshold'.\n\nNote that the shorter the gap is between the near and far clip planes for clouds, the better the performance and quality will be.";
		public const string TT_CLOUDAREAFARCLIPPLANE = "Distance from the camera beyond which clouds are no longer calculated.\n\nNote, that the shorter the gap is between the near and far clip planes for clouds, the better the performance and quality will be.";
		public const string TT_SHOWCLOUDAREAINEDITOR = "Visualize the area between the near and far clip planes for clouds in editor.";

		public const string TT_STEPSIZE = "Length of an individual step in world units when sampling Unity's mainlight shadows. Shorter steps mean more accurate shadow sampling but the shadows might not be sampled very far away from the ground.\n\nNote that this value does not affect accuracy of simulated clouds, which are always displayed as accurately as possible.";
		public const string TT_SHADOWSTEPS = "How many times are Unity's shadows sampled in a raymarch. This value has a significant effect on performance. The raymarch always begins at the rendered object and moves towards the camera.";
		public const string TT_CLOUDDATARESOLUTION = "Resolution of cloud data."+
		 " A higher value results in more detailed clouds but at a higher performance cost."
		 +" Instead of increasing resolution, consider shortening the gap between 'Cloud Near Clip Plane' and 'Cloud Far Clip Plane' for better visuals. This area can be visualized in editor by enabling the 'Show Cloud Area In Editor' checkbox. Minimum value is 64.";
		
		public const string TT_AUTOAPPLYSETTINGS = "Apply settings on every frame."+
			" Applying settings on every frame comes with a slight performance cost but is useful for visual feedback when adjusting volumetric light values.";
		
		public const string TT_OVERRIDEMATVALUES = "When settings are applied, should materials' cloud values be set with values defined in this script.";
		
		public const string TT_COOKIECLOUDS = "Adds a cookie to mainlight which casts the generated clouds. NOTE: this functionality will take over the position of your main light, which in most cases is irrelevant.";
		public const string TT_ADDITIONALCOOKIESIZE = "This value can be used to enlarge main light's cookie size. This does not affect the 3D God rays in any way, only the cookie size.";
		
		private static VolumetricLightManager _VLManager;
		
		public static void Init(VolumetricLightManager vLManager)
		{
			_VLManager = vLManager;
		}

		/**
		Returns "Ideal N" which represents the number of splits in Pre Computed Cloud Data texture
		that minimizes the total number of iterations in raymarch.
		*/
		public static int GetIdealN(int textureHeight)
		{
			float nIdealF = Mathf.Sqrt(textureHeight * 1.5f);
			int nIdeal = Mathf.FloorToInt(nIdealF / 8) * 8;
			return Mathf.Max(8, nIdeal);
		}
		
		/**
		Texture should be able to be divided by number that is close to IdealN.
		To ensure this can happen, iterate through textureWidth and find the closest number that can be divided by ideal N.
		*/
		static public int SnapTextureWidthToNearestOptimal(int textureWidth, int textureHeight)
		{
			int idealN = GetIdealN(textureHeight);
			int goodWidth = textureWidth;
			bool foundWorkingSolution = false;
			
			for (int i = 0; i < 16; i++)
			{
				int investigatedWidth = textureWidth + i * 8;
				if (investigatedWidth % idealN == 0)
				{
					foundWorkingSolution = true;
					goodWidth = investigatedWidth;
					break;
				}
			}
			
			if (!foundWorkingSolution)
			{
				Debug.LogWarning("Volumetric Lights could not properly optimize cloud sampling for this screen resolution and Cloud Data Resolution Combination. Artifacts may occur if distance between near and far clip planes is large.");
			}
			
			return goodWidth;
		}

		/**
		Returns texture width for Cloud Data Texture in pixels.
		The value is at least the diagonal length of the final render in pixels.
		This is then rounded up to the nearest multipler of 8 and snapped to the closest value that is compatible with the ideal resolution for the optimization system.
		*/
		static public int GetTextureWidth(int textureHeight)
		{
			float textureWidth;
			if (_VLManager.mainCamera.targetTexture == null)
			{
				textureWidth = Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height);
			} else 
			{
				textureWidth = Mathf.Sqrt(_VLManager.mainCamera.targetTexture.width * _VLManager.mainCamera.targetTexture.width + _VLManager.mainCamera.targetTexture.height * _VLManager.mainCamera.targetTexture.height);
			}
			textureWidth = Mathf.CeilToInt(textureWidth / 8) * 8; // Snap the resolution to be increment of 8
			textureWidth = SnapTextureWidthToNearestOptimal(Mathf.CeilToInt(textureWidth), textureHeight); // Ensure it is compatible with optimization algorithm
			return Mathf.CeilToInt(textureWidth);
		}
		
		static public float GetCameraDiagonalWorldUnits()
		{
			float orthoHeight = Camera.main.orthographicSize * 2;
			float orthoWidth = orthoHeight * Camera.main.aspect;
			return Mathf.Sqrt(orthoWidth * orthoWidth + orthoHeight * orthoHeight);
		}
		
		public static float GetPlaneHeightWorldUnits()
		{
			return _VLManager.cloudAreaFarClipPlane - _VLManager.cloudAreaNearClipPlane;
		}

		static public RenderTexture CreateCloudDataMap()
		{
			int textureWidth = GetTextureWidth(_VLManager.cloudDataResolution);
			RenderTexture newTexture = new RenderTexture(textureWidth, _VLManager.cloudDataResolution, 0, RenderTextureFormat.ARGB64);
			
			newTexture.enableRandomWrite = true;
			newTexture.wrapMode = TextureWrapMode.Clamp;
			newTexture.filterMode = FilterMode.Point;
			newTexture.Create();
			
			return newTexture;
		}
		
		static public RenderTexture CreateCookieMap(Vector2Int additionalCookieSizePixel)
		{
			int textureWidth = GetTextureWidth(_VLManager.cloudDataResolution);
			
			int textureX = Mathf.Max(64, Mathf.Min(4096, textureWidth + additionalCookieSizePixel.x));
			int textureY = Mathf.Max(64, Mathf.Min(4096, _VLManager.cloudDataResolution + additionalCookieSizePixel.y));
    
			RenderTexture newTexture = new RenderTexture(textureX, textureY, 0, RenderTextureFormat.ARGB64);
			
			newTexture.enableRandomWrite = true;
			newTexture.wrapMode = TextureWrapMode.Clamp;
			newTexture.filterMode = FilterMode.Point;
			newTexture.Create();
			
			return newTexture;
		}
		
		static public Transform FindMainLightTrans()
		{
			Light[] lights = GameObject.FindObjectsByType<Light>(FindObjectsSortMode.None);
			foreach (Light light in lights)
			{
				if (light.type == LightType.Directional)
				{
					return light.transform;
				}
			}
			return null;
		}
		
		static public Light FindMainLight()
		{
			Light[] lights = GameObject.FindObjectsByType<Light>(FindObjectsSortMode.None);
			foreach (Light light in lights)
			{
				if (light.type == LightType.Directional)
				{
					return light;
				}
			}
			return null;
		}

		static public Vector3 GetCloudShadowCenterPosition()
		{
			float lengthToMiddle = _VLManager.mainCamera.nearClipPlane + _VLManager.cloudAreaNearClipPlane + _VLManager.planeHeightWorldUnits * 0.5f;
			return _VLManager.mainCamera.transform.position + _VLManager.mainCamera.transform.forward * lengthToMiddle;
		}

	}
}

