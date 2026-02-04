using System.Collections.Generic;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.GUI.Modules
{
    using Structs;
    using Discovery;
    using Discovery.Handlers;
    using Service;

    /// <summary>
    /// HUD module for room discovery and filtering.
    /// Used in lobby to browse/filter/sort rooms.
    /// </summary>
    public class DiscoveryHUDModule
    {
        // Filter state
        private string _searchText = "";
        private RoomSortOptions _sortOption = RoomSortOptions.None;
        private bool _excludeFull;
        private bool _excludeEmpty;

        // Pagination
        private int _currentPage = 1;
        private int _totalPages = 1;
        private int _totalRooms;
        private const int PageSize = 10;

        // Results
        private List<RoomInfo> _rooms = new();
        private Vector2 _scrollPos;
        private uint _lastCacheVersion;
        private bool _isLoading;

        // Sort option names for display
        private static readonly string[] SortNames =
        {
            "Default", "Players ↑", "Players ↓",
            "Name A-Z", "Name Z-A", "Oldest", "Newest"
        };

        private bool _showSortDropdown;

        public void RegisterEvents()
        {
            RoomDiscoveryNetworkHandlers.OnClientRoomQueryResponseReceived += OnQueryResponse;
            RoomDiscoveryNetworkHandlers.OnClientRoomDeltaUpdateReceived += OnDeltaUpdate;
        }

        public void UnregisterEvents()
        {
            RoomDiscoveryNetworkHandlers.OnClientRoomQueryResponseReceived -= OnQueryResponse;
            RoomDiscoveryNetworkHandlers.OnClientRoomDeltaUpdateReceived -= OnDeltaUpdate;
        }

        private void OnQueryResponse(RoomQueryResponse response)
        {
            _isLoading = false;
            _rooms = new List<RoomInfo>(response.Rooms);
            _totalPages = Mathf.Max(1, response.TotalPages);
            _totalRooms = response.TotalRoomCount;
            _currentPage = response.CurrentPage;
            _lastCacheVersion = response.CacheVersion;
        }

        private void OnDeltaUpdate(RoomDeltaUpdate update)
        {
            if (update.CacheVersion > _lastCacheVersion)
            {
                // Cache outdated - refresh
                SendQuery();
                return;
            }

            // Apply delta locally
            switch (update.ChangeType)
            {
                case RoomChangeType.Added:
                    // Re-query to get correct pagination
                    SendQuery();
                    break;
                case RoomChangeType.Updated:
                    var idx = _rooms.FindIndex(r => r.ID == update.RoomData.ID);
                    if (idx >= 0) _rooms[idx] = update.RoomData;
                    break;
                case RoomChangeType.Removed:
                    _rooms.RemoveAll(r => r.ID == update.RoomData.ID);
                    break;
            }
        }

        public void DrawPanel(float x, float y, float width, float height)
        {
            HUDStyles.Init();

            GUILayout.BeginArea(new Rect(x, y, width, height), HUDStyles.BoxStyle);

            // Header
            GUILayout.Label("FIND ROOMS", HUDStyles.HeaderStyle);
            GUILayout.Space(5);

            // Search bar
            DrawSearchBar();

            // Filter options
            DrawFilterOptions();

            GUILayout.Space(5);

            // Room list
            DrawRoomList(height - 180);

            // Pagination
            DrawPagination();

            GUILayout.EndArea();
        }

        private void DrawSearchBar()
        {
            GUILayout.BeginHorizontal();

            var newSearch = GUILayout.TextField(_searchText, GUILayout.Height(22));
            if (newSearch != _searchText)
            {
                _searchText = newSearch;
                _currentPage = 1;
            }

            if (GUILayout.Button("Search", GUILayout.Width(55), GUILayout.Height(22)))
            {
                _currentPage = 1;
                SendQuery();
            }

            GUILayout.EndHorizontal();
        }

        private void DrawFilterOptions()
        {
            GUILayout.BeginHorizontal();

            // Sort dropdown button
            GUILayout.Label("Sort:", GUILayout.Width(30));
            if (GUILayout.Button(SortNames[(int)_sortOption], GUILayout.Width(80)))
            {
                _showSortDropdown = !_showSortDropdown;
            }

            GUILayout.Space(5);

            // Toggles
            var newExcludeFull = GUILayout.Toggle(_excludeFull, "Hide Full");
            if (newExcludeFull != _excludeFull)
            {
                _excludeFull = newExcludeFull;
                _currentPage = 1;
                SendQuery();
            }

            GUILayout.Space(5);

            var newExcludeEmpty = GUILayout.Toggle(_excludeEmpty, "Hide Empty");
            if (newExcludeEmpty != _excludeEmpty)
            {
                _excludeEmpty = newExcludeEmpty;
                _currentPage = 1;
                SendQuery();
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label($"Total: {_totalRooms}");

            GUILayout.EndHorizontal();

            // Sort dropdown options
            if (_showSortDropdown)
            {
                DrawSortDropdown();
            }
        }

        private void DrawSortDropdown()
        {
            GUILayout.BeginVertical(UnityEngine.GUI.skin.box);
            for (var i = 0; i < SortNames.Length; i++)
            {
                var isSelected = (int)_sortOption == i;
                var style = isSelected ? HUDStyles.TabActiveStyle : UnityEngine.GUI.skin.button;

                if (GUILayout.Button(SortNames[i], style, GUILayout.Height(20)))
                {
                    _sortOption = (RoomSortOptions)i;
                    _showSortDropdown = false;
                    _currentPage = 1;
                    SendQuery();
                }
            }
            GUILayout.EndVertical();
        }

        private void DrawRoomList(float listHeight)
        {
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(listHeight));

            if (_isLoading)
            {
                GUILayout.Label("Loading...", HUDStyles.HeaderStyle);
            }
            else if (_rooms.Count == 0)
            {
                GUILayout.Label("No rooms found", HUDStyles.HeaderStyle);
            }
            else
            {
                foreach (var room in _rooms)
                {
                    DrawRoomEntry(room);
                }
            }

            GUILayout.EndScrollView();
        }

        private void DrawRoomEntry(RoomInfo room)
        {
            GUILayout.BeginHorizontal(UnityEngine.GUI.skin.box);

            // Room name
            GUILayout.Label(room.RoomName, GUILayout.Width(100));

            // Player count with color
            var isFull = room.CurrentPlayers >= room.MaxPlayers;
            var old = UnityEngine.GUI.color;
            UnityEngine.GUI.color = isFull ? Color.red : (room.CurrentPlayers > 0 ? Color.green : Color.gray);
            GUILayout.Label($"{room.CurrentPlayers}/{room.MaxPlayers}", GUILayout.Width(45));
            UnityEngine.GUI.color = old;

            // Scene name (truncated)
            var sceneName = room.SceneName?.Length > 12 ? room.SceneName[..12] + ".." : room.SceneName;
            GUILayout.Label(sceneName ?? "-", GUILayout.Width(80));

            // Private indicator
            if (room.IsPrivate)
            {
                old = UnityEngine.GUI.color;
                UnityEngine.GUI.color = Color.yellow;
                GUILayout.Label("[P]", GUILayout.Width(25));
                UnityEngine.GUI.color = old;
            }
            else
            {
                GUILayout.Label("", GUILayout.Width(25));
            }

            // Join button
            GUILayout.FlexibleSpace();
            if (!isFull && GUILayout.Button("Join", GUILayout.Width(40)))
            {
                RoomClient.JoinRoom(room.RoomName);
            }

            GUILayout.EndHorizontal();
        }

        private void DrawPagination()
        {
            GUILayout.BeginHorizontal();

            // Prev button
            UnityEngine.GUI.enabled = _currentPage > 1;
            if (GUILayout.Button("<", GUILayout.Width(30)))
            {
                _currentPage--;
                SendQuery();
            }

            UnityEngine.GUI.enabled = true;

            // Page info
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Page {_currentPage}/{_totalPages}");
            GUILayout.FlexibleSpace();

            // Next button
            UnityEngine.GUI.enabled = _currentPage < _totalPages;
            if (GUILayout.Button(">", GUILayout.Width(30)))
            {
                _currentPage++;
                SendQuery();
            }

            UnityEngine.GUI.enabled = true;

            // Refresh button
            if (GUILayout.Button("Refresh", GUILayout.Width(55)))
            {
                SendQuery();
            }

            GUILayout.EndHorizontal();
        }

        public void SendQuery()
        {
            _isLoading = true;

            var filter = RoomFilter.Create();

            if (!string.IsNullOrEmpty(_searchText))
                filter.WithTextSearch(_searchText);

            if (_excludeFull)
                filter.ExcludeFull();

            if (_excludeEmpty)
                filter.ExcludeEmpty();

            RoomDiscoveryNetworkHandlers.SendQueryRequest(
                filter,
                _sortOption,
                _currentPage,
                PageSize
            );
        }

        public void ClearData()
        {
            _searchText = "";
            _sortOption = RoomSortOptions.None;
            _excludeFull = false;
            _excludeEmpty = false;
            _currentPage = 1;
            _totalPages = 1;
            _totalRooms = 0;
            _rooms.Clear();
            _isLoading = false;
            _showSortDropdown = false;
        }
    }
}
