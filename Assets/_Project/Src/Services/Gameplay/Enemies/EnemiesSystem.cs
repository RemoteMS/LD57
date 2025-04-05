using UnityEngine;

namespace Services.Gameplay.Enemies
{
    public class EnemiesSystem : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private float spawnInterval = 2f;

        [SerializeField] private Transform[] _spawnPoints;
        private float _timer;

        private void Start()
        {
            _timer = spawnInterval;
        }

        private void Update()
        {
            _timer -= Time.deltaTime;

            if (_timer <= 0)
            {
                SpawnBullet();
                _timer = spawnInterval;
            }
        }

        private void SpawnBullet()
        {
            var spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];

            Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        }
    }
}