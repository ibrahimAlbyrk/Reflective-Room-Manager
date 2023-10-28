using Mirror;
using UnityEngine;

namespace Example.Basic.Camera
{
    public class FollowCharacterCamera : MonoBehaviour
    {
        [SerializeField] private Vector3 positionOffset;

        [SerializeField] private float _followSpeed = 10f;
        [SerializeField] private float _lookSpeed = 10f;

        private Transform _playerTransform;

        private Vector3 _velocity;
        
        private Vector3 currentLookAngle;
        private Vector3 currentPos;

        private bool _lockMouse;
        
        private void LookAtTarget()
        {
            var relativePos = _playerTransform.position - transform.position;
            var toRotation = Quaternion.LookRotation(relativePos, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, _lookSpeed * Time.deltaTime);
        }

        private void MoveToTarget()
        {
            transform.position = Vector3.Lerp(transform.position, currentPos, _followSpeed * Time.deltaTime);
        }
        
        private void FollowTarget()
        {
            currentPos = _playerTransform.position + positionOffset;
            currentPos = Vector3.Slerp(currentPos, _playerTransform.position + positionOffset, _followSpeed * Time.deltaTime);
            currentPos.y = _playerTransform.position.y + positionOffset.y;
        }
        
        private void RotateAroundTarget(float horizontalInput)
        {
            if (Mathf.Abs(horizontalInput) < 0.001f) return;
            
            currentLookAngle.y = horizontalInput * _lookSpeed * Time.deltaTime;
            positionOffset = Quaternion.Euler(currentLookAngle) * positionOffset;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _lockMouse = !_lockMouse;
                
                Cursor.visible = !_lockMouse;
                Cursor.lockState = _lockMouse ? CursorLockMode.Locked : CursorLockMode.None;
            }
        }
        
        private void LateUpdate()
        {
            
            if (_playerTransform == null)
            {
                var players = GameObject.FindGameObjectsWithTag("Player");

                foreach (var player in players)
                {
                    if (player == null || !player.TryGetComponent(out NetworkIdentity identity) ||
                        !identity.isOwned) continue;
                    
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    
                    _playerTransform = player.transform;
                    transform.position = _playerTransform.position + positionOffset;
                    break;
                }
            }
            else
            {
                RotateAroundTarget(Input.GetAxisRaw("Mouse X"));
                FollowTarget();
                
                LookAtTarget();
                MoveToTarget();
            }
        }
    }
}