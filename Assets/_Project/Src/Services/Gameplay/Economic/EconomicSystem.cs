using System;
using Services.Gameplay.BulletSystem;
using UniRx;
using UnityEngine;

namespace Services.Gameplay.Economic
{
    public class EconomicSystem : IDisposable
    {
        public IReadOnlyReactiveProperty<int> coinsCount => _coinsCount;
        private readonly ReactiveProperty<int> _coinsCount;

        public IReadOnlyReactiveProperty<int> globalHealthCount => _globalHealthCount;
        private readonly ReactiveProperty<int> _globalHealthCount;

        private readonly CompositeDisposable _disposables = new();

        public EconomicSystem()
        {
            _coinsCount = new ReactiveProperty<int>(0).AddTo(_disposables);
            _globalHealthCount = new ReactiveProperty<int>(16).AddTo(_disposables);
        }

        public void GetPriceForEnemy(IEffectable effectable)
        {
            _coinsCount.Value += effectable.Price;
            Debug.LogWarning($"new coints count is {_coinsCount.Value}");
        }

        public void DamageToPlayer(IEffectable effectable)
        {
            _globalHealthCount.Value -= effectable.DamageToPlayer;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}