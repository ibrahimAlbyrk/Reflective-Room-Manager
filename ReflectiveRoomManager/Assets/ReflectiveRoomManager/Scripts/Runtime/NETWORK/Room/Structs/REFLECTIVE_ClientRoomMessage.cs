namespace REFLECTIVE.Runtime.NETWORK.Room.Structs
{
    using Enums;
    
    [System.Serializable]
    public struct REFLECTIVE_ClientRoomMessage : Mirror.NetworkMessage
    {
        public int ConnectionId;
        
        public string SceneName;
        
        public readonly REFLECTIVE_ClientRoomState ClientRoomState;

        public REFLECTIVE_ClientRoomMessage(string sceneName, REFLECTIVE_ClientRoomState clientRoomState, int connectionId)
        {
            SceneName = sceneName;
            ClientRoomState = clientRoomState;
            ConnectionId = connectionId;
        }
        
        public REFLECTIVE_ClientRoomMessage(REFLECTIVE_ClientRoomState clientRoomState, int connectionId)
        {
            SceneName = null;
            ClientRoomState = clientRoomState;
            ConnectionId = connectionId;
        }
    }
}