using UnityEngine;

namespace FazApp.TripleScreenCamera
{
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("FazApp/TripleScreenCamera/CameraFrustumController")]
    public class CameraFrustumController : MonoBehaviour
    {
        private Camera cameraComponent;

        public Camera CameraComponent
        {
            get
            {
                if (cameraComponent == null)
                {
                    cameraComponent = GetComponent<Camera>();
                }

                return cameraComponent;
            }
        }

        /// <summary>
        /// Sets camera frustum to match provided viewport corners positions
        /// </summary>
        /// <param name="corners"> Four corners positions array in world space. First element is bottom left, last is top left, goes counter clockwise</param>
        public void SetFrustum(Vector3[] corners, float nearClippingPlane, float farClippingPlane)
        {
            SetFrustum(corners[0], corners[1], corners[3], nearClippingPlane, farClippingPlane);
        }
    
        /// <summary>
        /// Sets camera frustum to match provided viewport corners positions
        /// </summary>
        public void SetFrustum(Vector3 bottomLeftCornerPosition, Vector3 bottomRightCornerPosition, Vector3 topLeftCornerPosition,  float nearClippingPlane, float farClippingPlane)
        {
            Vector3 cameraPosition = CameraComponent.transform.position;
        
            Vector3 rightDirectionNormalized = (bottomRightCornerPosition - bottomLeftCornerPosition).normalized;
            Vector3 upDirectionNormalized = (topLeftCornerPosition - bottomLeftCornerPosition).normalized;
            Vector3 normal = Vector3.Cross(rightDirectionNormalized, upDirectionNormalized).normalized;
        
            Vector3 bottomLeftDirection = bottomLeftCornerPosition - cameraPosition;
            Vector3 bottomRightDirection = bottomRightCornerPosition - cameraPosition;
            Vector3 topLeftDirection = topLeftCornerPosition - cameraPosition;
        
            float bottomLeftDot = Vector3.Dot(bottomLeftDirection, normal);
            float leftEdgeDistance = Vector3.Dot(rightDirectionNormalized, bottomLeftDirection) * nearClippingPlane / bottomLeftDot;
            float rightEdgeDistance = Vector3.Dot(rightDirectionNormalized, bottomRightDirection) * nearClippingPlane / bottomLeftDot;
            float bottomEdgeDistance = Vector3.Dot(upDirectionNormalized, bottomLeftDirection) * nearClippingPlane / bottomLeftDot;
            float topEdgeDistance = Vector3.Dot(upDirectionNormalized, topLeftDirection) * nearClippingPlane / bottomLeftDot;
        
            Matrix4x4 newProjectionMatrix = new ();
            newProjectionMatrix[0, 0] = 2.0f * nearClippingPlane / (rightEdgeDistance - leftEdgeDistance);
            newProjectionMatrix[0, 2] = (rightEdgeDistance + leftEdgeDistance) / (rightEdgeDistance - leftEdgeDistance);
            newProjectionMatrix[1, 1] = 2.0f * nearClippingPlane / (topEdgeDistance - bottomEdgeDistance);
            newProjectionMatrix[1, 2] = (topEdgeDistance + bottomEdgeDistance) / (topEdgeDistance - bottomEdgeDistance);
            newProjectionMatrix[2, 2] = (farClippingPlane + nearClippingPlane) / (nearClippingPlane - farClippingPlane);
            newProjectionMatrix[2, 3] = 2.0f * farClippingPlane * nearClippingPlane / (nearClippingPlane - farClippingPlane);
            newProjectionMatrix[3, 2] = -1.0f;

            CameraComponent.nearClipPlane = nearClippingPlane;
            CameraComponent.farClipPlane = farClippingPlane;
            CameraComponent.transform.rotation = Quaternion.LookRotation(normal);
            CameraComponent.projectionMatrix = newProjectionMatrix;
        }
    }
}