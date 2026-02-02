namespace REFLECTIVE.Runtime.NETWORK.Room.Structs
{
    [System.Serializable]
    public struct ServerShutdownWarningMessage : Mirror.NetworkMessage
    {
        public readonly float SecondsRemaining;

        public ServerShutdownWarningMessage(float secondsRemaining)
        {
            SecondsRemaining = secondsRemaining;
        }
    }
}
