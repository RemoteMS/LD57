using UnityEngine;

namespace Services.Gameplay.BulletSystem.Particles
{
    public class ProjectileSettings
    {
        public int bulletCount = 100;
        public float bulletSpeed = 10f;
        public float BulletMaxDistance = 70f;

        public GameObject ImpactEffectPrefab;
        public LayerMask CollisionMask;

        public ProjectileSettings()
        {
            Debug.Log("Projectile Settings Inited");
            
            ImpactEffectPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            CollisionMask = LayerMask.GetMask("Obstacle", "Unit");
        }
    }
}