﻿using Mirror;
using UnityEngine;
using REFLECTIVE.Runtime.Extensions;
using REFLECTIVE.Runtime.Physic.Collision.D3;

namespace Example.Basic.Character
{
    using Game;
    
    public class SimpleCharacterController : NetworkBehaviour
    {
        [SyncVar] public int ID;
        
        public float speed = 10f;

        [SerializeField] private Collision3D _collision3D;

        private CoinSpawner _coinSpawner;
        private ScoreManager _scoreManager;

        private Transform _camTransform;
        
        [ServerCallback]
        private void Start()
        {
            _coinSpawner = gameObject.RoomContainer().Get<CoinSpawner>();
            _scoreManager = gameObject.RoomContainer().Get<ScoreManager>();

            _collision3D.enabled = true;
            _collision3D.SetLayer(LayerMask.GetMask("Coin"));
            _collision3D.OnCollisionEnter += CollectCoin;
        }
        
        [ClientCallback]
        private void Update()
        {
            if (!isOwned) return;

            if (_camTransform == null)
                _camTransform = GameObject.FindGameObjectWithTag("PlayerCamera").transform;

            var xMovement = Input.GetAxisRaw("Horizontal");
            var zMovement = Input.GetAxisRaw("Vertical");

            var dir = (_camTransform.forward * zMovement + _camTransform.right * xMovement).normalized;
            dir.y = 0;

            transform.Translate(dir * (speed * Time.deltaTime));
        }

        private void CollectCoin(Collider coll)
        {
            if (coll == null) return;
            
            _coinSpawner?.DestroyCoin(coll.gameObject);
                
            _scoreManager?.AddScore(ID, 1);
        }
    }
}