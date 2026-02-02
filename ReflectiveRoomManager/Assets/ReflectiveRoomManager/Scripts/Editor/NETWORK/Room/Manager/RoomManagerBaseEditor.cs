using Mirror;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace REFLECTIVE.Editor.NETWORK.Room.Manager
{
    using Utilities;
    using Runtime.NETWORK.Room;
    using Runtime.NETWORK.Room.Loader;
    using Runtime.NETWORK.Room.Service;

    [CustomEditor(typeof(RoomManagerBase), true)]
    public class RoomManagerBaseEditor : ReflectiveEditorBase
    {
        private RoomManagerBase _roomManager;

        private Foldout _roomListFoldout;
        private ScrollView _roomScroll;
        private TextField _searchField;
        private PopupField<string> _searchTypeField;
        private VisualElement _propertiesContainer;
        private Label _warningBox;
        private Label _runtimeBox;

        private readonly HashSet<string> _expandedRooms = new();

        protected override string GetTitle() => "REFLECTIVE ROOM MANAGER";

        protected override void BuildInspectorUI(VisualElement root)
        {
            _roomManager = (RoomManagerBase)target;

            _warningBox = CreateHelpBox(
                "Interest manager is not attached! If scene interest management is not added while additive room mode is selected, players may interfere with each other.",
                true);
            root.Add(_warningBox);

            _runtimeBox = CreateHelpBox("You cannot make changes during runtime");
            root.Add(_runtimeBox);

            _propertiesContainer = new VisualElement();
            AddDefaultProperties(_propertiesContainer);
            root.Add(_propertiesContainer);

            BuildRoomListUI(root);

            var removeAllBtn = CreateButton("Remove All Rooms", () => RoomServer.RemoveAllRoom(true));
            removeAllBtn.AddToClassList("reflective-button--danger");
            root.Add(removeAllBtn);

            UpdateRuntimeState();

            root.schedule.Execute(UpdateRuntimeState).Every(500);
        }

        private void BuildRoomListUI(VisualElement root)
        {
            _roomListFoldout = new Foldout { text = "Room List (0)" };
            _roomListFoldout.AddToClassList("reflective-foldout");

            var searchToolbar = new VisualElement();
            searchToolbar.AddToClassList("search-toolbar");

            searchToolbar.Add(new Label("Search:"));

            _searchField = new TextField();
            _searchField.style.flexGrow = 1;
            _searchField.RegisterValueChangedCallback(_ => RefreshRoomList());
            searchToolbar.Add(_searchField);

            searchToolbar.Add(new Label("Type:"));

            _searchTypeField = new PopupField<string>(
                new List<string> { "name", "Client ID" }, 0);
            _searchTypeField.style.width = 90;
            _searchTypeField.RegisterValueChangedCallback(_ => RefreshRoomList());
            searchToolbar.Add(_searchTypeField);

            _roomListFoldout.Add(searchToolbar);

            _roomScroll = new ScrollView(ScrollViewMode.Vertical);
            _roomScroll.AddToClassList("room-scroll");
            _roomListFoldout.Add(_roomScroll);

            root.Add(_roomListFoldout);

            root.schedule.Execute(RefreshRoomList).Every(500);
        }

        private void RefreshRoomList()
        {
            if (_roomManager == null || _roomScroll == null) return;

            var rooms = _roomManager.GetRooms().ToArray();

            _roomListFoldout.text = $"Room List ({rooms.Length})";

            _roomScroll.Clear();

            var filter = _searchField?.value ?? "";
            var searchByName = _searchTypeField?.index == 0;

            foreach (var room in rooms)
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    if (searchByName)
                    {
                        if (!room.Name.Contains(filter))
                            continue;
                    }
                    else
                    {
                        if (int.TryParse(filter, out var id))
                            if (room.Connections.All(conn => conn.connectionId != id))
                                continue;
                    }
                }

                var card = new VisualElement();
                card.AddToClassList("room-card");

                var headerRow = new VisualElement();
                headerRow.AddToClassList("room-header-row");

                var roomName = room.Name;
                var isExpanded = _expandedRooms.Contains(roomName);

                var roomFoldout = new Foldout { text = $"Room: {roomName}", value = isExpanded };

                roomFoldout.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                        _expandedRooms.Add(roomName);
                    else
                        _expandedRooms.Remove(roomName);
                });

                var countLabel = new Label($"{room.CurrentPlayers}/{room.MaxPlayers}");
                countLabel.AddToClassList("room-player-count");

                var removeBtn = CreateButton("Remove", () => RoomServer.RemoveRoom(roomName, true));
                removeBtn.AddToClassList("reflective-button--small");
                removeBtn.style.width = 80;

                headerRow.Add(roomFoldout);
                headerRow.Add(countLabel);
                headerRow.Add(removeBtn);

                card.Add(headerRow);

                var detailId = new Label($"Room ID: {room.ID}");
                detailId.AddToClassList("room-detail-label");

                var detailPrivate = new Label($"Room Is Private: {room.IsPrivate}");
                detailPrivate.AddToClassList("room-detail-label");

                roomFoldout.Add(detailId);
                roomFoldout.Add(detailPrivate);

                _roomScroll.Add(card);
            }
        }

        private void UpdateRuntimeState()
        {
            if (_roomManager == null) return;

            var isPlaying = EditorApplication.isPlaying;

            _runtimeBox.style.display = isPlaying ? DisplayStyle.Flex : DisplayStyle.None;
            _propertiesContainer.SetEnabled(!isPlaying);

            var hasInterestManager = _roomManager.gameObject.TryGetComponent(out SceneInterestManagement _);
            var isAdditive = _roomManager.RoomLoaderType == RoomLoaderType.AdditiveScene;

            _warningBox.style.display = (!hasInterestManager && isAdditive)
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }
    }
}
