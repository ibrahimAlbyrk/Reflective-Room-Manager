using Mirror;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Room.Scenes
{
    internal class PendingContainerState
    {
        private readonly Dictionary<int, uint> _pending = new();

        internal void Add(NetworkConnection conn, uint roomId)
        {
            _pending[conn.connectionId] = roomId;
        }

        internal bool TryGet(NetworkConnection conn, out uint roomId)
        {
            return _pending.TryGetValue(conn.connectionId, out roomId);
        }

        internal void Remove(NetworkConnection conn)
        {
            _pending.Remove(conn.connectionId);
        }

        internal void Clear()
        {
            _pending.Clear();
        }
    }
}
