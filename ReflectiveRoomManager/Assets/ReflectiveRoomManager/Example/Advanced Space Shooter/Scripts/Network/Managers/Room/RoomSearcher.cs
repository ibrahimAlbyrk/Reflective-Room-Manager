using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using REFLECTIVE.Runtime.NETWORK.Room;
using REFLECTIVE.Runtime.NETWORK.Room.Structs;

namespace Examples.SpaceShooter.Network.Managers.Rooms
{
    using Utilities;
    
    public class RoomSearcher : MonoBehaviour
    {
        [SerializeField] private Transform _roomsContent;
        [SerializeField] private GameObject _roomFieldPrefab;

        [SerializeField] private Button _exitButton;

        private readonly List<RoomField_UI> _roomList = new();

        public void UpdateUI()
        {
            var rooms = RoomManagerBase.Instance?.GetRoomInfos();

            if (rooms == null) return;

            foreach (var room in _roomList.ToList())
            {
                _roomList.Remove(room);
                room.gameObject.Destroy();
            }

            foreach (var room in rooms.Where(room => room.RoomName != "OpenWorld"))
            {
                CreateRoomField(room);
            }
        }

        private void CreateRoomField(RoomInfo roomInfo)
        {
            var roomField = Instantiate(_roomFieldPrefab, _roomsContent).GetComponent<RoomField_UI>();

            if (roomField == null) return;

            roomField.Init(roomInfo.RoomName, roomInfo.CurrentPlayers, roomInfo.MaxPlayers);

            _roomList.Add(roomField);
        }

        private void RemoveRoomField(RoomField_UI roomField)
        {
            _roomList.Remove(roomField);
            roomField.gameObject.Destroy();
        }

        private void UpdateRoomField(RoomInfo roomInfo)
        {
            if (!gameObject.activeSelf) return;

            var roomName = roomInfo.RoomName;
            
            var roomFieldUI = _roomList.FirstOrDefault(field => field.GetRoomName() == roomName);

            if (roomFieldUI == null)
            {
                CreateRoomField(roomInfo);
                return;
            }
            
            if (roomInfo.CurrentPlayers < 1)
            {
                RemoveRoomField(roomFieldUI);
                return;
            }
            
            roomFieldUI.Init(roomInfo.RoomName, roomInfo.CurrentPlayers, roomInfo.MaxPlayers);
        }

        private void Start()
        {
            _exitButton.onClick.AddListener(() => gameObject.SetActive(false));
            RoomManagerBase.Instance.Events.OnServerCreatedRoom += UpdateRoomField;
        }
    }
}