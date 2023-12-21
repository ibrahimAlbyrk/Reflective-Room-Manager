using TMPro;
using System;
using UnityEngine;
using DG.Tweening;
using System.Collections;

namespace Examples.SpaceShooter.Game.Mod
{
    public class ShrinkingArea_UI : MonoBehaviour
    {
        [SerializeField] private GameObject _countDownContent;
        [SerializeField] private TMP_Text _titleText;

        public void StartShrinkingHandler(float time)
        {
            StartCoroutine(CountDown(time));
        }

        private IEnumerator CountDown(float time)
        {
            var timer = 0f;
            
            _countDownContent.SetActive(true);
            _countDownContent.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBack);
            _countDownContent.transform.DORotate(new Vector3(0, 0, -360), 1f,RotateMode.FastBeyond360).SetEase(Ease.OutQuad);

            while (timer < time)
            {
                _titleText.text = $"{(int)Math.Floor(time - timer)}";

                timer += Time.fixedDeltaTime;
                
                yield return new WaitForFixedUpdate();
            }
            
            _countDownContent.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InBack);
            _countDownContent.transform.DORotate(new Vector3(0, 0, 360), 1f,RotateMode.FastBeyond360).SetEase(Ease.InQuad).onComplete +=
                () =>
                {
                    _countDownContent.SetActive(false);
                };
        }
    }
}