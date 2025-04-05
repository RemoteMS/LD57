using System;
using Reflex.Extensions;
using Services.Gameplay.BulletSystem;
using UnityEngine;

namespace Services.Gameplay.Enemies
{
    public class Enemy : MonoBehaviour, IEffectable
    {
        [SerializeField] private float speed = 0.1f;
        [SerializeField] private float destroyYPosition = 10f;

        [field: SerializeField] public int DamageToPlayer { get; set; } = 1;
        [field: SerializeField] public int Price { get; set; } = 1;
        [field: SerializeField] public Health Health { get; set; } = new Health(10, 10);
        public GameObject GameObject => gameObject;

        private InGameEffectSystem _system;

        private void Start()
        {
            _system = gameObject.scene.GetSceneContainer().Resolve<InGameEffectSystem>();
        }

        private void Update()
        {
            transform.Translate(Vector3.up * (speed * Time.deltaTime));

            if (transform.position.y > destroyYPosition)
            {
                _system.EnemyExitLevel(this);
            }
        }
    }
}