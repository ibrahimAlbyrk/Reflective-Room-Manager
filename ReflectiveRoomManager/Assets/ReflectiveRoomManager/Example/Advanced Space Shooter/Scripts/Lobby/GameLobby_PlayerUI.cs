using TMPro;
using UnityEngine;

namespace Examples.SpaceShooter.Lobby
{
    public class GameLobby_PlayerUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _usernameText;

        public string GetUsername() => _usernameText.text;
        
        public void Init(string username)
        {
            _usernameText.text = username;
        }
    }
}