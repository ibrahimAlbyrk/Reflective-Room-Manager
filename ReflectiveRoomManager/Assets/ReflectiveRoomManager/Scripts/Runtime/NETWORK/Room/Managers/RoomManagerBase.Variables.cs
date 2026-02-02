using Mirror;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    using Loader;
    using Events;
    using Scenes;
    using Structs;
    using Identifier;
    
    public abstract partial class RoomManagerBase
    {
        #region Serialize Variables

        [Header("Configuration")]
        [SerializeField] private bool _dontDestroyOnLoad = true;

        [Header("Scene Management")]
        [SerializeField] private LocalPhysicsMode _physicsMode = LocalPhysicsMode.Physics3D;
        [SerializeField] [Scene] private string _lobbyScene;
        [SerializeField] [Scene] private string _roomScene;
        [SerializeField] [Scene] private string _clientContainerScene;
        
        [Header("Room Info")]
        [Tooltip("Maximum number of rooms that can be on the server")]
        [SerializeField] private int _maxRoomCount = 10;
        
        [Tooltip("Maximum number of players a client can specify for a room")]
        [SerializeField] private int _maxPlayerCountPerRoom = 5;

        [Tooltip("determines what type of loading the room will have")]
        [SerializeField] private RoomLoaderType _RoomLoaderType = RoomLoaderType.AdditiveScene;

        #endregion

        #region Public Variables

        /// <summary>The one and only RoomManager</summary>
        public static RoomManagerBase Instance
        {
            get
            {
                if (_singleton == null)
                    Debug.LogWarning("There is no Room Manager Instance");

                return _singleton;
            }
        }

        public RoomEventManager Events => m_eventManager;
        
        public LocalPhysicsMode PhysicsMode => _physicsMode;
        public int MaxRoomCount => _maxRoomCount;
        public int MaxPlayerCountPerRoom => _maxPlayerCountPerRoom;

        public RoomLoaderType RoomLoaderType => _RoomLoaderType;
        
        public string LobbyScene => _lobbyScene;
        public string RoomScene => _roomScene;
        public string ClientContainerScene => _clientContainerScene;
        
        #endregion

        #region Private Variables
        
        protected RoomEventManager m_eventManager;

        protected readonly List<Room> m_rooms = new();
        
        protected UniqueIdentifier m_uniqueIdentifier;

        private static RoomManagerBase _singleton;
        private readonly List<RoomInfo> _roomListInfos = new();

        private IRoomLoader _roomLoader;

        private RoomSceneSynchronizer _sceneSynchronizer;

        private System.Action _onServerStoppedRemoveAllRoom;

        #endregion
    }
}