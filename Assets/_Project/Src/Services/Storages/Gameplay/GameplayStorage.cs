using System;
using Services.Gameplay.BulletSystem;
using UniRx;
using Object = UnityEngine.Object;

namespace Services.Storages.Gameplay
{
    public class GameplayStorage : IDisposable
    {
        public IReadOnlyReactiveProperty<int> enemiesCount => _enemiesCount;
        private readonly ReactiveProperty<int> _enemiesCount;

        public IReadOnlyReactiveCollection<IEffectable> enemies => _enemies;
        private readonly ReactiveCollection<IEffectable> _enemies;

        
        public readonly GunData gunData;

        private readonly CompositeDisposable _disposables = new();

        public GameplayStorage()
        {
            gunData = new GunData(new GunDataSettings()).AddTo(_disposables);

            _enemiesCount = new ReactiveProperty<int>(0).AddTo(_disposables);

            _enemies = new ReactiveCollection<IEffectable>().AddTo(_disposables);

            _enemies.ObserveCountChanged()
                .Subscribe(x => { _enemiesCount.Value = x; })
                .AddTo(_disposables);
        }

        public void AddEnemy(IEffectable target)
        {
            _enemies.Add(target);
        }

        public void KillEnemy(IEffectable target)
        {
            _enemies.Remove(target);
            Object.Destroy(target.GameObject);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}