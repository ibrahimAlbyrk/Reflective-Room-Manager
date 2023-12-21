using TMPro;
using UnityEngine;

namespace Examples.SpaceShooter.UI.Player
{
    using Spaceship;
    
    public class PlayerUI : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private Health _health;
        [SerializeField] private TMP_Text _healthText;

        private void Awake()
        {
            _health.OnHealthChanged += HealthOnOnHealthChanged;
        }

        private void HealthOnOnHealthChanged(float health, float maxHealth)
        {
            _healthText.text = $"{health}";
        }
    }
}