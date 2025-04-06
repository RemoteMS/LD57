using System;
using UniRx;

namespace Services.Storages.Gameplay
{
    public class GunData : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();

        public IReadOnlyReactiveProperty<float> projectileSpeed => _projectileSpeed;
        private readonly ReactiveProperty<float> _projectileSpeed;

        public IReadOnlyReactiveProperty<float> fireRate => _fireRate;
        private readonly ReactiveProperty<float> _fireRate;

        public IReadOnlyReactiveProperty<float> reloadTime => _reloadTime;
        private readonly ReactiveProperty<float> _reloadTime;

        public IReadOnlyReactiveProperty<int> maxAmmo => _maxAmmo;
        private readonly ReactiveProperty<int> _maxAmmo;

        public IReadOnlyReactiveProperty<int> currentAmmo => _currentAmmo;
        private readonly ReactiveProperty<int> _currentAmmo;

        public IReadOnlyReactiveProperty<bool> isReloading => _isReloading;
        private readonly ReactiveProperty<bool> _isReloading;

        public IReadOnlyReactiveProperty<float> nextFireTime => _nextFireTime;
        private readonly ReactiveProperty<float> _nextFireTime;

        public GunData(GunDataSettings settings)
        {
            _projectileSpeed = new ReactiveProperty<float>(settings.projectileSpeed).AddTo(_disposables);
            _fireRate = new ReactiveProperty<float>(settings.fireRate).AddTo(_disposables);
            _reloadTime = new ReactiveProperty<float>(settings.reloadTime).AddTo(_disposables);
            _maxAmmo = new ReactiveProperty<int>(settings.maxAmmo).AddTo(_disposables);
            _currentAmmo = new ReactiveProperty<int>(settings.currentAmmo).AddTo(_disposables);
            _isReloading = new ReactiveProperty<bool>(settings.isReloading).AddTo(_disposables);
            _nextFireTime = new ReactiveProperty<float>(settings.nextFireTime).AddTo(_disposables);
        }

        public void UpdateProjectileSpeed(float speed)
        {
            _projectileSpeed.Value = speed;
        }

        public void UpdateFireRate(float fireRate)
        {
            _fireRate.Value = fireRate;
        }

        public void UpdateReloadTime(float reloadTime)
        {
            _reloadTime.Value = reloadTime;
        }

        public void UpdateMaxAmmo(int maxAmmo)
        {
            _maxAmmo.Value = maxAmmo;
        }

        public void UpdateCurrentAmmo(int currentAmmo)
        {
            _currentAmmo.Value = currentAmmo;
        }

        public void UpdateReloading(bool isReloading)
        {
            _isReloading.Value = isReloading;
        }

        public void UpdateNextFireTime(float nextFireTime)
        {
            _nextFireTime.Value = nextFireTime;
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}