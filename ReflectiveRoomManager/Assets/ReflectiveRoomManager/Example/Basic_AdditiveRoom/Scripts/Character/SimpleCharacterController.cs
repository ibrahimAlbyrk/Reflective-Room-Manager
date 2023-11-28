using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using REFLECTIVE.Runtime.Extensions;
using REFLECTIVE.Runtime.Physic.Collision.D3;
using REFLECTIVE.Runtime.NETWORK.Room.Listeners;

namespace Example.Basic.Character
{
    using Game;
    
    public class SimpleCharacterController : NetworkBehaviour, IRoomListener
    {
        [SyncVar] public int ID;
        
        public float speed = 10f;

        [SerializeField] private Collision3D _collision3D;

        private Transform _camTransform;

        private CoinSpawner _coinSpawner;
        private ScoreManager _scoreManager;
        
        [ServerCallback]
        private void OnDestroy()
        {
            gameObject.RoomContainer().UnRegisterListener(this);
        }
        
        public void OnRoomSceneChanged(Scene scene)
        {
            SetManagers();
        }
        
        [ServerCallback]
        private void Start()
        {
            _collision3D.enabled = true;
            _collision3D.SetLayer(LayerMask.GetMask("Coin"));
            _collision3D.OnCollisionEnter += CollectCoin;
            
            gameObject.RoomContainer().RegisterListener(this);

            SetManagers();
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
        
        private void SetManagers()
        {
            _coinSpawner = gameObject.RoomContainer().GetSingleton<CoinSpawner>();

            _scoreManager = gameObject.RoomContainer().GetSingleton<ScoreManager>();
        }
        
        private void CollectCoin(Collider coll)
        {
            if (coll == null) return;
            
            _coinSpawner.DestroyCoin(coll.gameObject);
                
            _scoreManager.AddScore(ID, 1);
        }
    }
}