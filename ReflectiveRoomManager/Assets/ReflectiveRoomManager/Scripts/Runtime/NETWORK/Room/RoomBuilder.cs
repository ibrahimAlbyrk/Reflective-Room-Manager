using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    using Structs;

    public sealed class RoomBuilder
    {
        private const int DefaultMaxPlayers = 10;

        private readonly string _roomName;
        private readonly string _sceneName;

        private int _maxPlayers = DefaultMaxPlayers;
        private bool _isPrivate;
        private Dictionary<string, string> _customData = new();

        public RoomBuilder(string roomName, string sceneName)
        {
            _roomName = roomName;
            _sceneName = sceneName;
        }

        public RoomBuilder WithMaxPlayers(int maxPlayers)
        {
            _maxPlayers = maxPlayers;
            return this;
        }

        public RoomBuilder AsPrivate()
        {
            _isPrivate = true;
            return this;
        }

        public RoomBuilder WithCustomData(string key, string value)
        {
            _customData[key] = value;
            return this;
        }

        public RoomBuilder WithCustomData(Dictionary<string, string> data)
        {
            _customData = data ?? new Dictionary<string, string>();
            return this;
        }

        public RoomInfo Build()
        {
            return new RoomInfo
            {
                RoomName = _roomName,
                SceneName = _sceneName,
                MaxPlayers = _maxPlayers,
                IsPrivate = _isPrivate,
                CustomData = _customData
            };
        }
    }
}
