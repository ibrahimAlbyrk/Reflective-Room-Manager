namespace REFLECTIVE.Runtime.NETWORK.Room.Structs
{
    using Enums;
    
    [System.Serializable]
    public struct ClientRoomMessage : Mirror.NetworkMessage
    {
        public int ConnectionId;
        
        public string SceneName;
        
        public readonly ClientRoomState ClientRoomState;

        public ClientRoomMessage(string sceneName, ClientRoomState clientRoomState, int connectionId)
        {
            SceneName = sceneName;
            ClientRoomState = clientRoomState;
            ConnectionId = connectionId;
        }
        
        public ClientRoomMessage(ClientRoomState clientRoomState, int connectionId)
        {
            SceneName = null;
            ClientRoomState = clientRoomState;
            ConnectionId = connectionId;
        }
    }
}