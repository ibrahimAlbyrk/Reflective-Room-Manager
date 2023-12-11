using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    using Connection.Manager;

    public abstract partial class RoomManagerBase
    {
        protected virtual void OnStartServer()
        {
            ReflectiveConnectionManager.roomConnections.AddRegistersForServer();
        }

        protected virtual void OnStopServer()
        {
            RemoveAllRoom(forced:true);
        }

        protected virtual void OnStartClient()
        {
            ReflectiveConnectionManager.roomConnections.AddRegistersForClient();
        }

        protected virtual void OnStopClient()
        {
        }

        protected virtual void OnServerConnect(NetworkConnection conn)
        {
            SendUpdateRoomListForClient(conn);

            SendConnectionMessageToClient(conn);
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