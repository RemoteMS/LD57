using System;

namespace Services.Storages.Gameplay
{
    [Serializable]
    public class GunDataSettings
    {
        public float nextFireTime = 0f;
        public float projectileSpeed = 10f;
        public float fireRate = 2f;
        public float reloadTime = 2f;
        public int maxAmmo = 5;
        public int currentAmmo = 5;
        public bool isReloading = false;
    }
}