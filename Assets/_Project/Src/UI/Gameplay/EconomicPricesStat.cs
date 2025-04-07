using System;
using Reflex.Attributes;
using Services.Gameplay.Economic;
using TMPro;
using UniRx;
using UnityEngine;
using PrimeTween;
using Cysharp.Threading.Tasks;

namespace UI.Gameplay
{
    public class EconomicPricesStat : MonoBehaviour, IDisposable
    {
        [SerializeField] private TMP_Text _coinsCountText;
        [SerializeField] private TMP_Text _playerHealthText;

        private readonly CompositeDisposable _disposables = new();
        private Tween _currentTween;

        [Inject]
        public void Inject(EconomicSystem economicSystem)
        {
            economicSystem.coinsCount
                .Subscribe(x => { _coinsCountText.text = x.ToString(); })
                .AddTo(_disposables);

            economicSystem.globalHealthCount
                .Subscribe(ChangeHealthText)
                .AddTo(_disposables);
        }

        private void ChangeHealthText(int x)
        {
            _playerHealthText.text = x.ToString();

            if (_currentTween.isAlive)
            {
                _currentTween.Stop();
            }

            FlashTextAsync().Forget();
        }

        private async UniTaskVoid FlashTextAsync()
        {
            const float duration = 0.2f;

            _currentTween = Tween.Color(_playerHealthText, Color.white, Color.red, duration, Ease.InOutQuad);
            await _currentTween.ToUniTask();

            _currentTween = Tween.Color(_playerHealthText, Color.red, Color.white, duration, Ease.InOutQuad);
            await _currentTween.ToUniTask();

            _currentTween = Tween.Color(_playerHealthText, Color.white, Color.red, duration, Ease.InOutQuad);
            await _currentTween.ToUniTask();

            _currentTween = Tween.Color(_playerHealthText, Color.red, Color.white, duration, Ease.InOutQuad);
            await _currentTween.ToUniTask();
        }

        public void Dispose()
        {
            _disposables?.Dispose();
            if (_currentTween.isAlive)
            {
                _currentTween.Stop();
            }
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}