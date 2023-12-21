using Mirror;
using UnityEngine;

namespace Examples.Basic.Camera
{
    public class FollowCharacterCamera : MonoBehaviour
    {
        [SerializeField] private Vector3 positionOffset;

        [SerializeField] private float _followSpeed = 10f;
        [SerializeField] private float _lookSpeed = 10f;

        private Transform _playerTransform;

        private Vector3 _velocity;
        private Vector3 currentPos;
        private Vector3 lookAngle;
        private Vector3 relativePos;

        private bool _lockMouse;

        private void Start()
        {
            _lockMouse = true;
        }

        private void Update() => ToggleMouseLock();

        private void LateUpdate()
        {
            if (_playerTransform == null) AssignPlayerToFollow();

            UpdateCameraFollow();
        }
        
        private void ToggleMouseLock()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;

            _lockMouse = !_lockMouse;
            
            Cursor.visible = !_lockMouse;
            Cursor.lockState = _lockMouse ? CursorLockMode.Locked : CursorLockMode.None;
        }
        
        private void AssignPlayerToFollow()
        {
            var players = GameObject.FindGameObjectsWithTag("Player");
            foreach (var player in players)
            {
                if (player == null || !player.TryGetComponent(out NetworkIdentity identity) || !identity.isOwned) continue;

                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                
                _playerTransform = player.transform;
                
                transform.position = _playerTransform.position + positionOffset;
                
                break;
            }
        }
        
        private void UpdateCameraFollow()
        {
            if (_playerTransform == null) return;

            CalculateLookAngle();
            CalculateRelativePosition();
            
            positionOffset = Quaternion.Euler(lookAngle) * positionOffset;
            
            currentPos = _playerTransform.position + positionOffset;
            
            var targetRotation = Quaternion.LookRotation(relativePos, Vector3.up);
            
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, _lookSpeed * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, currentPos, _followSpeed * Time.deltaTime);
        }
        
        private void CalculateLookAngle()
        {
            var horizontalInput = Input.GetAxisRaw("Mouse X");
            lookAngle = Vector3.up * (horizontalInput * _lookSpeed * Time.deltaTime);
        }

        private void CalculateRelativePosition()
        {
            relativePos = (_playerTransform.position - transform.position).normalized;
        }
    }
}