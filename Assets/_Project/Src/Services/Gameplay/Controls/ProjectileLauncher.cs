using Reflex.Attributes;
using Services.Gameplay.BulletSystem;
using UnityEngine;
using PrimeTween;
using Cysharp.Threading.Tasks;
using Services.Global.Audio;
using Services.Storages.Gameplay;

namespace Services.Gameplay.Controls
{
    public class ProjectileLauncher : MonoBehaviour
    {
        [SerializeField] public GameObject gunRenderer;
        [SerializeField] public Transform gunRendererDefaultPosition;
        [SerializeField] public Transform gunRendererShootTargetPosition;

        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform targetPoint;
        [SerializeField] private Transform launchPoint;

        [SerializeField] private float projectileSpeed = 10f;

        [SerializeField] private float animationDuration = 0.2f;


        private BulletManager _manager;
        private IAudioService _audioService;
        private GameplayStorage _gameplayStorage;

        private GunData _gunData;

        private Tween _currentTween;

        [Inject]
        public void Inject(BulletManager manager, IAudioService audioService, GameplayStorage gameplayStorage)
        {
            _manager = manager;
            _audioService = audioService;
            _gameplayStorage = gameplayStorage;

            _gunData = gameplayStorage.gunData;

            _gunData.UpdateCurrentAmmo(_gunData.maxAmmo.Value);
        }

        private async UniTask ShootAnimation()
        {
            if (_currentTween.isAlive)
            {
                _currentTween.Stop();
            }

            var startPosition = gunRenderer.transform.localPosition;

            _currentTween = Tween.LocalPosition(
                gunRenderer.transform,
                endValue: gunRendererShootTargetPosition.localPosition,
                duration: animationDuration / 2f,
                ease: Ease.InOutQuad
            );
            await _currentTween.ToUniTask();

            _currentTween = Tween.LocalPosition(
                gunRenderer.transform,
                endValue: gunRendererDefaultPosition.localPosition,
                duration: animationDuration / 2f,
                ease: Ease.InOutQuad
            );
            await _currentTween.ToUniTask();
        }

        private async UniTask Reload()
        {
            _gunData.UpdateReloading(true);
            await UniTask.Delay(System.TimeSpan.FromSeconds(_gunData.reloadTime.Value));
            _gunData.UpdateCurrentAmmo(_gunData.maxAmmo.Value);
            _gunData.UpdateReloading(false);
            _audioService.PlayReload(); // Предполагается, что в AudioService есть звук перезарядки
        }

        private async void Update()
        {
            if (_gunData.isReloading.Value) return;

            if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space))
            {
                if (_gunData.currentAmmo.Value > 0 && Time.time >= _gunData.nextFireTime.Value)
                {
                    _gunData.UpdateNextFireTime(Time.time                 + 1f / _gunData.fireRate.Value);
                    _gunData.UpdateCurrentAmmo(_gunData.currentAmmo.Value - 1);

                    await ShootAnimation();

                    _manager.SpawnBulletPattern(
                        launchPoint.position,
                        targetPoint.position
                    );
                    _audioService.PlayShot();
                }
                else if (_gunData.currentAmmo.Value <= 0)
                {
                    await Reload();
                }
            }

            // Ручная перезарядка по нажатию клавиши R
            if (Input.GetKeyDown(KeyCode.R) && _gunData.currentAmmo.Value < _gunData.maxAmmo.Value)
            {
                await Reload();
            }
        }


    }
}