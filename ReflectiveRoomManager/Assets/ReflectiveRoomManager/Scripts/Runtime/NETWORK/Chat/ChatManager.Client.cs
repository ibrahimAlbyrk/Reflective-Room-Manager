using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Chat
{
    using Structs;
    using Messages;

    public partial class ChatManager
    {
        private void OnClientChatBroadcast(ChatBroadcastMessage broadcast)
        {
            var message = broadcast.Message;

            // Store in local history
            if (!_clientHistory.TryGetValue(message.Channel, out var history))
            {
                history = new List<ChatMessage>();
                _clientHistory[message.Channel] = history;
            }

            history.Add(message);

            // Enforce local history limit
            while (history.Count > _settings.MaxHistoryPerChannel)
            {
                history.RemoveAt(0);
            }

            // Notify listeners
            OnMessageReceived?.Invoke(message);
        }

        private void OnClientChatError(ChatErrorMessage error)
        {
            Debug.LogWarning($"[ChatManager] Chat error: {error.Error} (Code: {error.Code})");
            OnChatError?.Invoke(error.Error, error.Code);
        }

        private void OnClientHistoryResponse(ChatHistoryResponseMessage response)
        {
            if (!_clientHistory.TryGetValue(response.Channel, out var history))
            {
                history = new List<ChatMessage>();
                _clientHistory[response.Channel] = history;
            }

            // Insert historical messages at the beginning
            history.InsertRange(0, response.Messages);

            // Enforce limit
            while (history.Count > _settings.MaxHistoryPerChannel)
            {
                history.RemoveAt(history.Count - 1);
            }

            // Notify for each historical message
            foreach (var message in response.Messages)
            {
                OnMessageReceived?.Invoke(message);
            }
        }

        #region Client Public API

        /// <summary>
        /// Sends a chat message to the specified channel.
        /// </summary>
        /// <param name="channel">Target channel</param>
        /// <param name="content">Message content</param>
        public void SendChatMessage(ChatChannel channel, string content)
        {
            if (!NetworkClient.active)
            {
                Debug.LogWarning("[ChatManager] SendChatMessage called but client not connected.");
                return;
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                Debug.LogWarning("[ChatManager] Cannot send empty message.");
                return;
            }

            var request = new ChatRequestMessage
            {
                Channel = channel,
                Content = content
            };

            NetworkClient.Send(request);
        }

        /// <summary>
        /// Sends a whisper message to a specific player.
        /// </summary>
        /// <param name="targetConnectionID">Target player's connection ID</param>
        /// <param name="targetName">Target player's display name</param>
        /// <param name="content">Message content</param>
        public void SendWhisper(uint targetConnectionID, string targetName, string content)
        {
            if (!NetworkClient.active)
            {
                Debug.LogWarning("[ChatManager] SendWhisper called but client not connected.");
                return;
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                Debug.LogWarning("[ChatManager] Cannot send empty whisper.");
                return;
            }

            var request = new ChatRequestMessage
            {
                Channel = ChatChannel.Whisper,
                Content = content,
                TargetConnectionID = targetConnectionID,
                TargetName = targetName
            };

            NetworkClient.Send(request);
        }

        /// <summary>
        /// Sends a message to the global channel.
        /// </summary>
        public void SendGlobalMessage(string content)
        {
            SendChatMessage(ChatChannel.Global, content);
        }

        /// <summary>
        /// Sends a message to the current room channel.
        /// </summary>
        public void SendRoomMessage(string content)
        {
            SendChatMessage(ChatChannel.Room, content);
        }

        /// <summary>
        /// Sends a message to the team channel.
        /// </summary>
        public void SendTeamMessage(string content)
        {
            SendChatMessage(ChatChannel.Team, content);
        }

        /// <summary>
        /// Requests message history for a channel from the server.
        /// </summary>
        /// <param name="channel">Channel to get history for</param>
        /// <param name="count">Maximum number of messages to retrieve</param>
        public void RequestHistory(ChatChannel channel, int count = 50)
        {
            if (!NetworkClient.active)
            {
                Debug.LogWarning("[ChatManager] RequestHistory called but client not connected.");
                return;
            }

            var request = new ChatHistoryRequestMessage
            {
                Channel = channel,
                Count = count
            };

            NetworkClient.Send(request);
        }

        /// <summary>
        /// Gets local cached history for a channel.
        /// </summary>
        /// <param name="channel">Channel to get history for</param>
        /// <returns>List of cached messages (oldest first)</returns>
        public List<ChatMessage> GetLocalHistory(ChatChannel channel)
        {
            if (_clientHistory == null || !_clientHistory.TryGetValue(channel, out var history))
                return new List<ChatMessage>();

            return new List<ChatMessage>(history);
        }

        /// <summary>
        /// Gets the most recent messages from local cache.
        /// </summary>
        /// <param name="channel">Channel to get history for</param>
        /// <param name="count">Maximum number of messages</param>
        /// <returns>List of recent messages (oldest first)</returns>
        public List<ChatMessage> GetRecentMessages(ChatChannel channel, int count)
        {
            var history = GetLocalHistory(channel);
            if (history.Count <= count)
                return history;

            return history.GetRange(history.Count - count, count);
        }

        /// <summary>
        /// Clears local history for a specific channel.
        /// </summary>
        public void ClearLocalHistory(ChatChannel channel)
        {
            if (_clientHistory != null && _clientHistory.TryGetValue(channel, out var history))
                history.Clear();
        }

        /// <summary>
        /// Clears all local history.
        /// </summary>
        public void ClearAllLocalHistory()
        {
            _clientHistory?.Clear();
        }

        #endregion
    }
}
