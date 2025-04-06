namespace PixelCamera
{
    using UnityEngine;

    /// <summary>
    /// Simple camera controller to try out the camera system or expand on it.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        PixelCameraManager pixelCameraManager;

        [Header("Moving")]
        public float MoveSpeed = 10f;

        [Header("Scrolling")]
        public float ViewScrollSpeed = 0.1f;
        public float GameScrollSpeed = 1f;

        [Header("Rotating")]
        public float DragSpeed = 0.5f;
        public float AutoRotateSpeed = 30f;
        public float DistanceToPivot = 30f;
        public int RotationQuantDegrees = 45;

        // Rotation variables
        float previousMousePosition;
        float totalInput;
        float rotationRemaining;

        /// <summary>
        /// Unity lifetime function. Called before the first frame update only if the script instance is enabled.
        /// </summary>
        void Start()
        {
            if (!this.TryGetComponent(out this.pixelCameraManager))
            {
                Debug.LogError("This game object should have a pixel camera manager connected. It was not found!");
            }
        }

        /// <summary>
        /// Unity lifetime function Start. Is called once per frame and is the main function for frame updates.
        /// </summary>
        void Update()
        {
            this.UpdateZoom(Input.mouseScrollDelta.y, Input.GetKey(KeyCode.LeftControl));

            this.UpdateMovement(Input.GetAxisRaw("Vertical"), Input.GetAxisRaw("Horizontal"), this.MoveSpeed * Time.deltaTime);

            this.UpdateRotation((Input.mousePosition.x - this.previousMousePosition) * this.DragSpeed, 
                                Input.mousePosition.x, 
                                Input.GetMouseButton(1), 
                                Input.GetMouseButtonUp(1));
        }

        /// <summary>
        /// Updates the zoom of the camera.
        /// </summary>
        void UpdateZoom(float input, bool updateGameCamera)
        {
            if (updateGameCamera)
            {
                this.pixelCameraManager.GameCameraZoom -= input * this.GameScrollSpeed;
            }
            else
            {
                this.pixelCameraManager.ViewCameraZoom -= input * this.ViewScrollSpeed;
            }
        }

        /// <summary>
        /// Updates the movement of the camera.
        /// </summary>
        void UpdateMovement(float verticalInput, float horizontalInput, float speedTimeInput)
        {
            var target = this.pixelCameraManager.FollowedTransform;

            var forward = new Vector3(target.forward.x, 0, target.forward.z).normalized;
            var right = new Vector3(target.right.x, 0, target.right.z).normalized;

            target.position += ((verticalInput * forward) + (horizontalInput * right)) * speedTimeInput;
        }

        /// <summary>
        /// Rotates the camera.
        /// </summary>
        void Rotate(float rotateInput)
        {
            var target = this.pixelCameraManager.FollowedTransform;
            var targetPosition = target.position + (target.forward * this.DistanceToPivot);
            target.RotateAround(targetPosition, Vector3.up, rotateInput);
        }

        /// <summary>
        /// Updates the rotation of the camera.
        /// </summary>
        void UpdateRotation(float rotationInput, float mousePosition, bool rotationInputting, bool rotationInputStopped)
        {
            if (rotationInputStopped)
            {
                this.rotationRemaining = GetRotationLeft(this.totalInput - this.rotationRemaining, this.totalInput, this.RotationQuantDegrees);
                this.totalInput = 0;
            }

            var isRotationRemaining = Mathf.Abs(this.rotationRemaining) > 0f;
            if (rotationInputting)
            {
                this.Rotate(rotationInput);
                this.totalInput += rotationInput;
            }
            else if (isRotationRemaining)
            {
                var autoRotate = Mathf.Clamp(Time.deltaTime * this.AutoRotateSpeed, 0, Mathf.Abs(this.rotationRemaining)) * Mathf.Sign(this.rotationRemaining);
                this.Rotate(autoRotate);
                this.rotationRemaining -= autoRotate;
            }

            var smoothPixelCamera = !(rotationInputting || isRotationRemaining);
            this.pixelCameraManager.VoxelGridMovement = smoothPixelCamera;
            this.pixelCameraManager.SubpixelAdjustments = smoothPixelCamera;

            this.previousMousePosition = mousePosition;
        }

        /// <summary>
        /// Gets how much rotation is left after rotating manually.
        /// </summary>
        static float GetRotationLeft(float totalRotation, float input, int acceptableCameraAngle = 45)
        {
            var rotationQuanted = totalRotation / acceptableCameraAngle;
            var yawGoal = (input > 0 ? Mathf.Ceil(rotationQuanted) : Mathf.Floor(rotationQuanted)) * acceptableCameraAngle;
            return yawGoal - totalRotation;
        }
    }
}