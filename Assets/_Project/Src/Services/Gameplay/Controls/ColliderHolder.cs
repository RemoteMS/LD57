using UnityEngine;

namespace Services.Gameplay.Controls
{
    public class ColliderHolder : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            Debug.LogWarning($"Projectile triggered with {other.gameObject.name}");
            Destroy(gameObject);
        }
    }
}