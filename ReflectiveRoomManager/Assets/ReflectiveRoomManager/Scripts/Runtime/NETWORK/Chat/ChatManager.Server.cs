#if REFLECTIVE_SERVER
using System;
using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Chat
{
    using Structs;
    using Messages;
    using Room;
    using REFLECTIVE.Runtime.NETWORK.Team;

    public partial class ChatManager
    {
        private void OnServerChatRequest(NetworkConnectionToClient conn, ChatRequestMessage request)
        {
            if (!ValidateRequest(conn, request, out var error, out var errorCode))
            {
                SendError(conn, error, errorCode);
                return;
            }

            var filteredContent = ApplyFilters(request.Content, out var flags);

            if (filteredContent == null)
            {
                SendError(conn, "Message blocked by filter.", ChatErrorCode.ContentBlocked);
                return;
            }

            var message = CreateMessage(conn, request, filteredContent, flags);

            _serverHistory.AddMessage(message);
            BroadcastMessage(message);
        }

        private void OnServerHistoryRequest(NetworkConnectionToClient conn, ChatHistoryRequestMessage request)
        {
            var clampedCount = Mathf.Clamp(request.Count, 1, _settings.MaxHistoryPerChannel);
            var messages = _serverHistory.GetHistory(request.Channel, clampedCount);

            conn.Send(new ChatHistoryResponseMessage
            {
                Channel = request.Channel,
                Messages = messages
            });
        }

        private bool ValidateRequest(NetworkConnectionToClient conn, ChatRequestMessage request, out string error, out ChatErrorCode code)
        {
            error = null;
            code = ChatErrorCode.None;

            // Rate limit check
            if (!_rateLimiter.AllowRequest((uint)conn.connectionId))
            {
                error = "Rate limit exceeded. Wait before sending.";
                code = ChatErrorCode.RateLimited;
                return false;
            }

            // Mute check
            if (_muteManager.IsMuted((uint)conn.connectionId))
            {
                var info = _muteManager.GetMuteInfo((uint)conn.connectionId);
                error = info.IsPermanent ? "You are permanently muted." : $"You are muted for {info.RemainingSeconds:F0}s.";
                code = ChatErrorCode.Muted;
                return false;
            }

            // Content validation
            if (string.IsNullOrWhiteSpace(request.Content))
            {
                error = "Message cannot be empty.";
                code = ChatErrorCode.InvalidContent;
                return false;
            }

            if (request.Content.Length > _settings.MaxContentLength)
            {
                error = $"Message exceeds {_settings.MaxContentLength} character limit.";
                code = ChatErrorCode.InvalidContent;
                return false;
            }

            // Channel permission check
            if (!HasChannelPermission(conn, request.Channel))
            {
                error = "No permission for this channel.";
                code = ChatErrorCode.NoPermission;
                return false;
            }

            // Whisper target validation
            if (request.Channel == ChatChannel.Whisper && request.TargetConnectionID == 0)
            {
                error = "Whisper target not specified.";
                code = ChatErrorCode.TargetNotFound;
                return false;
            }

            return true;
        }

        private string ApplyFilters(string content, out ChatMessageFlags flags)
        {
            flags = ChatMessageFlags.None;

            foreach (var filter in _filters)
            {
                var result = filter.Filter(content);
                content = result.FilteredContent;

                if (result.WasCensored)
                    flags |= ChatMessageFlags.Censored;
                else if (result.WasModified)
                    flags |= ChatMessageFlags.Filtered;

                if (result.ShouldBlock)
                    return null;
            }

            return content;
        }

        private ChatMessage CreateMessage(NetworkConnectionToClient conn, ChatRequestMessage request, string filteredContent, ChatMessageFlags flags)
        {
            var connId = (uint)conn.connectionId;

            return new ChatMessage
            {
                MessageID = GenerateMessageID(connId),
                SenderConnectionID = connId,
                SenderName = GetPlayerName(connId),
                Content = filteredContent,
                Channel = request.Channel,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Flags = flags,
                TargetConnectionID = request.TargetConnectionID,
                TargetName = request.TargetName,
                RoomID = GetRoomID(conn)
            };
        }

        private void BroadcastMessage(ChatMessage message)
        {
            var broadcast = new ChatBroadcastMessage { Message = message };

            switch (message.Channel)
            {
                case ChatChannel.Whisper:
                    BroadcastWhisper(message, broadcast);
                    break;

                case ChatChannel.Room:
                    BroadcastToRoom(message.RoomID, broadcast);
                    break;

                case ChatChannel.Team:
                    BroadcastToTeam(message.SenderConnectionID, broadcast);
                    break;

                case ChatChannel.Global:
                    BroadcastGlobal(broadcast);
                    break;

                case ChatChannel.System:
                    NetworkServer.SendToAll(broadcast);
                    break;

                default:
                    NetworkServer.SendToAll(broadcast);
                    break;
            }
        }

        private void BroadcastWhisper(ChatMessage message, ChatBroadcastMessage broadcast)
        {
            // Send to sender
            SendToConnection(message.SenderConnectionID, broadcast);

            // Send to target if different
            if (message.TargetConnectionID != message.SenderConnectionID)
                SendToConnection(message.TargetConnectionID, broadcast);
        }

        private void BroadcastToRoom(uint roomID, ChatBroadcastMessage broadcast)
        {
            if (roomID == 0)
            {
                Debug.LogWarning("[ChatManager] Room broadcast attempted but sender not in room.");
                return;
            }

            var roomManager = RoomManagerBase.Instance;
            if (roomManager == null)
            {
                Debug.LogWarning("[ChatManager] RoomManagerBase not found for room broadcast.");
                return;
            }

            var room = roomManager.GetRoom(roomID);
            if (room == null)
            {
                Debug.LogWarning($"[ChatManager] Room {roomID} not found.");
                return;
            }

            foreach (var conn in room.Connections)
            {
                if (conn is NetworkConnectionToClient clientConn)
                    clientConn.Send(broadcast);
            }
        }

        private void BroadcastToTeam(uint senderConnectionID, ChatBroadcastMessage broadcast)
        {
            var roomManager = RoomManagerBase.Instance;
            if (roomManager == null) return;

            // Find sender connection
            if (!NetworkServer.connections.TryGetValue((int)senderConnectionID, out var senderConn)) return;
            if (senderConn is not NetworkConnectionToClient clientConn) return;

            var room = roomManager.GetRoomByConnection(clientConn);
            if (room == null) return;

            var teamManager = roomManager.GetRoomTeamManager(room);
            var senderTeam = teamManager?.GetPlayerTeam(clientConn);
            if (senderTeam == null) return;

            // Send to all team members
            foreach (var memberConn in senderTeam.GetMemberConnections())
            {
                if (memberConn is NetworkConnectionToClient memberClient)
                    memberClient.Send(broadcast);
            }
        }

        private void BroadcastGlobal(ChatBroadcastMessage broadcast)
        {
            switch (_settings.GlobalChatScope)
            {
                case GlobalChatScope.AllConnected:
                    NetworkServer.SendToAll(broadcast);
                    break;

                case GlobalChatScope.LobbyOnly:
                    BroadcastToLobby(broadcast);
                    break;

                case GlobalChatScope.InRoomOnly:
                    BroadcastToAllRooms(broadcast);
                    break;
            }
        }

        private void BroadcastToLobby(ChatBroadcastMessage broadcast)
        {
            foreach (var conn in NetworkServer.connections.Values)
            {
                if (conn is NetworkConnectionToClient clientConn && !IsInRoom(clientConn))
                    clientConn.Send(broadcast);
            }
        }

        private void BroadcastToAllRooms(ChatBroadcastMessage broadcast)
        {
            foreach (var conn in NetworkServer.connections.Values)
            {
                if (conn is NetworkConnectionToClient clientConn && IsInRoom(clientConn))
                    clientConn.Send(broadcast);
            }
        }

        private void SendToConnection(uint connectionID, ChatBroadcastMessage broadcast)
        {
            if (NetworkServer.connections.TryGetValue((int)connectionID, out var conn))
            {
                if (conn is NetworkConnectionToClient clientConn)
                    clientConn.Send(broadcast);
            }
        }

        private void SendError(NetworkConnectionToClient conn, string error, ChatErrorCode code)
        {
            conn.Send(new ChatErrorMessage { Error = error, Code = code });
        }

        private ulong GenerateMessageID(uint senderID)
        {
            var timestamp = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return (timestamp << 16) | (senderID & 0xFFFF);
        }

        private bool HasChannelPermission(NetworkConnectionToClient conn, ChatChannel channel)
        {
            // Room channel requires being in a room
            if (channel == ChatChannel.Room)
                return IsInRoom(conn);

            // Team channel requires team membership
            if (channel == ChatChannel.Team)
                return IsInTeam(conn);

            // System channel is server-only
            if (channel == ChatChannel.System)
                return false;

            return true;
        }

        private bool IsInTeam(NetworkConnectionToClient conn)
        {
            var roomManager = RoomManagerBase.Instance;
            if (roomManager == null) return false;

            var room = roomManager.GetRoomByConnection(conn);
            if (room == null) return false;

            var teamManager = roomManager.GetRoomTeamManager(room);
            return teamManager != null && teamManager.GetPlayerTeam(conn) != null;
        }

        private bool IsInRoom(NetworkConnectionToClient conn)
        {
            var roomManager = RoomManagerBase.Instance;
            if (roomManager == null)
                return false;

            return roomManager.GetRoomByConnection(conn) != null;
        }

        private uint GetRoomID(NetworkConnectionToClient conn)
        {
            var roomManager = RoomManagerBase.Instance;
            if (roomManager == null)
                return 0;

            var room = roomManager.GetRoomByConnection(conn);
            return room?.ID ?? 0;
        }

        private string GetPlayerName(uint connectionID)
        {
            if (_playerNames.TryGetValue(connectionID, out var name))
                return name;

            return $"Player_{connectionID}";
        }

        #region Server Public API

        /// <summary>
        /// Sets or updates a player's display name.
        /// </summary>
        public void SetPlayerName(uint connectionID, string name)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[ChatManager] SetPlayerName called but server not active.");
                return;
            }

            var truncatedName = name?.Length > _settings.MaxNameLength
                ? name.Substring(0, _settings.MaxNameLength)
                : name ?? $"Player_{connectionID}";

            _playerNames[connectionID] = truncatedName;
        }

        /// <summary>
        /// Mutes a player for a specified duration.
        /// </summary>
        public void MutePlayer(uint connectionID, float durationSeconds, string reason = null)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[ChatManager] MutePlayer called but server not active.");
                return;
            }

            _muteManager.Mute(connectionID, durationSeconds, reason);
        }

        /// <summary>
        /// Permanently mutes a player.
        /// </summary>
        public void MutePlayerPermanent(uint connectionID, string reason = null)
        {
            MutePlayer(connectionID, 0f, reason);
        }

        /// <summary>
        /// Unmutes a player.
        /// </summary>
        public void UnmutePlayer(uint connectionID)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[ChatManager] UnmutePlayer called but server not active.");
                return;
            }

            _muteManager.Unmute(connectionID);
        }

        /// <summary>
        /// Sends a system message to all players.
        /// </summary>
        public void SendSystemMessage(string content)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[ChatManager] SendSystemMessage called but server not active.");
                return;
            }

            var message = new ChatMessage
            {
                MessageID = GenerateMessageID(0),
                SenderConnectionID = 0,
                SenderName = "System",
                Content = content,
                Channel = ChatChannel.System,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Flags = ChatMessageFlags.System
            };

            _serverHistory.AddMessage(message);
            NetworkServer.SendToAll(new ChatBroadcastMessage { Message = message });
        }

        /// <summary>
        /// Sends a system message to a specific room.
        /// </summary>
        public void SendSystemMessageToRoom(uint roomID, string content)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[ChatManager] SendSystemMessageToRoom called but server not active.");
                return;
            }

            var message = new ChatMessage
            {
                MessageID = GenerateMessageID(0),
                SenderConnectionID = 0,
                SenderName = "System",
                Content = content,
                Channel = ChatChannel.System,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Flags = ChatMessageFlags.System,
                RoomID = roomID
            };

            BroadcastToRoom(roomID, new ChatBroadcastMessage { Message = message });
        }

        /// <summary>
        /// Handles player disconnect - cleans up resources.
        /// </summary>
        public void OnPlayerDisconnected(uint connectionID)
        {
            if (!NetworkServer.active)
                return;

            _rateLimiter?.ResetPlayer(connectionID);
            _playerNames?.Remove(connectionID);
            // Note: We don't remove mute status on disconnect for reconnection scenarios
        }

        #endregion
    }
}
#endif
