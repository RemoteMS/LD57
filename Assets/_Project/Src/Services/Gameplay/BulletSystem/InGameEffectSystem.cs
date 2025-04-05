using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Services.Gameplay.BulletSystem
{
    public class InGameEffectSystem : IDisposable
    {
        public void TakeEffect(IEffectDealer effectDealer, IEffectable target)
        {
            var effects = effectDealer.Effects;

            foreach (var effect in effects)
            {
                effect.Apply(target);
            }

            // calculate death
        }

        public void TakeImpact(Vector3 impactPosition,
            Quaternion impactIdentity,
            GameObject impactEffectPrefab,
            Vector3 up,
            float timeToDestroy)
        {
            Debug.LogWarning($"impactEffectPrefab - {impactPosition}");
            var impactEffect = Object.Instantiate(
                impactEffectPrefab,
                impactPosition,
                impactIdentity
            );
            impactEffect.transform.up = up;

            Object.Destroy(impactEffect, timeToDestroy);
        }

        public void Dispose()
        {
        }
    }
}