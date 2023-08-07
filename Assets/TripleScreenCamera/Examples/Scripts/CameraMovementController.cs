using UnityEngine;

namespace FazApp.TripleScreenCamera.Examples
{
    public class CameraMovementController : MonoBehaviour
    {
        [SerializeField]
        private float maxZPosition;
        [SerializeField]
        private float resetZPosition;
        [SerializeField]
        private float speed;

        private void LateUpdate()
        {
            transform.Translate(Vector3.forward * (speed * Time.deltaTime), Space.World);

            if (transform.position.z > maxZPosition)
            {
                Vector3 position = transform.position;
                position.z = resetZPosition;
                transform.position = position;
            }
        }
    }
}