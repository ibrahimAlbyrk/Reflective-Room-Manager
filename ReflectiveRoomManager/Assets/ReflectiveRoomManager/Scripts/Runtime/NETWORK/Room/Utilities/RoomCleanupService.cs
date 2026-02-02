using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Utilities
{
    public class RoomCleanupService
    {
        private readonly float _emptyRoomTimeout;
        private readonly float _checkInterval;
        private readonly Dictionary<uint, float> _emptyRoomTimestamps = new();
        private float _lastCheckTime;

        public RoomCleanupService(float emptyRoomTimeoutSeconds, float checkIntervalSeconds)
        {
            _emptyRoomTimeout = emptyRoomTimeoutSeconds;
            _checkInterval = checkIntervalSeconds;
        }

        public void Update(IEnumerable<Room> rooms, Action<Room> removeRoomAction)
        {
            var now = Time.unscaledTime;

            if (now - _lastCheckTime < _checkInterval) return;

            _lastCheckTime = now;

            var roomsToRemove = new List<Room>();

            foreach (var room in rooms)
            {
                if (room.CurrentPlayers < 1 && room.ReservedSlots < 1 && !room.IsServer)
                {
                    if (!_emptyRoomTimestamps.TryGetValue(room.ID, out var timestamp))
                    {
                        _emptyRoomTimestamps[room.ID] = now;
                    }
                    else if (now - timestamp >= _emptyRoomTimeout)
                    {
                        roomsToRemove.Add(room);
                    }
                }
                else
                {
                    _emptyRoomTimestamps.Remove(room.ID);
                }
            }

            foreach (var room in roomsToRemove)
            {
                _emptyRoomTimestamps.Remove(room.ID);
                removeRoomAction(room);
            }
        }

        public void Clear() => _emptyRoomTimestamps.Clear();

        public void RemoveRoom(uint roomId) => _emptyRoomTimestamps.Remove(roomId);
    }
}
