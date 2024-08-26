using System.Collections.Generic;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Matchmaker
{
    public class NoneRoomMatchStrategy : RoomMatchStrategy
    {
        protected override string CreateMatchData(Dictionary<string, string> data)
        {
            var matchData = new MatchData
            {
                Type = "None",
                Data = null
            };

            return JsonUtility.ToJson(matchData);
        }

        protected override bool IsSameMatch(Dictionary<string, string> data, string otherData)
        {
            return true;
        }
    }
}