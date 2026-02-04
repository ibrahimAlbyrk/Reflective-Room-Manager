using System;
using System.Collections.Generic;
using REFLECTIVE.Runtime.NETWORK.Chat.Messages;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.GUI.Modules
{
    using Chat;
    using Structs;
    using REFLECTIVE.Runtime.NETWORK.Chat.Structs;

    public class ChatHUDModule : IHUDModule
    {
        public string TabName => "Chat";

        private ChatChannel _selectedChannel = ChatChannel.Room;
        private string _inputText = "";
        private Vector2 _scrollPos;
        private List<ChatMessage> _displayMessages = new();
        private bool _autoScroll = true;
        private string _lastError;
        private float _errorTime;

        // Channel colors
        private static readonly Color GlobalColor = new(0.8f, 0.8f, 0.4f);
        private static readonly Color RoomColor = new(0.5f, 0.8f, 1f);
        private static readonly Color TeamColor = new(0.4f, 1f, 0.4f);
        private static readonly Color WhisperColor = new(1f, 0.6f, 0.8f);
        private static readonly Color SystemColor = new(1f, 0.5f, 0.5f);

        public void RegisterEvents()
        {
            var chat = ChatManager.Instance;
            if (chat == null) return;

            chat.OnMessageReceived += OnMessageReceived;
            chat.OnChatError += OnChatError;
        }

        public void UnregisterEvents()
        {
            var chat = ChatManager.Instance;
            if (chat == null) return;

            chat.OnMessageReceived -= OnMessageReceived;
            chat.OnChatError -= OnChatError;
        }

        private void OnMessageReceived(ChatMessage msg)
        {
            // Add to display if matches selected channel or is whisper/system
            if (msg.Channel == _selectedChannel ||
                msg.Channel == ChatChannel.Whisper ||
                msg.Channel == ChatChannel.System)
            {
                _displayMessages.Add(msg);

                // Limit displayed messages
                while (_displayMessages.Count > 100)
                    _displayMessages.RemoveAt(0);

                _autoScroll = true;
            }
        }

        private void OnChatError(string error, ChatErrorCode code)
        {
            _lastError = error;
            _errorTime = Time.time;
        }

        public void DrawTab(RoomInfo room)
        {
            DrawChannelTabs();
            GUILayout.Space(5);
            DrawMessages();
            DrawInput();
            DrawError();
        }

        private void DrawChannelTabs()
        {
            GUILayout.BeginHorizontal();

            // Global
            DrawChannelButton(ChatChannel.Global, "Global", GlobalColor);

            // Room (always available when in room)
            DrawChannelButton(ChatChannel.Room, "Room", RoomColor);

            // Team (if team system enabled)
            var rm = RoomManagerBase.Instance;
            if (rm != null && rm.EnableTeamSystem)
                DrawChannelButton(ChatChannel.Team, "Team", TeamColor);

            GUILayout.EndHorizontal();
        }

        private void DrawChannelButton(ChatChannel channel, string label, Color color)
        {
            var isSelected = _selectedChannel == channel;
            var oldColor = UnityEngine.GUI.backgroundColor;

            if (isSelected)
                UnityEngine.GUI.backgroundColor = color;

            if (GUILayout.Button(label, GUILayout.Height(22)))
            {
                if (_selectedChannel != channel)
                {
                    _selectedChannel = channel;
                    RefreshMessages();
                }
            }

            UnityEngine.GUI.backgroundColor = oldColor;
        }

        private void RefreshMessages()
        {
            _displayMessages.Clear();
            var chat = ChatManager.Instance;
            if (chat == null) return;

            var history = chat.GetLocalHistory(_selectedChannel);
            _displayMessages.AddRange(history);
            _autoScroll = true;
        }

        private void DrawMessages()
        {
            var scrollHeight = 180f;

            // Auto-scroll to bottom
            if (_autoScroll)
            {
                _scrollPos.y = float.MaxValue;
                _autoScroll = false;
            }

            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(scrollHeight));

            if (_displayMessages.Count == 0)
            {
                GUILayout.Label("No messages yet...", GetChannelStyle(ChatChannel.System));
            }
            else
            {
                foreach (var msg in _displayMessages)
                {
                    DrawMessage(msg);
                }
            }

            GUILayout.EndScrollView();
        }

        private void DrawMessage(ChatMessage msg)
        {
            var oldColor = UnityEngine.GUI.color;
            UnityEngine.GUI.color = GetChannelColor(msg.Channel);

            var time = DateTimeOffset.FromUnixTimeMilliseconds(msg.Timestamp).ToLocalTime();
            var timeStr = time.ToString("HH:mm");

            string prefix;
            if (msg.Channel == ChatChannel.Whisper)
            {
                // Show whisper direction
                var isFromMe = msg.SenderName == GetMyName();
                prefix = isFromMe ? $"[{timeStr}] To {msg.TargetName}" : $"[{timeStr}] From {msg.SenderName}";
            }
            else if (msg.Channel == ChatChannel.System)
            {
                prefix = $"[{timeStr}] [SYSTEM]";
            }
            else
            {
                prefix = $"[{timeStr}] {msg.SenderName}";
            }

            // Show censored indicator
            var content = msg.Content;
            if ((msg.Flags & ChatMessageFlags.Censored) != 0)
                content = $"{content} [filtered]";

            GUILayout.Label($"{prefix}: {content}", GetChannelStyle(msg.Channel));
            UnityEngine.GUI.color = oldColor;
        }

        private void DrawInput()
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();

            // Input field
            UnityEngine.GUI.SetNextControlName("ChatInput");
            _inputText = GUILayout.TextField(_inputText, 512);

            // Send button
            var canSend = !string.IsNullOrWhiteSpace(_inputText);
            UnityEngine.GUI.enabled = canSend;

            if (GUILayout.Button("Send", GUILayout.Width(50)) ||
                (canSend && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return))
            {
                SendMessage();
            }

            UnityEngine.GUI.enabled = true;
            GUILayout.EndHorizontal();
        }

        private void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(_inputText)) return;

            var chat = ChatManager.Instance;
            if (chat == null) return;

            chat.SendChatMessage(_selectedChannel, _inputText.Trim());
            _inputText = "";
            _autoScroll = true;

            // Keep focus on input
            UnityEngine.GUI.FocusControl("ChatInput");
        }

        private void DrawError()
        {
            // Show error for 3 seconds
            if (string.IsNullOrEmpty(_lastError) || Time.time - _errorTime > 3f)
                return;

            var oldColor = UnityEngine.GUI.color;
            UnityEngine.GUI.color = Color.red;
            GUILayout.Label(_lastError);
            UnityEngine.GUI.color = oldColor;
        }

        private static Color GetChannelColor(ChatChannel channel)
        {
            return channel switch
            {
                ChatChannel.Global => GlobalColor,
                ChatChannel.Room => RoomColor,
                ChatChannel.Team => TeamColor,
                ChatChannel.Whisper => WhisperColor,
                ChatChannel.System => SystemColor,
                _ => Color.white
            };
        }

        private static GUIStyle GetChannelStyle(ChatChannel channel)
        {
            var style = new GUIStyle(UnityEngine.GUI.skin.label)
            {
                wordWrap = true,
                richText = false,
                fontSize = 11
            };
            return style;
        }

        private static string GetMyName()
        {
            // Try to get player name from local player
            var localPlayer = Mirror.NetworkClient.localPlayer;
            if (localPlayer != null)
                return $"Player_{localPlayer.netId}";

            return "Me";
        }

        public void ClearData()
        {
            _displayMessages.Clear();
            _inputText = "";
            _lastError = null;
            _selectedChannel = ChatChannel.Room;
        }
    }
}
