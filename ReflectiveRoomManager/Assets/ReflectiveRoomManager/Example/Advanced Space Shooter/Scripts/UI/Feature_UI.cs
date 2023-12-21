using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Examples.SpaceShooter.Features
{
    using Utilities;
    
    public class Feature_UI : MonoBehaviour
    {
        [SerializeField] private Image _barImage;
        [SerializeField] private Image _iconImage;

        private float _duration;

        private float _timer;

        public void UpdateDuration() => _timer = 0;
        
        public void Init(Sprite icon, float duration)
        {
            _duration = duration;

            _iconImage.sprite = icon;

            StartCoroutine(StartCountDown());
        }

        private IEnumerator StartCountDown()
        {
            while (_timer < _duration)
            {
                _barImage.fillAmount = 1 - _timer / _duration;

                _timer += Time.fixedDeltaTime;

                yield return new WaitForFixedUpdate();
            }

            gameObject.Destroy();
        }
    }
}