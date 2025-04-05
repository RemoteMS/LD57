using System.Collections.Generic;
using UnityEngine;

namespace Services.Gameplay.BulletSystem
{
    public class Bullet : MonoBehaviour, IEffectDealer
    {
        public IEnumerable<Effect> Effects { get; private set; }

        public Vector3 direction;
        private float _maxDistance;
        private Vector3 _startPosition;

        public void Initialize(Vector3 position, Vector3 bulletDirection, float maxDistance,
            IEnumerable<Effect> effects)
        {
            Effects = effects;

            transform.position = position;
            _startPosition = position;
            direction = bulletDirection;
            _maxDistance = maxDistance;

            transform.rotation = Quaternion.LookRotation(bulletDirection);
        }

        public bool HasTraveledMaxDistance()
        {
            var distanceTraveledSq = (_startPosition - transform.position).sqrMagnitude;
            return distanceTraveledSq >= _maxDistance * _maxDistance;
        }
    }
}