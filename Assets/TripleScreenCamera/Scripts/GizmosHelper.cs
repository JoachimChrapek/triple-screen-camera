using System.Collections.Generic;
using UnityEngine;

namespace FazApp.TripleScreenCamera
{
    public static class GizmosHelper
    {
        /// <summary>
        /// Draws virtual display rectangle gizmos
        /// </summary>
        /// <param name="originPosition">Origin transform world space position</param>
        /// <param name="displayCornersLocalSpace">Four corners positions in origin local space coordinates. First element is bottom left, last is top left, goes counter clockwise</param>
        public static void DrawDisplayGizmos(Vector3 originPosition, IReadOnlyList<Vector3> displayCornersLocalSpace)
        {
            if (displayCornersLocalSpace == null)
            {
                return;
            }
            
            Gizmos.color = Color.red;

            for (int i = 0; i < 4; i++)
            {
                int nextIndex = (i + 1) % 4;
                Gizmos.DrawLine(originPosition + displayCornersLocalSpace[i], originPosition + displayCornersLocalSpace[nextIndex]);
            }
        }

        /// <summary>
        /// Draws camera frustum same as built-in gizmos
        /// </summary>
        public static void DrawCameraFrustumGizmos(Camera camera)
        {
            if (camera == null)
            {
                return;
            }
            
            Gizmos.color = Color.white;
            
            Vector3[] nearCorners = new Vector3[4];
            Vector3[] farCorners = new Vector3[4];
            Plane[] cameraPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
            (cameraPlanes[1], cameraPlanes[2]) = (cameraPlanes[2], cameraPlanes[1]);
        
            for (int i = 0; i < 4; i++)
            {
                nearCorners[i] = ThreePlaneIntersection(cameraPlanes[4], cameraPlanes[i], cameraPlanes[(i + 1) % 4]);
                farCorners[i] = ThreePlaneIntersection(cameraPlanes[5], cameraPlanes[i], cameraPlanes[(i + 1) % 4]);
            }
        
            for (int i = 0; i < 4; i++)
            {
                Gizmos.DrawLine(nearCorners[i], nearCorners[(i + 1) % 4]);
                Gizmos.DrawLine(farCorners[i], farCorners[(i + 1) % 4]);
                Gizmos.DrawLine(nearCorners[i], farCorners[i]);
            }
        }
    
        private static Vector3 ThreePlaneIntersection(Plane plane1, Plane plan2, Plane plane3)
        {
            return (-plane1.distance * Vector3.Cross(plan2.normal, plane3.normal) +
                    -plan2.distance * Vector3.Cross(plane3.normal, plane1.normal) +
                    -plane3.distance * Vector3.Cross(plane1.normal, plan2.normal)) /
                   Vector3.Dot(plane1.normal, Vector3.Cross(plan2.normal, plane3.normal));
        }
    }
}