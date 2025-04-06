using UnityEngine;


namespace CritterVolumetricLighting
{
	static public class PreComputedCloudBufferHandler
	{
		private static ComputeShader _computeShader;
		private static VolumetricLightManager _vlManager;

		private static RenderTexture _cloudDataTexture;
		private static RenderTexture _preComputedTexture;
		
		private static Material _vLMat;
		private static int _screenHeight, _screenWidth;
		
		private static int _mainCSKernel;
		private static int _clearerKernel;

		private static Vector2Int _preCalcTextBounds;

		private static int _n; // 'N' is the amount of pixels on Y axis in the precomputed cloud data texture. Each individual pixel in this texture represents a chunk of pixels in the raw cloud data texture. X in this texture is same as in raw texture
		private static int _nSizeInCloudDataTex;
		
		private static bool _firstInit = false;

		public static RenderTexture Init(ref RenderTexture cloudTexture, VolumetricLightManager vlManager)
		{
			_vlManager = vlManager;
			_cloudDataTexture = cloudTexture;
			
			if (!_firstInit) {
				_computeShader = Resources.Load<ComputeShader>("CloudTexturePreCalculationCS");
				_mainCSKernel = _computeShader.FindKernel("CSMain");
				_clearerKernel = _computeShader.FindKernel("ClearKernel");
				_firstInit = true;
			}

			_n = CalculateN(cloudTexture.width, cloudTexture.height);
			_nSizeInCloudDataTex = Mathf.CeilToInt(cloudTexture.height / _n);
			
			GetPreCalTextBounds();
			InitPreCalcTexture(vlManager);
			InitCs();

			RunCS();
			return _preComputedTexture;
		}
		
		/**
		Calculates optimal amount of splits for the precomputed cloud data texture that maximizes optimization benefits.
		N needs to be dividable by 8 and the texture width needs to be dividable by N.
		
		The final N is as close to the most performatic "ideal N" as possible, while still adhering to the constraints described above.
		*/
		private static int CalculateN(int dataTextureWidth, int dataTextureHeight)
		{
			int nIdeal = VolumetricLightResources.GetIdealN(dataTextureHeight);
			int n = 16;
			bool foundOptimal = false;
			for (int i = 0; i < 8; i++)
			{
				float investigatedN = nIdeal + i * 8;
				if (dataTextureWidth  % investigatedN == 0)
				{
					foundOptimal = true;
					n = Mathf.RoundToInt(investigatedN);
					break;
				}
			}
			
			// Because texture width is configured to be compatible with optimization, nearest optimal N should be quite close to ideal N.
			// Exhausting all combinations of screen and data resolutions did not find a single case where optimal was not found.
			// Still... If for some reason optimal is not found, we can try to find a working but slightly suboptimal solution.
			if (!foundOptimal)
			{
				int iterationsToIdealN = Mathf.RoundToInt((nIdeal - 8) / 8);
				bool foundSubOptimalN = false;
				
				for (int i = 0; i < iterationsToIdealN; i++)
				{
					float investigatedN = nIdeal - i * 8;
					if (dataTextureWidth  % investigatedN == 0)
					{
						foundSubOptimalN = true;
						n = Mathf.RoundToInt(investigatedN);
						break;
					}
				}
				
				if (!foundSubOptimalN)
				{
					Debug.LogWarning("Stylized Volumetric Lighting could not properly optimize the sampling of Clouds with this screen resolution. Artifacts in cloud shadows may occur in some cases.");
				}
			}
			return n;
		}

		private static void InitPreCalcTexture(VolumetricLightManager vlManager)
		{
			if (_preComputedTexture != null)
			{
				_preComputedTexture.Release();
				_preComputedTexture = null;
			}
			
			_preComputedTexture = new RenderTexture(_preCalcTextBounds.x, _preCalcTextBounds.y, 0, RenderTextureFormat.ARGB64);
			_preComputedTexture.wrapMode = TextureWrapMode.Clamp;
			_preComputedTexture.filterMode = FilterMode.Point;
			_preComputedTexture.enableRandomWrite = true;
			_preComputedTexture.Create();
			
			_vLMat = vlManager.godrayMat;
			_vLMat.SetFloat("_PreCalcN", _n);
			_vLMat.SetFloat("_PreCalcWorldUnitSizeN", VolumetricLightResources.GetPlaneHeightWorldUnits() / _n);
			_vLMat.SetTexture("_PreComputedTexture", _preComputedTexture);
		}
	
		private static void GetPreCalTextBounds()
		{
			int textureHeight = _n;
			int textureWidth = _vlManager.cloudDataTexture.width;
			_preCalcTextBounds = new Vector2Int(textureWidth, textureHeight);
		}
		
		
		private static void InitCs()
		{	
			_computeShader.SetTexture(_clearerKernel, "_PreCalcTexture", _preComputedTexture);
			_computeShader.SetTexture(_mainCSKernel, "_PreCalcTexture", _preComputedTexture);
			_computeShader.SetTexture(_mainCSKernel, "_CloudShadowMap", _cloudDataTexture);
			_computeShader.SetFloat("_nSizeInCloudDataTex", _nSizeInCloudDataTex);
		}
		

		public static void RunCS()
		{
			_computeShader.Dispatch(_clearerKernel, _preCalcTextBounds.x / 8, _preCalcTextBounds.y / 8, 1);
			_computeShader.Dispatch(_mainCSKernel, _preCalcTextBounds.x / 8, _preCalcTextBounds.y / 8, 1);
		}
	}
}