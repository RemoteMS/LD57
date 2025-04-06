using System;
using System.Collections.Generic;
using Services.Storages.Gameplay;
using UniRx;

namespace Services.Gameplay.Upgrading
{
    public class UpgradeSystem : IDisposable
    {
        public readonly GunData _storageGunData;
        private readonly GameplayStorage _storage;
        private readonly CompositeDisposable _disposables = new();

        public UpgradeValue<float> fireRate => _fireRate;
        private readonly UpgradeValue<float> _fireRate;

        public UpgradeValue<float> reloadTime => _reloadTime;
        private readonly UpgradeValue<float> _reloadTime;

        public UpgradeSystem(GameplayStorage storage)
        {
            _storage = storage;
            _storageGunData = storage.gunData;

            _fireRate = new UpgradeValue<float>(
                new List<LevelData<float>>
                {
                    new(level: 1, value: 2.1f, cost: 5),
                    new(level: 2, value: 2f, cost: 10),
                    new(level: 3, value: 3f, cost: 15),
                    new(level: 4, value: 4f, cost: 20),
                    new(level: 5, value: 5f, cost: 25),
                    new(level: 6, value: 6f, cost: 30),
                    new(level: 7, value: 7f, cost: 35),
                }
            ).AddTo(_disposables);

            _reloadTime = new UpgradeValue<float>(
                new List<LevelData<float>>()
                {
                    new(level: 1, value: 2f, cost: 10),
                    new(level: 2, value: 1.9f, cost: 20),
                    new(level: 3, value: 1.8f, cost: 30),
                    new(level: 4, value: 1.7f, cost: 40),
                    new(level: 5, value: 1.6f, cost: 50),
                    new(level: 6, value: 1.5f, cost: 60),
                    new(level: 7, value: 1.4f, cost: 70),
                    new(level: 8, value: 1.3f, cost: 80),
                }
            ).AddTo(_disposables);
        }

        public void UpdateFireRate()
        {
            _fireRate.NextValue();
            _storageGunData.UpdateFireRate(_fireRate.currentValue.Value);
        }

        public void UpdateReloadTime()
        {
            _reloadTime.NextValue();
            _storageGunData.UpdateReloadTime(_reloadTime.currentValue.Value);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}