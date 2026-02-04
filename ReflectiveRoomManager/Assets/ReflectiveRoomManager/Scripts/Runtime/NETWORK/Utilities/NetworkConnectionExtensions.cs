using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Utilities
{
    /// <summary>
    /// Extension methods for NetworkConnection to provide consistent access to connection ID.
    /// </summary>
    public static class NetworkConnectionExtensions
    {
        /// <summary>
        /// Gets the connection ID from a NetworkConnection.
        /// Works with both NetworkConnectionToClient and other connection types.
        /// </summary>
        public static int GetConnectionId(this NetworkConnection conn)
        {
            if (conn == null) return -1;
            return (conn as NetworkConnectionToClient)?.connectionId ?? conn.GetHashCode();
        }
    }
}
