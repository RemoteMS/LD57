using Reflex.Attributes;
using Services.Gameplay.BulletSystem;
using UnityEngine;
using PrimeTween;
using Cysharp.Threading.Tasks;
using Services.Global.Audio;

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
        [SerializeField] private float fireRate = 1f;
        [SerializeField] private float animationDuration = 0.2f;

        private float _nextFireTime = 0f;
        private BulletManager _manager;
        private IAudioService _audioService;
        private Tween _currentTween;

        [Inject]
        public void Inject(BulletManager manager, IAudioService audioService)
        {
            _manager = manager;
            _audioService = audioService;
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

        private async void Update()
        {
            if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space))
            {
                if (Time.time >= _nextFireTime)
                {
                    _nextFireTime = Time.time + 1f / fireRate;
                    await ShootAnimation();

                    _manager.SpawnBulletPattern(
                        launchPoint.position,
                        targetPoint.position
                    );
                    _audioService.PlayShot();
                }
            }
        }
    }
}