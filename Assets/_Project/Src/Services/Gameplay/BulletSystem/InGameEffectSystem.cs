using System;
using Services.Gameplay.Economic;
using Services.Global.ResourceManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Services.Gameplay.BulletSystem
{
    public class InGameEffectSystem : IDisposable
    {
        private readonly EconomicSystem _economicSystem;
        private readonly IDataManager _resourceManager;

        public InGameEffectSystem(EconomicSystem economicSystem, IDataManager resourceManager)
        {
            _economicSystem = economicSystem;
            _resourceManager = resourceManager;
        }

        public void TakeEffect(IEffectDealer effectDealer, IEffectable target)
        {
            Debug.LogWarning($"impactEffectPrefab - {nameof(TakeEffect)}");

            var effects = effectDealer.Effects;

            foreach (var effect in effects)
            {
                effect.Apply(target);

                if (!target.Health.IsAlive)
                {
                    _economicSystem.GetPriceForEnemy(target);
                    Debug.LogWarning($"should be dead - {target} {target.GameObject.name}");

                    // kill enemy                    
                    Object.Destroy(target.GameObject);
                }
            }

            // calculate death
        }

        public void TakeImpact(Vector3 impactPosition,
            Quaternion impactIdentity,
            string impactEffectPrefabAdress,
            Vector3 up,
            float timeToDestroy)
        {
            Debug.LogWarning($"impactEffectPrefab - {impactPosition}");
            var impactEffectPrefab = _resourceManager.GetObjectCopyFast(impactEffectPrefabAdress);
            var impactEffect = Object.Instantiate(
                impactEffectPrefab,
                impactPosition,
                impactIdentity
            );
            impactEffect.transform.up = up;

            Object.Destroy(impactEffect, timeToDestroy);
        }

        public void EnemyExitLevel(IEffectable effectable)
        {
            _economicSystem.DamageToPlayer(effectable);
            Object.Destroy(effectable.GameObject);
        }

        public void Dispose()
        {
        }
    }
}