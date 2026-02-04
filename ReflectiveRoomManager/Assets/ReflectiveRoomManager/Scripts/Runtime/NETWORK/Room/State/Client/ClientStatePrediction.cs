using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.State.Client
{
    /// <summary>
    /// Client-side state prediction and rollback.
    /// Predicts timer values between server syncs for smooth UI.
    /// </summary>
    public class ClientStatePrediction
    {
        private const float DRIFT_THRESHOLD = 0.1f; // 100ms
        private const float INTERPOLATION_SPEED = 5f;

        private float _predictedElapsed;
        private float _serverElapsed;
        private float _interpolatedElapsed;
        private byte _currentStateID;
        private bool _isPredicting;

        /// <summary>Smoothly interpolated elapsed time for UI</summary>
        public float DisplayElapsed => _interpolatedElapsed;

        /// <summary>Raw predicted value</summary>
        public float PredictedElapsed => _predictedElapsed;

        /// <summary>Last known server value</summary>
        public float ServerElapsed => _serverElapsed;

        /// <summary>Current state ID</summary>
        public byte CurrentStateID => _currentStateID;

        /// <summary>
        /// Called when state changes (from server).
        /// Resets prediction.
        /// </summary>
        public void OnStateChange(byte stateID, float serverElapsed)
        {
            _currentStateID = stateID;
            _serverElapsed = serverElapsed;
            _predictedElapsed = serverElapsed;
            _interpolatedElapsed = serverElapsed;
            _isPredicting = true;
        }

        /// <summary>
        /// Called when sync message received (periodic).
        /// Corrects drift if needed.
        /// </summary>
        public void OnStateSync(byte stateID, float serverElapsed)
        {
            if (stateID != _currentStateID)
            {
                // State mismatch - wait for state change message
                return;
            }

            _serverElapsed = serverElapsed;

            var drift = Mathf.Abs(_predictedElapsed - serverElapsed);

            if (drift > DRIFT_THRESHOLD)
            {
                // Large drift - rollback
                Debug.LogWarning($"[ClientStatePrediction] Rollback: predicted={_predictedElapsed:F2}, server={serverElapsed:F2}, drift={drift:F3}");
                _predictedElapsed = serverElapsed;
            }
            // Small drift - will be smoothed via interpolation
        }

        /// <summary>
        /// Called every frame to advance prediction.
        /// </summary>
        public void Update(float deltaTime)
        {
            if (!_isPredicting) return;

            // Advance prediction
            _predictedElapsed += deltaTime;

            // Smooth interpolation towards predicted value
            _interpolatedElapsed = Mathf.Lerp(
                _interpolatedElapsed,
                _predictedElapsed,
                deltaTime * INTERPOLATION_SPEED
            );
        }

        /// <summary>
        /// Gets remaining countdown time (for UI).
        /// </summary>
        public float GetCountdownRemaining(float totalDuration)
        {
            return Mathf.Max(0f, totalDuration - _interpolatedElapsed);
        }

        /// <summary>
        /// Stops prediction (e.g., on disconnect).
        /// </summary>
        public void Stop()
        {
            _isPredicting = false;
        }

        /// <summary>
        /// Resets prediction state.
        /// </summary>
        public void Reset()
        {
            _predictedElapsed = 0f;
            _serverElapsed = 0f;
            _interpolatedElapsed = 0f;
            _currentStateID = 0;
            _isPredicting = false;
        }
    }
}
