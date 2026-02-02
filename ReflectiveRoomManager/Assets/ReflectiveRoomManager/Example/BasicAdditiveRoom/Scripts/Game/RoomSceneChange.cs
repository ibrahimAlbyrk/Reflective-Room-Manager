using Mirror;
using UnityEngine;
using REFLECTIVE.Runtime.NETWORK.Room;
using REFLECTIVE.Runtime.NETWORK.Room.Service;

namespace Examples.Basic.Game
{
    public class RoomSceneChange : NetworkBehaviour
    {
        private void OnGUI()
        {
            const float buttonWidth = 200f;
            const float buttonHeight = 50f;

            var rect = new Rect(Screen.width / 2f - buttonWidth / 2, 0, buttonWidth,
                buttonHeight);
            
            if (GUI.Button(rect,"Reload The Scene"))
            {
                if (isServer)
                    ChangeScene();
                else
                    ChangeScene_CMD();
            }
        }


        [Command(requiresAuthority = false)]
        private void ChangeScene_CMD() => ChangeScene();
        
        private void ChangeScene()
        {
            var scene = gameObject.scene;
            
            var room = RoomManagerBase.Instance.GetRoomByScene(scene);
            
            RoomServer.ChangeScene(room, RoomManagerBase.Instance.RoomScene, true);
        }
    }
}