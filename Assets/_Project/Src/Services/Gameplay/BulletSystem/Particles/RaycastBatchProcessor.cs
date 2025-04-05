using System;
using Helpers;
using Unity.Collections;
using UnityEngine;

namespace Services.Gameplay.BulletSystem.Particles
{
    public class RaycastBatchProcessor : IDisposable
    {
        private const int MaxRaycastsPerJob = 10000;

        private NativeArray<RaycastCommand> _rayCommands;
        private NativeArray<SpherecastCommand> _sphereCommands;
        private NativeArray<RaycastHit> _hitResults;

        public void PerformRaycasts(
            Vector3[] origins,
            Vector3[] directions,
            int layerMask,
            bool hitBackfaces,
            bool hitTriggers,
            bool hitMultiFace,
            Action<RaycastHit[]> callback
        )
        {
            const float maxDistance = 1f;
            var rayCount = Mathf.Min(origins.Length, MaxRaycastsPerJob);

            var queryTriggerInteraction =
                hitTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;

            using (_rayCommands = new NativeArray<RaycastCommand>(rayCount, Allocator.TempJob))
            {
                var parameters = new QueryParameters
                {
                    layerMask = layerMask,
                    hitBackfaces = hitBackfaces,
                    hitTriggers = queryTriggerInteraction,
                    hitMultipleFaces = hitMultiFace
                };

                for (var i = 0; i < rayCount; i++)
                {
                    _rayCommands[i] = new RaycastCommand(origins[i], directions[i], parameters, maxDistance);
                }

                ExecuteRaycasts(_rayCommands, callback);
            }
        }

        private void ExecuteRaycasts(NativeArray<RaycastCommand> raycastCommands, Action<RaycastHit[]> callback)
        {
            const int maxHitsPerRaycast = 1;
            var totalHitsNeeded = raycastCommands.Length * maxHitsPerRaycast;

            using (_hitResults = new NativeArray<RaycastHit>(totalHitsNeeded, Allocator.TempJob))
            {
                foreach (RaycastCommand t in raycastCommands)
                {
                    Debug.DrawLine(t.from, t.from + t.direction * 1f, Color.red, 0.5f);
                }

                var raycastJobHandle = RaycastCommand.ScheduleBatch(raycastCommands, _hitResults, maxHitsPerRaycast);
                raycastJobHandle.Complete();

                if (_hitResults.Length > 0)
                {
                    var results = _hitResults.ToArray();

                    for (var i = 0; i < results.Length; i++)
                    {
                        if (results[i].collider)
                        {
                            var interfaceInParent =
                                GameObjects.FindInterfaceInParent<IEffectable>(results[i].collider.transform);

                            if (interfaceInParent != null)
                            {
                                Debug.Log($"hits[i].collider - Found interface in parent - {interfaceInParent}");
                            }
                        }
                    }

                    callback?.Invoke(results);
                }
            }
        }

        public void Dispose()
        {
        }
    }
}