﻿using Mirror;
using UnityEngine;

namespace Example.Basic.Camera
{
    public class FollowCharacterCamera : MonoBehaviour
    {
        [SerializeField] private float followSmoothness = 50f;
        [SerializeField] private Vector3 positionOffset;

        private Transform _playerTransform;

        private Vector3 _velocity;

        private void LateUpdate()
        {
            if (_playerTransform == null)
            {
                var player = GameObject.FindWithTag("Player");

                if (player != null && player.TryGetComponent(out NetworkIdentity identity) && identity.isOwned)
                {
                    _playerTransform = player.transform;
                    transform.position = _playerTransform.position + positionOffset;
                }
            }
            else
            {
                var target = _playerTransform.position + positionOffset;

                transform.position = Vector3.SmoothDamp(transform.position, target, ref _velocity, followSmoothness * Time.deltaTime);
            }
        }
    }
}