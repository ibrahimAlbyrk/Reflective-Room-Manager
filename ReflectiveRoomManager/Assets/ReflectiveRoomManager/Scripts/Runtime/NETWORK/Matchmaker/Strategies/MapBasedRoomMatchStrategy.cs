using System.Collections.Generic;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Matchmaker
{
    public class MapBasedRoomMatchStrategy : RoomMatchStrategy
    {
        protected override string CreateMatchData(Dictionary<string, string> data)
        {
            var matchData = new MatchData
            {
                Type = "MapBased",
                Data = data
            };

            return JsonUtility.ToJson(matchData);
        }

        protected override bool IsSameMatch(Dictionary<string, string> data, string otherData)
        {
            var matchData = JsonUtility.FromJson<MatchData>(otherData);

            if (matchData.Type != "MapBased") return false;

            return string.Equals(matchData.Data["Map"], data["Map"]);
        }
    }
}