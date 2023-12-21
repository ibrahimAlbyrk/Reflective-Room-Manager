using UnityEngine;

namespace Examples.SpaceShooter.UI.Fields
{
    public class Bar_UI : MonoBehaviour
    {
        [SerializeField] private Transform _imageTransform;

        private float _maxValue;
        private float _currentValue;
        
        public void SetValue(float value, float maxValue = 1)
        {
            _currentValue = value;
            _maxValue = maxValue;
            
            UpdateUI();
        }
        
        private void UpdateUI()
        {
            var scale = _imageTransform.localScale;
            scale.x = _currentValue / _maxValue;

            _imageTransform.localScale = scale;
        }
    }
}