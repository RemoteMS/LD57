using Reflex.Attributes;
using Services.Gameplay.BulletSystem;
using UnityEngine;

namespace Services.Gameplay.Controls
{
    public class ProjectileLauncher : MonoBehaviour
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform targetPoint;
        [SerializeField] private Transform launchPoint;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private float fireRate = 1f;

        private float _nextFireTime = 0f;
        private BulletManager _manager;

        [Inject]
        public void Inject(BulletManager manager)
        {
            _manager = manager;
        }

        private void Update()
        {
            if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space))
            {
                if (Time.time >= _nextFireTime)
                {
                    _nextFireTime = Time.time + 1f / fireRate;

                    _manager.SpawnBulletPattern(
                        launchPoint.position,
                        targetPoint.position
                    );
                }
            }
        }
    }


}