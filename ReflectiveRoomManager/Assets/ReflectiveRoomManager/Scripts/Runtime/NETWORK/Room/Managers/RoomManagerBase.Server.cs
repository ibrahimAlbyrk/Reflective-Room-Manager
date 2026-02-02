using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    public abstract partial class RoomManagerBase
    {
        protected virtual void OnStartServer()
        {
            _connectionManager.RoomConnections.AddRegistersForServer();
        }

        protected virtual void OnStopServer()
        {
            RemoveAllRoom(forced:true);
        }

        protected virtual void OnStartClient()
        {
            _connectionManager.RoomConnections.AddRegistersForClient();
        }

        protected virtual void OnStopClient()
        {
        }

        protected virtual void OnServerConnect(NetworkConnection conn)
        {
            SendUpdateRoomListForClient(conn);
        }

        protected virtual void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            ExitRoom(conn, true);
        }

        protected virtual void OnClientConnect()
        {
        }

        protected virtual void OnClientDisconnect()
        {
        }
    }
}