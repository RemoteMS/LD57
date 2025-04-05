using System;
using System.Collections.Generic;
using Helpers;
using Services.Gameplay.BulletSystem.Particles;
using UniRx;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Services.Gameplay.BulletSystem
{
    public class BulletManager : IDisposable
    {
        private readonly ObjectPool<Bullet> _bulletPool;

        private readonly List<Bullet> _activeProjectiles = new();
        private readonly List<Bullet> _bulletsToReturn = new();

        private TransformAccessArray _bulletTransforms;

        private readonly ProjectileSettings _projectileSettings;
        private readonly InGameEffectSystem _inGameEffectSystem;
        private readonly RaycastBatchProcessor _raycastProcessor;

        private readonly CompositeDisposable _disposables = new();

        public BulletManager(ProjectileSettings projectileSettings, InGameEffectSystem inGameEffectSystem,
            RaycastBatchProcessor raycastProcessor)
        {
            _projectileSettings = projectileSettings;
            _inGameEffectSystem = inGameEffectSystem;
            _raycastProcessor = raycastProcessor;

            var bulleta = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            _bulletPool = new ObjectPool<Bullet>(
                createFunc: () =>
                {
                    var bulletObj = Object.Instantiate(bulleta);
                    bulletObj.SetActive(false);
                    return bulletObj.GetOrAdd<Bullet>();
                },
                actionOnGet: bullet => bullet.gameObject.SetActive(true),
                actionOnRelease: bullet => bullet.gameObject.SetActive(false),
                actionOnDestroy: DestroyBullet,
                collectionCheck: false,
                defaultCapacity: _projectileSettings.bulletCount,
                maxSize: _projectileSettings.bulletCount * 10
            );

            Observable.EveryUpdate().Subscribe(x => OnUpdateTrigger()).AddTo(_disposables);
            Observable.EveryLateUpdate().Subscribe(x => OnLateUpdateTrigger()).AddTo(_disposables);

            _disposables.Add(_bulletPool);
        }

        private BulletHellProjectile GeneratePattern(
            Vector3 origin,
            Vector3 target,
            float speed)
        {
            var dir = target - origin;

            return new BulletHellProjectile(origin, dir, speed);
        }

        public void SpawnBulletPattern(
            Vector3 position,
            Vector3 direction
        )
        {
            var newBullets = GeneratePattern(position, direction, 120f);

            var bullet = _bulletPool.Get();
            bullet.Initialize(
                newBullets.Position,
                newBullets.Direction,
                _projectileSettings.BulletMaxDistance,
                // weapon.bulletType.Effects
                null
            );
            _activeProjectiles.Add(bullet);
        }

        private static void DestroyBullet(Bullet bullet)
        {
            if (bullet)
            {
                Object.Destroy(bullet.gameObject);
            }
        }

        private void OnUpdateTrigger()
        {
            const int subSteps = 5;
            var subStepTime = Time.deltaTime / subSteps;

            // Consider caching the TransformAccessArray if possible
            using (_bulletTransforms = new TransformAccessArray(_activeProjectiles.Count))
            {
                for (var i = _activeProjectiles.Count; i-- > 0;)
                {
                    var bullet = _activeProjectiles[i];
                    if (bullet.HasTraveledMaxDistance())
                    {
                        ReturnBullet(bullet);
                        continue;
                    }

                    _bulletTransforms.Add(bullet.transform);
                }

                for (var step = 0; step < subSteps; step++)
                {
                    var job = new BulletMoveJob
                    {
                        DeltaTime = subStepTime,
                        Speed = _projectileSettings.bulletSpeed
                    };

                    var jobHandle = job.Schedule(_bulletTransforms);
                    jobHandle.Complete();

                    HandleCollisions();
                }
            }
        }

        private void HandleCollisions()
        {
            var origins = new Vector3[_activeProjectiles.Count];
            var directions = new Vector3[_activeProjectiles.Count];

            for (var i = 0; i < _activeProjectiles.Count; i++)
            {
                var bullet = _activeProjectiles[i];
                origins[i] = bullet.transform.position;
                directions[i] = bullet.direction;
            }

            _raycastProcessor.PerformRaycasts(origins, directions,
                _projectileSettings.CollisionMask.value, false,
                false, false,
                OnRaycastResults);
        }

        private void OnRaycastResults(RaycastHit[] hits)
        {
            for (var i = hits.Length; i-- > 0;)
            {
                if (hits[i].collider)
                {
                    Debug.Log(
                        $"hits[i].collider - {hits[i].collider}, _activeProjectiles[i] - {_activeProjectiles[i]}");

                    var interfaceInParent = GameObjects.FindInterfaceInParent<IEffectable>(hits[i].collider.transform);

                    if (interfaceInParent != null)
                    {
                        // sent Effect
                        _inGameEffectSystem.TakeEffect(_activeProjectiles[i], interfaceInParent);
                    }

                    ReturnBullet(_activeProjectiles[i]);

                    _inGameEffectSystem.TakeImpact(
                        impactPosition: hits[i].point,
                        impactEffectPrefab: _projectileSettings.ImpactEffectPrefab,
                        impactIdentity: Quaternion.identity,
                        up: hits[i].normal,
                        timeToDestroy: 2f
                    );
                }
            }
        }

        private void ReturnBullet(Bullet bullet)
        {
            _bulletsToReturn.Add(bullet);
            _activeProjectiles.Remove(bullet);
        }

        [BurstCompile]
        private struct BulletMoveJob : IJobParallelForTransform
        {
            public float DeltaTime;
            public float Speed;

            public void Execute(int index, TransformAccess transform)
            {
                var forward = transform.rotation * Vector3.forward;
                transform.position += forward * Speed * DeltaTime;
            }
        }

        private void OnLateUpdateTrigger()
        {
            foreach (var bullet in _bulletsToReturn)
            {
                _bulletPool.Release(bullet);
            }

            _bulletsToReturn.Clear();
        }

        public void Dispose()
        {
            foreach (var bullet in _activeProjectiles)
            {
                if (bullet)
                {
                    _bulletPool.Release(bullet);
                }
            }

            _activeProjectiles.Clear();
            _bulletsToReturn.Clear();

            _disposables?.Dispose();
        }
    }
}