using TMPro;
using UnityEngine;
using UnityEngine.UI;
using REFLECTIVE.Runtime.NETWORK.Room.Service;

namespace Examples.SpaceShooter.Network.Managers.Rooms
{
    public class RoomCreator : MonoBehaviour
    {
        [Header("Fields")]
        [SerializeField] private TMP_InputField _roomNameField;
        [SerializeField] private TMP_InputField _maxPlayerField;
        
        [Header("Buttons")]
        [SerializeField] private Button _createRoomButton;
        [SerializeField] private Button _exitButton;
        
        private void CreateRoom()
        {
            var roomName = _roomNameField.text;
            var maxPlayers = int.Parse(_maxPlayerField.text);
            
            const string sceneName = "OpenWorld_Scene";
            
            RoomClient.CreateRoom(roomName, sceneName, maxPlayers);
        }

        private void ButtonEnableHandler()
        {
            var isInteractible = !string.IsNullOrEmpty(_roomNameField.text) && !string.IsNullOrEmpty(_maxPlayerField.text);
            
            _createRoomButton.interactable = isInteractible;
        }

        private void OnRoomNameValueChanged(string _) => ButtonEnableHandler();

        private void OnMaxPlayerValueChanged(string value)
        {
            if (int.TryParse(value, out var intValue))
            {
                _maxPlayerField.text = intValue switch
                {
                    > 30 => "30",
                    < 2 => "2",
                    _ => _maxPlayerField.text
                };
            }
            
            ButtonEnableHandler();
        }

        private void Awake()
        {
            _createRoomButton.interactable = false;
            
            _exitButton?.onClick.AddListener(() => gameObject.SetActive(false));
            _createRoomButton?.onClick.AddListener(CreateRoom);
            
            _roomNameField?.onValueChanged.AddListener(OnRoomNameValueChanged);
            _maxPlayerField?.onValueChanged.AddListener(OnMaxPlayerValueChanged);
        }
    }
}