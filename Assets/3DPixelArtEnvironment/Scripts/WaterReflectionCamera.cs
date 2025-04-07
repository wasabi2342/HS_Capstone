namespace Environment.PixelWater
{
    using UnityEngine;

    [RequireComponent(typeof(Camera))]
    public class WaterReflectionCamera : MonoBehaviour
    {
        public Transform followedCameraTransform;

        Transform waterPlane;
        Camera reflectionCamera;
        Vector2Int waterResolution = new Vector2Int(-1, -1); // State variable for updates

        void OnEnable()
        {
            this.reflectionCamera = this.GetComponent<Camera>();

            if (this.transform.parent == null)
            {
                Debug.LogError("The water reflection camera should have a parent that is the water plane.");
            }
            this.waterPlane = this.transform.parent;

            if (Camera.main == null)
            {
                Debug.LogError("There is no main camera found! Set the tag in editor.");
            }

            if (this.followedCameraTransform == null)
            {
                this.followedCameraTransform = Camera.main.transform;
            }

            this.ApplyNewRenderTexture();
        }

        private void OnDisable()
        {
            if (this.reflectionCamera.targetTexture != null)
            {
                this.reflectionCamera.targetTexture.Release();
            }
        }

        private void LateUpdate()
        {
            this.transform.position = PlanarReflectionProbe.GetPosition(this.followedCameraTransform.position, this.waterPlane.position, this.waterPlane.up);

            this.transform.LookAt(this.transform.position + Vector3.Reflect(this.followedCameraTransform.forward, this.waterPlane.up), Vector3.Reflect(this.followedCameraTransform.up, this.waterPlane.up));

            this.reflectionCamera.projectionMatrix = PlanarReflectionProbe.GetObliqueProjection(this.reflectionCamera, this.waterPlane.position, this.waterPlane.up);

            this.reflectionCamera.orthographicSize = Camera.main.orthographicSize;
            if (Camera.main.targetTexture != null)
            {
                if (Camera.main.targetTexture.width != this.waterResolution.x || Camera.main.targetTexture.height != this.waterResolution.y)
                {
                    this.ApplyNewRenderTexture();
                }
            }
        }

        /// <summary>
        /// Apply new render texture.
        /// </summary>
        void ApplyNewRenderTexture()
        {
            var textureResolution = Camera.main.targetTexture == null ? new Vector2Int(Camera.main.pixelWidth, Camera.main.pixelHeight) 
                                                                      : new Vector2Int(Camera.main.targetTexture.width, Camera.main.targetTexture.height);
            var newTexture = new RenderTexture(textureResolution.x, textureResolution.y, 32, RenderTextureFormat.ARGB32)
            {
                filterMode = FilterMode.Point
            };
            _ = newTexture.Create();

            if (this.reflectionCamera.targetTexture != null)
            {
                this.reflectionCamera.targetTexture.Release();
            }

            this.reflectionCamera.targetTexture = newTexture;

            var materialPropertyBlock = new MaterialPropertyBlock();
            materialPropertyBlock.SetTexture("_WaterReflectionTexture", newTexture);
            this.waterPlane.GetComponent<MeshRenderer>().SetPropertyBlock(materialPropertyBlock);

            this.waterResolution = textureResolution;
        }
    }
}