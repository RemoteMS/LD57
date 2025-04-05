using Services.Global.ResourceManagement;
using UnityEngine;

namespace Services.Gameplay.BulletSystem.Particles
{
    public class ProjectileSettings
    {
        public int bulletCount = 100;
        public float bulletSpeed = 10f;
        public float BulletMaxDistance = 70f;

        public GameObject Bullet;
        public string ImpactEffectPrefab;
        public LayerMask CollisionMask;

        public ProjectileSettings(IDataManager resourceManager)
        {
            Debug.Log("Projectile Settings Inited");

            Bullet = resourceManager.GetObjectCopyFast(ResourcesConstants.Gameplay.World.Bullet);

            ImpactEffectPrefab = ResourcesConstants.Gameplay.World.Explosion;
            CollisionMask = LayerMask.GetMask("Obstacle", "Unit");
        }
    }
}