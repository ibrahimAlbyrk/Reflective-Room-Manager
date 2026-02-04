using System.Collections.Generic;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Chat
{
    /// <summary>
    /// Sliding window rate limiter for chat messages.
    /// Tracks request timestamps per connection and enforces limits.
    /// </summary>
    public class ChatRateLimiter
    {
        private readonly Dictionary<uint, Queue<float>> _requestTimes = new();
        private readonly int _maxRequests;
        private readonly float _windowSeconds;

        public ChatRateLimiter(int maxRequests, float windowSeconds)
        {
            _maxRequests = maxRequests > 0 ? maxRequests : 5;
            _windowSeconds = windowSeconds > 0 ? windowSeconds : 10f;
        }

        /// <summary>
        /// Checks if a request is allowed and records it if so.
        /// </summary>
        /// <param name="connectionID">Connection ID making the request</param>
        /// <returns>True if request is allowed</returns>
        public bool AllowRequest(uint connectionID)
        {
            var currentTime = Time.unscaledTime;

            if (!_requestTimes.TryGetValue(connectionID, out var timestamps))
            {
                timestamps = new Queue<float>();
                _requestTimes[connectionID] = timestamps;
            }

            // Remove expired timestamps
            while (timestamps.Count > 0 && currentTime - timestamps.Peek() > _windowSeconds)
            {
                timestamps.Dequeue();
            }

            // Check limit
            if (timestamps.Count >= _maxRequests)
                return false;

            // Record this request
            timestamps.Enqueue(currentTime);
            return true;
        }

        /// <summary>
        /// Gets the number of remaining requests for a connection.
        /// </summary>
        /// <param name="connectionID">Connection ID to check</param>
        /// <returns>Number of remaining requests in current window</returns>
        public int GetRemainingRequests(uint connectionID)
        {
            if (!_requestTimes.TryGetValue(connectionID, out var timestamps))
                return _maxRequests;

            var currentTime = Time.unscaledTime;

            // Clean expired
            while (timestamps.Count > 0 && currentTime - timestamps.Peek() > _windowSeconds)
            {
                timestamps.Dequeue();
            }

            return Mathf.Max(0, _maxRequests - timestamps.Count);
        }

        /// <summary>
        /// Resets rate limit tracking for a connection.
        /// </summary>
        /// <param name="connectionID">Connection ID to reset</param>
        public void ResetPlayer(uint connectionID)
        {
            _requestTimes.Remove(connectionID);
        }

        /// <summary>
        /// Clears all rate limit tracking data.
        /// </summary>
        public void Clear()
        {
            _requestTimes.Clear();
        }
    }
}
