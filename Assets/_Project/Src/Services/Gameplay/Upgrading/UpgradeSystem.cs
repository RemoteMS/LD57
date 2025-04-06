using System;
using System.Collections.Generic;
using Services.Gameplay.Economic;
using Services.Storages.Gameplay;
using UniRx;

namespace Services.Gameplay.Upgrading
{
    public class UpgradeSystem : IDisposable
    {
        public readonly GunData _storageGunData;
        private readonly GameplayStorage _storage;
        private readonly EconomicSystem _economicSystem;
        private readonly CompositeDisposable _disposables = new();

        public UpgradeValue<float> fireRate => _fireRate;
        private readonly UpgradeValue<float> _fireRate;

        public UpgradeValue<float> reloadTime => _reloadTime;
        private readonly UpgradeValue<float> _reloadTime;

        public UpgradeValue<int> maxAmmo => _maxAmmo;
        private readonly UpgradeValue<int> _maxAmmo;

        public UpgradeSystem(GameplayStorage storage, EconomicSystem economicSystem)
        {
            _storage = storage;
            _economicSystem = economicSystem;
            _storageGunData = storage.gunData;

            _fireRate = new UpgradeValue<float>(
                new List<LevelData<float>>
                {
                    new(level: 1, value: 2f, cost: 5),
                    new(level: 2, value: 3f, cost: 10),
                    new(level: 3, value: 4f, cost: 15),
                    new(level: 4, value: 5f, cost: 20),
                    new(level: 5, value: 6f, cost: 25),
                    new(level: 6, value: 7f, cost: 30),
                    new(level: 7, value: 8f, cost: 35),
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

            _maxAmmo = new UpgradeValue<int>(
                    new List<LevelData<int>>()
                    {
                        new(level: 1, value: 5, cost: 10),
                        new(level: 2, value: 6, cost: 20),
                        new(level: 3, value: 7, cost: 30),
                        new(level: 4, value: 8, cost: 40),
                        new(level: 5, value: 9, cost: 50),
                        new(level: 6, value: 10, cost: 60),
                        new(level: 7, value: 11, cost: 70),
                        new(level: 8, value: 12, cost: 80),
                        new(level: 9, value: 13, cost: 90),
                        new(level: 10, value: 14, cost: 100),
                    }
                )
                .AddTo(_disposables);
        }

        public void UpdateFireRate()
        {
            _fireRate.NextValue();
            _storageGunData.UpdateFireRate(_fireRate.currentValue.Value);
            _economicSystem.SpendCoins(_fireRate.currentCost.Value);
        }

        public void UpdateReloadTime()
        {
            _reloadTime.NextValue();
            _storageGunData.UpdateReloadTime(_reloadTime.currentValue.Value);
            _economicSystem.SpendCoins(_reloadTime.currentCost.Value);
        }
        
        public void UpdateMaxAmmo()
        {
            _maxAmmo.NextValue();
            _storageGunData.UpdateMaxAmmo(_maxAmmo.currentValue.Value);
            _economicSystem.SpendCoins(_maxAmmo.currentCost.Value);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}