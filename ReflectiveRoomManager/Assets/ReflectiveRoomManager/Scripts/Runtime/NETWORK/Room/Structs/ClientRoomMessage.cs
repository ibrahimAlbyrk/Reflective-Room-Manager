namespace REFLECTIVE.Runtime.NETWORK.Room.Structs
{
    using Enums;
    
    [System.Serializable]
    public struct ClientRoomMessage : Mirror.NetworkMessage
    {
        public readonly ClientRoomState ClientRoomState;
        
        public ClientRoomMessage(ClientRoomState clientRoomState)
        {
            ClientRoomState = clientRoomState;
        }
    }
}