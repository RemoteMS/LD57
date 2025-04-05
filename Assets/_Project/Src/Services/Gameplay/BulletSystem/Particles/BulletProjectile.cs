using UnityEngine;

namespace Services.Gameplay.BulletSystem.Particles
{
    public class BulletProjectile
    {
        public Effect[] Effects = { new DamageEffect() };
        public string bulletName = "Bullet";

        public GameObject BulletPrefab;
        public GameObject ImpactEffectPrefab;

        public Sprite hudSprite;
    }
}