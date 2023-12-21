using UnityEngine;

namespace Examples.SpaceShooter
{
    public class LookAtCamera : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        public Camera GetCamera() => _camera;

        private void Start()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        private void LateUpdate()
        {
            transform.LookAt(_camera.transform);
        }
    }
}