using UnityEngine;
using REFLECTIVE.Runtime.NETWORK.Room;
using REFLECTIVE.Runtime.NETWORK.Room.Service;

namespace Example.Basic.Game
{
    public class RoomSceneChange : MonoBehaviour
    {
        private void OnGUI()
        {
            const float buttonWidth = 200f;
            const float buttonHeight = 50f;

            var rect = new Rect(Screen.width / 2f - buttonWidth / 2, 0, buttonWidth,
                buttonHeight);
            
            if (GUI.Button(rect,"Reload The Scene"))
            {
                ChangeScene();
            }
        }
        
        private void ChangeScene()
        {
            var scene = gameObject.scene;
            
            var room = RoomManagerBase.Instance.GetRoomOfScene(scene);
            
            RoomServer.ChangeScene(room, RoomManagerBase.Instance.RoomScene, true);
        }
    }
}