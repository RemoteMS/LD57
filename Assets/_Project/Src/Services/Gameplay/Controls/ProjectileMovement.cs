using UnityEngine;

namespace Services.Gameplay.Controls
{
    [SelectionBase]
    public class ProjectileMovement : MonoBehaviour
    {
        private Vector3 _direction;
        private float _speed;

        public void Initialize(Vector3 dir, float spd)
        {
            _direction = dir;
            _speed = spd;
        }

        private void Update()
        {
            Vector3 dir = _direction;
            transform.Translate(_direction * (_speed * Time.deltaTime));
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.LogWarning($"Projectile triggered with {other.gameObject.name}");
            Destroy(gameObject);
        }
    }
}