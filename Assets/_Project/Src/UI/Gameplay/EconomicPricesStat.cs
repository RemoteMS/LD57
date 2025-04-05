using System;
using Reflex.Attributes;
using Services.Gameplay.Economic;
using TMPro;
using UniRx;
using UnityEngine;

namespace UI.Gameplay
{
    public class EconomicPricesStat : MonoBehaviour, IDisposable
    {
        [SerializeField] private TMP_Text _coinsCountText;
        [SerializeField] private TMP_Text _playerHealthText;

        private readonly CompositeDisposable _disposables = new();

        [Inject]
        public void Inject(EconomicSystem economicSystem)
        {
            economicSystem.coinsCount
                .Subscribe(x => { _coinsCountText.text = x.ToString(); })
                .AddTo(_disposables);

            economicSystem.globalHealthCount
                .Subscribe(
                    x => { _playerHealthText.text = x.ToString(); })
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}