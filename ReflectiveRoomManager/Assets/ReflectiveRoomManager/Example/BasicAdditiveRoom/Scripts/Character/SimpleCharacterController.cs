using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using REFLECTIVE.Runtime.Container;
using REFLECTIVE.Runtime.Extensions;
using REFLECTIVE.Runtime.NETWORK.Room;
using REFLECTIVE.Runtime.Physic.Collision.D3;
using REFLECTIVE.Runtime.NETWORK.Room.Listeners;

namespace Examples.Basic.Character
{
    using Game;

    public class SimpleCharacterController : NetworkBehaviour, IRoomSceneListener
    {
        public static SimpleCharacterController Local;
        
        [SyncVar] public int ID;
        
        public float speed = 10f;

        [SerializeField] private Collision3D _collision3D;

        private Transform _camTransform;

        private CoinSpawner _coinSpawner;
        private ScoreManager _scoreManager;

        private string _roomName;
        
        public void OnRoomSceneChanged(Scene scene)  
        {
            SetManagers(scene);
            
            _collision3D.UpdatePhysicScene(scene.GetPhysicsScene());
        }

        public override void OnStartClient()
        {
            if (isOwned)
                Local = this;
        }

        [ServerCallback]
        private void Start()
        {
            _collision3D.enabled = true;
            _collision3D.SetLayer(LayerMask.GetMask("Coin"));
            _collision3D.OnCollisionEnter += CollectCoin;

            if (RoomManagerBase.Instance == null) return;
            
            SetManagers(gameObject.scene);
            
            var room = RoomManagerBase.Instance.GetRoomByScene(gameObject.scene);
            ID = room.CurrentPlayers;

            _roomName = room.Name;
            
            gameObject.RoomContainer().RegisterListener<IRoomSceneListener>(this);
        }

        [ServerCallback]
        private void OnDestroy()
        {
            RoomContainer.Listener.UnRegisterListener<IRoomSceneListener>(_roomName, this);
        }
        
        [ClientCallback]
        private void Update()
        {
            if (!isOwned) return;

            if (_camTransform == null)
                _camTransform = GameObject.FindGameObjectWithTag("PlayerCamera")?.transform;

            if (_camTransform == null) return;

            var xMovement = Input.GetAxisRaw("Horizontal");
            var zMovement = Input.GetAxisRaw("Vertical");

            var dir = (_camTransform.forward * zMovement + _camTransform.right * xMovement).normalized;
            dir.y = 0;

            transform.Translate(dir * (speed * Time.deltaTime));
        }

        private void SetManagers(Scene scene)
        {
            _coinSpawner = RoomContainer.Singleton.Get<CoinSpawner>(scene);
            _scoreManager = RoomContainer.Singleton.Get<ScoreManager>(scene);
        }
        
        private void CollectCoin(Collider coll)
        {
            _coinSpawner.DestroyCoin(coll.gameObject);
                
            _scoreManager.AddScore(ID, 1);
        }
    }
}