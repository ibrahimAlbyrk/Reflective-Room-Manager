using System;
using System.Collections.Generic;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Chat.History
{
    using Structs;

    /// <summary>
    /// Stores chat message history per channel with configurable limits.
    /// </summary>
    public class MessageHistory
    {
        private readonly Dictionary<ChatChannel, Queue<ChatMessage>> _channelHistory;
        private readonly int _maxPerChannel;

        public MessageHistory(int maxPerChannel)
        {
            _maxPerChannel = maxPerChannel > 0 ? maxPerChannel : 100;
            _channelHistory = new Dictionary<ChatChannel, Queue<ChatMessage>>();
        }

        /// <summary>
        /// Adds a message to the appropriate channel history.
        /// Automatically removes oldest messages when limit is exceeded.
        /// </summary>
        /// <param name="message">Message to store</param>
        public void AddMessage(ChatMessage message)
        {
            if (!_channelHistory.TryGetValue(message.Channel, out var queue))
            {
                queue = new Queue<ChatMessage>();
                _channelHistory[message.Channel] = queue;
            }

            queue.Enqueue(message);

            // Enforce limit
            while (queue.Count > _maxPerChannel)
            {
                queue.Dequeue();
            }
        }

        /// <summary>
        /// Retrieves the most recent messages from a channel.
        /// </summary>
        /// <param name="channel">Channel to retrieve from</param>
        /// <param name="count">Maximum number of messages to retrieve</param>
        /// <returns>Array of messages (oldest first)</returns>
        public ChatMessage[] GetHistory(ChatChannel channel, int count)
        {
            if (!_channelHistory.TryGetValue(channel, out var queue))
                return Array.Empty<ChatMessage>();

            var actualCount = Mathf.Min(count, queue.Count);
            if (actualCount <= 0)
                return Array.Empty<ChatMessage>();

            var allMessages = queue.ToArray();

            // Return last N messages
            var result = new ChatMessage[actualCount];
            Array.Copy(allMessages, allMessages.Length - actualCount, result, 0, actualCount);

            return result;
        }

        /// <summary>
        /// Gets the total message count for a channel.
        /// </summary>
        /// <param name="channel">Channel to check</param>
        /// <returns>Number of stored messages</returns>
        public int GetMessageCount(ChatChannel channel)
        {
            if (!_channelHistory.TryGetValue(channel, out var queue))
                return 0;

            return queue.Count;
        }

        /// <summary>
        /// Clears all messages for a specific channel.
        /// </summary>
        /// <param name="channel">Channel to clear</param>
        public void ClearChannel(ChatChannel channel)
        {
            if (_channelHistory.TryGetValue(channel, out var queue))
                queue.Clear();
        }

        /// <summary>
        /// Clears all stored message history.
        /// </summary>
        public void ClearAll()
        {
            _channelHistory.Clear();
        }
    }
}
