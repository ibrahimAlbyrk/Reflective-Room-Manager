using Mirror;
using System.Linq;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Room.Scenes
{
    internal class PendingContainerState
    {
        private readonly Dictionary<int, uint> _pending = new();

        internal void Add(NetworkConnectionToClient conn, uint roomId)
        {
            _pending[conn.connectionId] = roomId;
        }

        internal bool TryGet(NetworkConnectionToClient conn, out uint roomId)
        {
            return _pending.TryGetValue(conn.connectionId, out roomId);
        }

        internal void Remove(NetworkConnectionToClient conn)
        {
            _pending.Remove(conn.connectionId);
        }

        internal void RemoveByRoomId(uint roomId)
        {
            var keysToRemove = _pending
                .Where(kvp => kvp.Value == roomId)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _pending.Remove(key);
            }
        }

        internal void Clear()
        {
            _pending.Clear();
        }
    }
}
