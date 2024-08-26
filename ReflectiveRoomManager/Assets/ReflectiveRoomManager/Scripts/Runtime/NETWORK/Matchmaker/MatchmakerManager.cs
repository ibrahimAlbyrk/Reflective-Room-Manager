using Mirror;
using UnityEngine;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Matchmaker
{
    using Room;
    using Identifier;
    using Room.Service;

    public class Match
    {
        public Dictionary<string, string> Data;
        
        public List<NetworkConnection> Connections;
    }
    
    [System.Serializable]
    public class MatchInfo
    {
        public string Scene;
        public int MaxPlayerCount;
    }
    
    public class MatchmakerManager : MonoBehaviour
    {
        public static MatchmakerManager Instance;

        [SerializeField] private bool _dontDestroyOnLoad;
        
        [Header("Match Info")]
        [Scene, SerializeField] private string _matchScene;
        [SerializeField] private int _matchMaxPlayerCount;

        private MatchInfo _matchInfo;
        
        private RoomManagerBase _roomManager;

        private UniqueIdentifier _uniqueIdentifier;

        private readonly List<Room> _matchQueue = new();

        [ServerCallback]
        public void JoinMatch(NetworkConnectionToClient connection, RoomMatchType roomMatchType, params (string, string)[] matchData)
        {
            JoinMatch(new []{connection}, roomMatchType, matchData);
        }
        
        [ServerCallback]
        public void JoinMatch(IEnumerable<NetworkConnectionToClient> connections, RoomMatchType roomMatchType, params (string, string)[] matchData)
        {
            if (_roomManager == null) return;

            var matchStrategy = GetMatchStrategy(roomMatchType);

            var dictData = new Dictionary<string, string>();

            foreach (var (key, value) in matchData)
            {
                dictData.Add(key, value);
            }
            
            var room = CreateOrFindRoom(matchStrategy, dictData);

            if (_matchQueue.Contains(room)) return;

            foreach (var connection in connections)
            {
                RoomServer.JoinRoom(connection, room.Name);
            }
            
            _matchQueue.Add(room);
        }

        [ServerCallback]
        private Room CreateOrFindRoom(RoomMatchStrategy roomMatchStrategy, Dictionary<string,string> matchData)
        {
            var room = FindRoom(roomMatchStrategy, matchData) ?? CreateRoom(roomMatchStrategy, matchData);

            return room;
        }
        
        [ServerCallback]
        private Room FindRoom(RoomMatchStrategy roomMatchStrategy, Dictionary<string,string> matchData)
        {
            var rooms = _roomManager.GetRooms();

            foreach (var room in rooms)
            {
                var isFinded = roomMatchStrategy.Find(room, matchData);
                
                if(!isFinded) continue;

                return room;
            }
            
            return null;
        }

        [ServerCallback]
        private Room CreateRoom(RoomMatchStrategy roomMatchStrategy, Dictionary<string,string> matchData)
        {
            var id = _uniqueIdentifier.CreateID();

            var room = roomMatchStrategy.Create(id, matchData);
            
            return room;
        }

        private RoomMatchStrategy GetMatchStrategy(RoomMatchType roomMatchType)
        {
            var roomMatchStrategy = RoomMatchFactory.Create(roomMatchType);
            
            roomMatchStrategy.Init(_matchInfo);

            return roomMatchStrategy;
        }

        private void Awake()
        {
            _uniqueIdentifier = new UniqueIdentifier(4);

            _matchInfo = new MatchInfo
            {
                Scene = _matchScene,
                MaxPlayerCount = _matchMaxPlayerCount
            };
            
            CreateInstance();
        }
        
        private void Start()
        {
            _roomManager = RoomManagerBase.Instance;
        }

        private void Update()
        {
            if (_matchQueue.Count < 1) return;
            
            for (var i = 0; i < _matchQueue.Count; i++)
            {
                var room = _matchQueue[i];

                var playerCount = room.CurrentPlayers;
                var maxCount = room.MaxPlayers;
                
                if(playerCount < maxCount) continue;
                
                _matchQueue.RemoveAt(i);
            }
        }

        private void CreateInstance()
        {
            if (Instance != this && Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            
            if(_dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
        }
    }
}