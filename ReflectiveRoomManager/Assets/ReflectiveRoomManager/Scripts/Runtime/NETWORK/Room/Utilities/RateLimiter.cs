using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Utilities
{
    public class RateLimiter
    {
        private readonly Dictionary<NetworkConnection, Queue<float>> _requestTimes = new();
        private readonly int _maxRequestsPerWindow;
        private readonly float _windowSeconds;

        public RateLimiter(int maxRequests, float windowSeconds)
        {
            _maxRequestsPerWindow = maxRequests;
            _windowSeconds = windowSeconds;
        }

        public bool IsAllowed(NetworkConnection conn)
        {
            var currentTime = Time.unscaledTime;

            if (!_requestTimes.TryGetValue(conn, out var times))
            {
                times = new Queue<float>();
                _requestTimes[conn] = times;
            }

            while (times.Count > 0 && currentTime - times.Peek() > _windowSeconds)
                times.Dequeue();

            if (times.Count >= _maxRequestsPerWindow)
                return false;

            times.Enqueue(currentTime);
            return true;
        }

        public void RemoveConnection(NetworkConnection conn)
        {
            _requestTimes.Remove(conn);
        }

        public void Clear()
        {
            _requestTimes.Clear();
        }
    }
}
