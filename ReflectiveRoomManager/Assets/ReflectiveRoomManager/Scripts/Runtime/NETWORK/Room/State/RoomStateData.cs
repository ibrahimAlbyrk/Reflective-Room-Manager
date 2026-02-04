using System;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Room.State
{
    /// <summary>
    /// Network-serializable state data.
    /// Sent from server to clients to sync state changes.
    /// </summary>
    [Serializable]
    public struct RoomStateData
    {
        /// <summary>State type ID (0-255)</summary>
        public byte StateTypeID;

        /// <summary>Time elapsed in current state</summary>
        public float ElapsedTime;

        /// <summary>State-specific data (serialized as string)</summary>
        public Dictionary<string, string> Data;

        public RoomStateData(byte stateTypeID, float elapsedTime, Dictionary<string, string> data = null)
        {
            StateTypeID = stateTypeID;
            ElapsedTime = elapsedTime;
            Data = data ?? new Dictionary<string, string>();
        }
    }
}
