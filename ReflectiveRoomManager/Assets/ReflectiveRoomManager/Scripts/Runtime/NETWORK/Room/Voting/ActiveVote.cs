using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.Voting
{
    /// <summary>
    /// Represents an active vote in progress.
    /// </summary>
    public class ActiveVote
    {
        public string VoteID { get; }
        public IVoteType Type { get; }
        public NetworkConnection Initiator { get; }
        public string Question { get; }
        public string[] Options { get; }
        public float StartTime { get; }
        public float Duration { get; }
        public VoteContext Context { get; }

        /// <summary>Maps connection to option index</summary>
        public Dictionary<NetworkConnection, int> Votes { get; }

        public ActiveVote(
            IVoteType type,
            NetworkConnection initiator,
            string question,
            string[] options,
            float startTime,
            float duration,
            VoteContext context)
        {
            VoteID = Guid.NewGuid().ToString();
            Type = type;
            Initiator = initiator;
            Question = question;
            Options = options;
            StartTime = startTime;
            Duration = duration;
            Context = context;
            Votes = new Dictionary<NetworkConnection, int>();
        }

        #region Vote Data

        /// <summary>Total votes cast</summary>
        public int TotalVotes => Votes.Count;

        /// <summary>Time elapsed since vote started</summary>
        public float ElapsedTime => Time.time - StartTime;

        /// <summary>Time remaining for vote</summary>
        public float RemainingTime => Mathf.Max(0f, Duration - ElapsedTime);

        /// <summary>Checks if connection has voted</summary>
        public bool HasVoted(NetworkConnection conn) => Votes.ContainsKey(conn);

        /// <summary>Gets vote count for specific option</summary>
        public int GetVoteCount(int optionIndex)
        {
            var count = 0;
            foreach (var vote in Votes.Values)
            {
                if (vote == optionIndex)
                    count++;
            }
            return count;
        }

        /// <summary>Gets vote counts for all options</summary>
        public int[] GetVoteCounts()
        {
            var counts = new int[Options.Length];
            foreach (var vote in Votes.Values)
            {
                if (vote >= 0 && vote < Options.Length)
                    counts[vote]++;
            }
            return counts;
        }

        /// <summary>Gets participation rate (0-1)</summary>
        public float GetParticipationRate(int totalEligibleVoters)
        {
            if (totalEligibleVoters <= 0) return 0f;
            return (float)TotalVotes / totalEligibleVoters;
        }

        /// <summary>Gets all connections that voted for each option</summary>
        public Dictionary<int, List<NetworkConnection>> GetVotesByOption()
        {
            var result = new Dictionary<int, List<NetworkConnection>>();

            for (var i = 0; i < Options.Length; i++)
            {
                result[i] = new List<NetworkConnection>();
            }

            foreach (var kvp in Votes)
            {
                if (kvp.Value >= 0 && kvp.Value < Options.Length)
                    result[kvp.Value].Add(kvp.Key);
            }

            return result;
        }

        #endregion

        #region Result Calculation

        /// <summary>
        /// Calculates vote result based on current votes.
        /// </summary>
        public VoteResult CalculateResult(int totalEligibleVoters)
        {
            var voteCounts = GetVoteCounts();
            var participationRate = GetParticipationRate(totalEligibleVoters);

            // Check minimum participation
            if (participationRate < Type.MinParticipationRate)
            {
                return new VoteResult
                {
                    WinningOption = -1,
                    VoteCounts = voteCounts,
                    ParticipationRate = participationRate,
                    Passed = false,
                    Reason = VoteEndReason.TimerExpired,
                    VotesByOption = GetVotesByOption()
                };
            }

            // Find winning option
            var winningOption = GetWinningOption(voteCounts);

            // Check if winning option meets threshold
            var winningPercentage = TotalVotes > 0 ? (float)voteCounts[Mathf.Max(0, winningOption)] / TotalVotes : 0f;
            var passed = winningOption >= 0 && winningPercentage >= Type.WinningThreshold;

            return new VoteResult
            {
                WinningOption = winningOption,
                VoteCounts = voteCounts,
                ParticipationRate = participationRate,
                Passed = passed,
                Reason = VoteEndReason.TimerExpired,
                VotesByOption = GetVotesByOption()
            };
        }

        /// <summary>
        /// Determines winning option (handles ties).
        /// </summary>
        private int GetWinningOption(int[] voteCounts)
        {
            if (voteCounts.Length == 0) return -1;

            var maxVotes = voteCounts.Max();
            var tiedOptions = voteCounts
                .Select((count, index) => new { Count = count, Index = index })
                .Where(x => x.Count == maxVotes)
                .Select(x => x.Index)
                .ToArray();

            // No tie
            if (tiedOptions.Length == 1)
                return tiedOptions[0];

            // Handle tie based on resolution mode
            return Type.TieResolution switch
            {
                TieResolutionMode.Fail => -1,
                TieResolutionMode.FirstOption => tiedOptions[0],
                TieResolutionMode.LastOption => tiedOptions[^1],
                TieResolutionMode.Random => tiedOptions[UnityEngine.Random.Range(0, tiedOptions.Length)],
                TieResolutionMode.InitiatorChoice => Votes.TryGetValue(Initiator, out var choice) ? choice : -1,
                _ => -1
            };
        }

        #endregion
    }
}
