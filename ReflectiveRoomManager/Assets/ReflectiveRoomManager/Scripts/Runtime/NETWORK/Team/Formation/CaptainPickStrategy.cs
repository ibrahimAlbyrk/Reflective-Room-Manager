using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Team.Formation
{
    /// <summary>
    /// Strategy where team captains take turns picking players.
    /// Useful for competitive scrims and tournaments.
    /// </summary>
    public class CaptainPickStrategy : ITeamFormationStrategy
    {
        public string StrategyName => "Captain Pick";

        private readonly List<NetworkConnection> _captains;
        private readonly List<NetworkConnection> _pickPool;
        private int _pickTurn;
        private bool _draftActive;

        public CaptainPickStrategy()
        {
            _captains = new List<NetworkConnection>();
            _pickPool = new List<NetworkConnection>();
            _pickTurn = 0;
            _draftActive = false;
        }

        /// <summary>
        /// Current pick turn index (which captain's turn it is).
        /// </summary>
        public int CurrentPickTurn => _pickTurn;

        /// <summary>
        /// Whether a draft is currently in progress.
        /// </summary>
        public bool IsDraftActive => _draftActive;

        /// <summary>
        /// Players waiting to be picked.
        /// </summary>
        public IReadOnlyList<NetworkConnection> PickPool => _pickPool;

        /// <summary>
        /// Sets the captains for each team.
        /// Captain order matches team order.
        /// </summary>
        public void SetCaptains(List<NetworkConnection> captains)
        {
            _captains.Clear();
            _captains.AddRange(captains);
            _pickTurn = 0;
        }

        /// <summary>
        /// Adds a player to the pick pool for drafting.
        /// </summary>
        public void AddToPickPool(NetworkConnection conn)
        {
            if (conn != null && !_pickPool.Contains(conn) && !_captains.Contains(conn))
                _pickPool.Add(conn);
        }

        /// <summary>
        /// Starts the draft process.
        /// </summary>
        public void StartDraft()
        {
            _draftActive = true;
            _pickTurn = 0;
        }

        /// <summary>
        /// Ends the draft process.
        /// </summary>
        public void EndDraft()
        {
            _draftActive = false;
            _pickPool.Clear();
        }

        /// <summary>
        /// Gets the captain whose turn it is to pick.
        /// </summary>
        public NetworkConnection GetCurrentPickingCaptain()
        {
            if (_captains.Count == 0) return null;
            return _captains[_pickTurn % _captains.Count];
        }

        /// <summary>
        /// Advances to the next captain's turn.
        /// </summary>
        public void AdvancePickTurn()
        {
            _pickTurn++;
        }

        public Team AssignPlayer(IReadOnlyList<Team> teams, NetworkConnection conn, TeamContext context)
        {
            if (teams == null || teams.Count == 0)
            {
                Debug.LogWarning("[CaptainPickStrategy] No teams available");
                return null;
            }

            // If draft not active, captains go to their teams
            if (!_draftActive)
            {
                var captainIndex = _captains.IndexOf(conn);
                if (captainIndex >= 0 && captainIndex < teams.Count)
                {
                    var captainTeam = teams[captainIndex];
                    if (!captainTeam.IsFull)
                        return captainTeam;
                }

                // Non-captain joining before draft - add to pool
                AddToPickPool(conn);
                return null; // Will be assigned during draft
            }

            // During draft - context should have the target team from captain's pick
            if (context?.PreferredTeamID.HasValue == true)
            {
                var pickedTeam = teams.FirstOrDefault(t => t.ID == context.PreferredTeamID.Value);
                if (pickedTeam != null && !pickedTeam.IsFull)
                {
                    _pickPool.Remove(conn);
                    AdvancePickTurn();
                    return pickedTeam;
                }
            }

            Debug.LogWarning("[CaptainPickStrategy] Invalid pick during draft");
            return null;
        }

        /// <summary>
        /// Makes a captain pick a specific player for their team.
        /// </summary>
        public bool MakePick(IReadOnlyList<Team> teams, NetworkConnection captain, NetworkConnection picked)
        {
            if (!_draftActive)
            {
                Debug.LogWarning("[CaptainPickStrategy] Draft is not active");
                return false;
            }

            var currentCaptain = GetCurrentPickingCaptain();
            if (currentCaptain != captain)
            {
                Debug.LogWarning("[CaptainPickStrategy] It's not this captain's turn to pick");
                return false;
            }

            if (!_pickPool.Contains(picked))
            {
                Debug.LogWarning("[CaptainPickStrategy] Picked player is not in the pool");
                return false;
            }

            // Find captain's team
            var captainIndex = _captains.IndexOf(captain);
            if (captainIndex < 0 || captainIndex >= teams.Count)
            {
                Debug.LogWarning("[CaptainPickStrategy] Captain's team not found");
                return false;
            }

            var team = teams[captainIndex];
            if (team.IsFull)
            {
                Debug.LogWarning("[CaptainPickStrategy] Captain's team is full");
                return false;
            }

            // Successful pick - actual team assignment handled by TeamManager
            _pickPool.Remove(picked);
            AdvancePickTurn();

            // Check if draft is complete
            if (_pickPool.Count == 0)
                EndDraft();

            return true;
        }
    }
}
