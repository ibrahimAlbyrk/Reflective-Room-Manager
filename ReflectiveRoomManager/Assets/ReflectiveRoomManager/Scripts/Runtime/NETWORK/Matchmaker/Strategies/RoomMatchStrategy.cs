using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Matchmaker
{
    using Room;
    using Room.Service;
    
    public abstract class RoomMatchStrategy
    {
        public class MatchData
        {
            public string Type;
            public Dictionary<string, string> Data;
        }
        
        protected MatchInfo _matchInfo;

        public void Init(MatchInfo roomInfo)
        {
            _matchInfo = roomInfo;
        }
        
        public virtual Room Create(uint ID, Dictionary<string, string> data)
        {
            var roomName = $"Room_{ID}";

            var matchData = CreateMatchData(data);

            var customData = new Dictionary<string, string>
            {
                {"Math", matchData}
            };
            
            RoomServer.CreateRoom(roomName, _matchInfo.Scene, _matchInfo.MaxPlayerCount, customData);
            
            var room = RoomManagerBase.Instance.GetRoom(roomName);

            return room;
        }
        
        public virtual bool Find(Room room, Dictionary<string,string> data)
        {
            if(room.IsPrivate) return false;
                
            if(room.CurrentPlayers < room.MaxPlayers) return false;

            var customData = room.GetCustomData();

            if (!customData.TryGetValue("Match", out var matchData)) return false;

            return IsSameMatch(data, matchData);
        }

        protected abstract string CreateMatchData(Dictionary<string, string> data);
        protected abstract bool IsSameMatch(Dictionary<string, string> data, string otherData);

    }
}