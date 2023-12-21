using TMPro;
using UnityEngine;
using UnityEngine.UI;
using REFLECTIVE.Runtime.NETWORK.Room.Service;

namespace Examples.SpaceShooter.Network.Managers.Rooms
{
    public class RoomField_UI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _countText;
        [SerializeField] private TMP_Text _nameText;

        [SerializeField] private Button _connectButton;

        public string GetRoomName() => _nameText.text;
        
        public void Init(string roomName, int currentPlayerCount, int maxPlayerCount)
        {
            _countText.text = $"{currentPlayerCount}/{maxPlayerCount}";
            _nameText.text = roomName;
        }

        private void OnConnect()
        {
            var roomName = _nameText.text;
            
            if(string.IsNullOrEmpty(roomName)) return;
            
            RoomClient.JoinRoom(roomName);
        }

        private void Awake()
        {
            _connectButton?.onClick.AddListener(OnConnect);
        }
    }
}