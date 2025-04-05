using System;
using Reflex.Attributes;
using Reflex.Extensions;
using Services.Gameplay.BulletSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            if (Input.GetMouseButtonDown(0))
            {
                if (Time.time >= _nextFireTime)
                {
                    // FireProjectile();
                    _nextFireTime = Time.time + 1f / fireRate;

                    _manager.SpawnBulletPattern(
                        launchPoint.position,
                        targetPoint.position
                    );
                }
            }
        }

        private void FireProjectile()
        {
            var projectile = Instantiate(projectilePrefab, launchPoint.position, Quaternion.identity);

            var direction = (targetPoint.position - launchPoint.position).normalized;

            var colliderHolder = projectile.GetComponent<ColliderHolder>();
            var movementScript = projectile.AddComponent<ProjectileMovement>();

            movementScript.Initialize(direction, projectileSpeed);
        }
    }


}