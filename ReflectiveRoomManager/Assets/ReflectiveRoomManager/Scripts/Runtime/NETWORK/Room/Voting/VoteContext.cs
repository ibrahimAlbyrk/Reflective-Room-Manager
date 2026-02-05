using Mirror;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Room.Voting
{
    /// <summary>
    /// Context passed to vote type methods.
    /// Provides rich data for dynamic behavior.
    /// </summary>
    public class VoteContext
    {
        public Room Room { get; }
        public NetworkConnection Initiator { get; }
        public object CustomData { get; set; }

        /// <summary>Generic data storage for vote types</summary>
        public Dictionary<string, object> Data { get; }

        public VoteContext(Room room, NetworkConnection initiator, object customData = null)
        {
            Room = room;
            Initiator = initiator;
            CustomData = customData;
            Data = new Dictionary<string, object>();
        }

        public T GetData<T>(string key, T defaultValue = default)
        {
            if (Data.TryGetValue(key, out var value) && value is T typedValue)
                return typedValue;
            return defaultValue;
        }

        public void SetData(string key, object value)
        {
            Data[key] = value;
        }
    }
}
