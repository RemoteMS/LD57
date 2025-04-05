using Reflex.Attributes;
using Services.Gameplay.BulletSystem;
using UnityEngine;
using PrimeTween;
using Cysharp.Threading.Tasks;

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
        private Tween _currentTween; // Для отслеживания и отмены текущей анимации

        [Inject]
        public void Inject(BulletManager manager)
        {
            _manager = manager;
        }

        private async UniTask ShootAnimation()
        {
            // Останавливаем предыдущую анимацию, если она есть
            if (_currentTween.isAlive)
            {
                _currentTween.Stop();
            }

            // Получаем текущую локальную позицию как начальную точку
            Vector3 startPosition = gunRenderer.transform.localPosition;

            // Сначала движемся к shoot target position
            _currentTween = Tween.LocalPosition(
                gunRenderer.transform,

                // startPosition: startPosition,
                endValue: gunRendererShootTargetPosition.localPosition,
                duration: animationDuration / 2f,
                ease: Ease.InOutQuad
            );
            await _currentTween.ToUniTask();

            // Затем возвращаемся к default position
            _currentTween = Tween.LocalPosition(
                gunRenderer.transform,
                // startPosition: gunRendererShootTargetPosition.localPosition,
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
                    await ShootAnimation(); // Ждем завершения анимации

                    _manager.SpawnBulletPattern(
                        launchPoint.position,
                        targetPoint.position
                    );
                }
            }
        }
    }
}