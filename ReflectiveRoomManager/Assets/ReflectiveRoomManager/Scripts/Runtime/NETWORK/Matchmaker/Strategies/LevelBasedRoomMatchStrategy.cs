using UnityEngine;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.NETWORK.Matchmaker
{
    public class LevelBasedRoomMatchStrategy : RoomMatchStrategy
    {
        protected override string CreateMatchData(Dictionary<string, string> data)
        {
            var matchData = new MatchData
            {
                Type = "LevelBased",
                Data = data
            };

            return JsonUtility.ToJson(matchData);
        }

        protected override bool IsSameMatch(Dictionary<string, string> data, string otherData)
        {
            var matchData = JsonUtility.FromJson<MatchData>(otherData);
            
            if (matchData.Type != "LevelBased") return false;

            var level = int.Parse(matchData.Data["Level"]);

            var otherLevel = int.Parse(data["Level"]);
            
            return level == otherLevel;
        }
    }
}