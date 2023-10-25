using Mirror;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    using Data;
    using Loader;
    using Events;
    using Structs;
    using Handlers;
    
    public abstract partial class RoomManagerBase
    {
        #region Serialize Variables

        [Header("Configuration")]
        [SerializeField] private bool _dontDestroyOnLoad = true;

        [Header("Scene Management")]
        [SerializeField] private LocalPhysicsMode _physicsMode = LocalPhysicsMode.Physics3D;
        [SerializeField] [Scene] private string _lobbyScene;
        [SerializeField] [Scene] private string _roomScene;
        
        [Header("Setup")]
        [SerializeField] private RoomData _defaultRoomData = new (10, 10, RoomLoaderType.AdditiveScene);

        #endregion

        #region Public Variables

        /// <summary>The one and only RoomManager</summary>
        public static RoomManagerBase Singleton
        {
            get
            {
                if (_singleton == null)
                    Debug.LogWarning("There is no Room Manager");

                return _singleton;
            }
        }

        public RoomEventManager Events => m_eventManager;
        
        public LocalPhysicsMode PhysicsMode => _physicsMode;
        
        public RoomData RoomData => _defaultRoomData;
        
        public string LobbyScene => _lobbyScene;
        public string RoomScene => _roomScene;
        
        #endregion

        #region Private Variables
        
        protected RoomEventManager m_eventManager;
        private NetworkConnectionHandler _networkConnectionHandler;
        private RoomConnectionHandler _roomConnectionHandler;

        protected List<Room> m_rooms = new();

        private static RoomManagerBase _singleton;
        private readonly List<RoomInfo> _roomListInfos = new();

        private IRoomLoader _roomLoader;

        #endregion
    }
}