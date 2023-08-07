using UnityEngine;

namespace FazApp.TripleScreenCamera
{
    [AddComponentMenu("FazApp/TripleScreenCamera/TripleScreenCameraController")]
    public class TripleScreenCameraController : MonoBehaviour
    {
        [Tooltip("Used for switching between single and triple screen camera setups")]
        [SerializeField]
        private ScreenSetupType screenSetup = ScreenSetupType.TripleScreen;
        [Tooltip("Displays aspect ratio - x is horizontal, y is vertical")]
        [SerializeField]
        private Vector2 aspectRatio = new (16.0f, 9.0f);
        [Tooltip("Width of each monitor display in meters")]
        [SerializeField, Range(MinDisplayWidth, MaxDisplayWidth)]
        private float displayWidth = 0.5f;
        [Tooltip("Distance from center displays to driver head in meters")]
        [SerializeField, Range(MinDistanceFromCenterDisplay, MaxDistanceFromCenterDisplay)]
        private float distanceFromCenterDisplay = 0.5f;
        [Tooltip("Angle between center and lateral display")]
        [SerializeField, Range(MinLateralDisplayAngle, MaxLateralDisplayAngle)]
        private float lateralDisplaysAngle = 45.0f;
        [Tooltip("Distance from edge of center display to edge of lateral display in meters")]
        [SerializeField, Range(MinLateralDisplayMargin, MaxLateralDisplayMargin)]
        private float lateralDisplaysMargin;
        [Tooltip("Near clipping plane value that is used in all cameras")]
        [SerializeField, Min(MinNearClippingPlane)]
        private float nearClippingPlane = 0.2f;
        [Tooltip("Far clipping plane value that is used in all cameras")]
        [SerializeField, Min(MinFarClippingPlane)]
        private float farClippingPlane = 1000.0f;
    
        [Tooltip("Triggers automatic camera setup updates after changing settings properties. Can cause performance issues")]
        [SerializeField]
        private bool autoUpdate;
        
        [SerializeField]
        private CameraFrustumController centerCameraFrustumController;
        [SerializeField]
        private CameraFrustumController leftCameraFrustumController;
        [SerializeField]
        private CameraFrustumController rightCameraFrustumController;

        [SerializeField]
        private bool drawDebugDisplayGizmos = true;
        [SerializeField]
        private bool drawDebugCameraFrustumGizmos = true;

        private Vector3[] leftDisplayWorldSpaceCorners;
        private Vector3[] centerDisplayWorldSpaceCorners;
        private Vector3[] rightDisplayWorldSpaceCorners;
        
        private Vector3[] leftDisplayLocalSpaceCorners;
        private Vector3[] centerDisplayLocalSpaceCorners;
        private Vector3[] rightDisplayLocalSpaceCorners;

        private const float MinDisplayWidth = 0.01f;
        private const float MaxDisplayWidth = 3.0f;
        private const float MinDistanceFromCenterDisplay = 0.01f;
        private const float MaxDistanceFromCenterDisplay = 3.0f;
        private const float MinLateralDisplayAngle = 0.0f;
        private const float MaxLateralDisplayAngle = 90.0f;
        private const float MinLateralDisplayMargin = 0.0f;
        private const float MaxLateralDisplayMargin = 1.0f;
        private const float MinNearClippingPlane = 0.001f;
        private const float MinFarClippingPlane = 1.0f;
        
        private static readonly Rect SingleScreenCameraViewportRect = new (0.0f, 0.0f, 1.0f, 1.0f); 
        private static readonly Rect TripleScreenCameraViewportRect = new (1.0f / 3.0f, 0.0f, 1.0f / 3.0f, 1.0f);
        
        /// <summary>
        /// Used for switching between single and triple screen camera setups
        /// </summary>
        public ScreenSetupType ScreenSetup
        {
            get => screenSetup;
            set
            {
                screenSetup = value;

                if (AutoUpdate)
                {
                    UpdateCameraActiveState();
                    UpdateCenterCameraViewport();
                }
            }
        }
        
        /// <summary>
        /// Displays aspect ratio - x is horizontal, y is vertical. 
        /// </summary>
        public Vector2 AspectRatio
        {
            get => aspectRatio;
            set
            {
                aspectRatio = value;

                if (AutoUpdate)
                {
                    UpdateDisplayPositions();
                    UpdateCameraFrustum();
                }
            }
        }
        
        /// <summary>
        /// Width of each monitor display in meters
        /// </summary>
        public float DisplayWidth
        {
            get => displayWidth;
            set
            {
                if (!IsOptionInRange(nameof(DisplayWidth), value, MinDisplayWidth, MaxDisplayWidth))
                {
                    return;
                }
                
                displayWidth = value;

                if (AutoUpdate)
                {
                    UpdateDisplayPositions();
                    UpdateCameraFrustum();
                }
            }
        }
        
        /// <summary>
        /// Distance from center displays to driver head in meters
        /// </summary>
        public float DistanceFromCenterDisplay
        {
            get => distanceFromCenterDisplay;
            set
            {
                if (!IsOptionInRange(nameof(DistanceFromCenterDisplay), value, MinDistanceFromCenterDisplay, MaxDistanceFromCenterDisplay))
                {
                    return;
                }
                
                distanceFromCenterDisplay = value;

                if (AutoUpdate)
                {
                    UpdateDisplayPositions();
                    UpdateCameraFrustum();
                }
            }
        }
        
        /// <summary>
        /// Angle between center and lateral display
        /// </summary>
        public float LateralDisplaysAngle
        {
            get => lateralDisplaysAngle;
            set
            {
                if (!IsOptionInRange(nameof(LateralDisplaysAngle), value, MinLateralDisplayAngle, MaxLateralDisplayAngle))
                {
                    return;
                }
                
                lateralDisplaysAngle = value;

                if (AutoUpdate)
                {
                    UpdateDisplayPositions();
                    UpdateCameraFrustum();
                }
            }
        }
        
        /// <summary>
        /// Distance from edge of center display to edge of lateral display in meters
        /// </summary>
        public float LateralDisplaysMargin
        {
            get => lateralDisplaysMargin;
            set
            {
                if (!IsOptionInRange(nameof(LateralDisplaysMargin), value, MinLateralDisplayMargin, MaxLateralDisplayMargin))
                {
                    return;
                }
                
                lateralDisplaysMargin = value;

                if (AutoUpdate)
                {
                    UpdateDisplayPositions();
                    UpdateCameraFrustum();
                }
            }
        }
        
        /// <summary>
        /// Near clipping plane value that is used in all cameras
        /// </summary>
        public float NearClippingPlane
        {
            get => nearClippingPlane;
            set
            {
                if (!IsOptionInRange(nameof(NearClippingPlane), value, MinNearClippingPlane))
                {
                    return;
                }
                
                nearClippingPlane = value;

                if (AutoUpdate)
                {
                    UpdateDisplayPositions();
                    UpdateCameraFrustum();
                }
            }
        }
        
        /// <summary>
        /// Far clipping plane value that is used in all cameras
        /// </summary>
        public float FarClippingPlane
        {
            get => farClippingPlane;
            set
            {
                if (!IsOptionInRange(nameof(FarClippingPlane), value, MinFarClippingPlane))
                {
                    return;
                }
                
                farClippingPlane = value;

                if (AutoUpdate)
                {
                    UpdateDisplayPositions();
                    UpdateCameraFrustum();
                }
            }
        }
        
        /// <summary>
        /// Triggers automatic camera setup updates after changing settings properties. Can cause performance issues
        /// </summary>
        public bool AutoUpdate
        {
            get => autoUpdate;
            set
            {
                autoUpdate = value;

                if (value)
                {
                    UpdateAfterSettingsChange();
                }
            }
        }
        
        /// <summary>
        /// Use this method to force update after doing changes to camera setup settings without auto update enabled
        /// </summary>
        public void UpdateAfterSettingsChange()
        {
            UpdateCameraActiveState();
            UpdateCenterCameraViewport();
            UpdateDisplayPositions();
            UpdateCameraFrustum();
        }

        private void Awake()
        {
            UpdateAfterSettingsChange();
        }
    
        private void UpdateCameraActiveState()
        {
            leftCameraFrustumController.gameObject.SetActive(screenSetup == ScreenSetupType.TripleScreen);
            rightCameraFrustumController.gameObject.SetActive(screenSetup == ScreenSetupType.TripleScreen);
        }
    
        private void UpdateDisplayPositions()
        {
            Vector3 centerDisplayPosition = transform.position + transform.forward * distanceFromCenterDisplay;
            centerDisplayWorldSpaceCorners = GetDisplayCorners(centerDisplayPosition, transform.forward, transform.up);

            UpdateLateralDisplayCorners(centerDisplayPosition, true);
            UpdateLateralDisplayCorners(centerDisplayPosition, false);

            centerDisplayLocalSpaceCorners = GetLocalSpaceCornersPositions(centerDisplayWorldSpaceCorners);
            leftDisplayLocalSpaceCorners = GetLocalSpaceCornersPositions(leftDisplayWorldSpaceCorners);
            rightDisplayLocalSpaceCorners = GetLocalSpaceCornersPositions(rightDisplayWorldSpaceCorners);
        }

        private Vector3[] GetDisplayCorners(Vector3 displayPosition, Vector3 displayForwardDirection, Vector3 displayUpDirection)
        {
            Vector3[] corners = new Vector3[4];
            Vector3 rightDirection = Vector3.Cross(-displayForwardDirection, displayUpDirection).normalized;

            corners[0] = displayPosition + -1.0f * rightDirection * displayWidth * 0.5f + -1.0f * displayUpDirection * GetDisplayHeight() * 0.5f;
            corners[1] = displayPosition + rightDirection * displayWidth * 0.5f + -1.0f * displayUpDirection * GetDisplayHeight() * 0.5f;
            corners[2] = displayPosition + rightDirection * displayWidth * 0.5f + displayUpDirection * GetDisplayHeight() * 0.5f;
            corners[3] = displayPosition + -1.0f * rightDirection * displayWidth * 0.5f + displayUpDirection * GetDisplayHeight() * 0.5f;

            return corners;
        }
    
        private void UpdateLateralDisplayCorners(Vector3 centerDisplayPosition, bool isLeftDisplay)
        {
            float angleOfLateralDisplays = isLeftDisplay == true ? lateralDisplaysAngle * -1.0f : lateralDisplaysAngle;
            Vector3 displayEdgeDirection = isLeftDisplay == true ? transform.right * -1.0f : transform.right;
            Vector3 displayCenterDirection = isLeftDisplay == true ? Vector3.left : Vector3.right;
            float halfDisplayWidth = displayWidth / 2.0f;

            Quaternion displayRotation = transform.rotation * Quaternion.AngleAxis(angleOfLateralDisplays, transform.up);
            Vector3 displayEdgePosition = centerDisplayPosition + displayEdgeDirection * halfDisplayWidth + Quaternion.AngleAxis(angleOfLateralDisplays / 2.0f, transform.up) * displayEdgeDirection * lateralDisplaysMargin;
            Vector3 displayPosition = displayEdgePosition + displayRotation * displayCenterDirection * halfDisplayWidth;

            if (isLeftDisplay)
            {
                leftDisplayWorldSpaceCorners = GetDisplayCorners(displayPosition, displayRotation * transform.forward, transform.up);
            }
            else
            {
                rightDisplayWorldSpaceCorners = GetDisplayCorners(displayPosition, displayRotation * transform.forward, transform.up);
            }
        }

        private Vector3[] GetLocalSpaceCornersPositions(Vector3[] worldSpaceCorners)
        {
            Vector3[] result = new Vector3[worldSpaceCorners.Length];
            
            for (int i = 0; i < worldSpaceCorners.Length; i++)
            {
                result[i] = transform.InverseTransformPoint(worldSpaceCorners[i]);
            }

            return result;
        }
        
        private void UpdateCenterCameraViewport()
        {
            centerCameraFrustumController.CameraComponent.rect = screenSetup == ScreenSetupType.SingleScreen ? SingleScreenCameraViewportRect : TripleScreenCameraViewportRect;
        }
    
        private void UpdateCameraFrustum()
        {
            (float near, float far) = GetLateralCamerasClippingPlanes();
        
            centerCameraFrustumController.SetFrustum(centerDisplayWorldSpaceCorners, nearClippingPlane, farClippingPlane);
            leftCameraFrustumController.SetFrustum(leftDisplayWorldSpaceCorners, near, far);
            rightCameraFrustumController.SetFrustum(rightDisplayWorldSpaceCorners, near, far);
        }

        private (float, float) GetLateralCamerasClippingPlanes()
        {
            float near = GetLateralCameraClippingPlane(nearClippingPlane);
            float far = GetLateralCameraClippingPlane(farClippingPlane);

            return (near, far);
        }

        private float GetLateralCameraClippingPlane(float centerCameraClippingPlane)
        {
            float ratio = centerCameraClippingPlane / distanceFromCenterDisplay;
            float width = displayWidth * ratio;
            Vector3 centerClippingPlanePosition = transform.position + transform.forward * centerCameraClippingPlane;
        
            Quaternion clippingPlaneRotation = transform.rotation * Quaternion.AngleAxis(lateralDisplaysAngle, transform.up);
            Vector3 clippingPlaneEdgePosition = centerClippingPlanePosition + transform.right * (width * 0.5f) + Quaternion.AngleAxis(lateralDisplaysAngle / 2.0f, transform.up) * transform.right * lateralDisplaysMargin * ratio;
            Vector3 lateralClippingPlaneSideDirection = (clippingPlaneRotation * Vector3.right).normalized;

            Vector3 positionToEdge = transform.position - clippingPlaneEdgePosition;
            float dot = Vector3.Dot(positionToEdge, lateralClippingPlaneSideDirection);
            Vector3 closesPointOnClippingPlane = clippingPlaneEdgePosition + lateralClippingPlaneSideDirection * dot;
        
            return Vector3.Distance(transform.position, closesPointOnClippingPlane);
        }
    
        private float GetDisplayHeight()
        {
            return displayWidth / (aspectRatio.x / aspectRatio.y);
        }

        private bool IsOptionInRange(string optionName, float newValue, float min, float max = float.MaxValue)
        {
            if (newValue >= min && newValue <= max)
            {
                return true;
            }

            Debug.LogWarning("New option value is out of range\n" +
                             $"Option name: {optionName}\n" +
                             $"New value: {newValue:F4}, range: {min:F4} - {max:F4}");
            return false;
        }
        
        private void OnDrawGizmosSelected()
        {
            DrawDisplayGizmos();
            DrawCameraFrustumGizmos();
        }

        private void DrawDisplayGizmos()
        {
            if (!drawDebugDisplayGizmos)
            {
                return;
            }

            Vector3 position = transform.position;
            GizmosHelper.DrawDisplayGizmos(position, leftDisplayLocalSpaceCorners);
            GizmosHelper.DrawDisplayGizmos(position, centerDisplayLocalSpaceCorners);
            GizmosHelper.DrawDisplayGizmos(position, rightDisplayLocalSpaceCorners);
        }
    
        private void DrawCameraFrustumGizmos()
        {
            if (!drawDebugCameraFrustumGizmos)
            {
                return;    
            }

            if (centerCameraFrustumController != null)
            {
                GizmosHelper.DrawCameraFrustumGizmos(centerCameraFrustumController.CameraComponent);
            }

            if (screenSetup == ScreenSetupType.SingleScreen)
            {
                return;
            }
        
            if (leftCameraFrustumController != null)
            {
                GizmosHelper.DrawCameraFrustumGizmos(leftCameraFrustumController.CameraComponent);
            }
        
            if (rightCameraFrustumController != null)
            {
                GizmosHelper.DrawCameraFrustumGizmos(rightCameraFrustumController.CameraComponent);
            }
        }
    }
}