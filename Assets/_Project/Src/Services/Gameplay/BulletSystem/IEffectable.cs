using Services.Gameplay.Enemies;
using UnityEngine;

namespace Services.Gameplay.BulletSystem
{
    public interface IEffectable
    {
        public int DamageToPlayer { get; }
        public int Price { get; }
        public Health Health { get; }

        public GameObject GameObject { get; }
    }
}