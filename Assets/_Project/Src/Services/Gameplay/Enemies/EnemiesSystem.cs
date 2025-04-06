using System;
using Reflex.Attributes;
using Services.Gameplay.GameProcessManagement;
using Services.Storages.Gameplay;
using UniRx;
using UnityEngine;

namespace Services.Gameplay.Enemies
{
    public class EnemiesSystem : MonoBehaviour, IDisposable
    {
        [SerializeField] private AudioSource AudioSourceAwake;
        [SerializeField] private AudioClip AudioAwake;
        
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private float spawnInterval = 2f;
        [SerializeField] private Transform[] _spawnPoints;

        private float _timer;
        private bool _canSpawn;
        private GameProcessManager _manager;
        private readonly CompositeDisposable _disposables = new();

        private GameplayStorage _gameplayStorage;

        [Inject]
        public void Inject(GameProcessManager manager, GameplayStorage gameplayStorage)
        {
            _manager = manager;
            _gameplayStorage = gameplayStorage;

            _manager.currentState
                .Subscribe(state => { _canSpawn = state == GameState.WaveActive; })
                .AddTo(_disposables);

            _manager.remainingEnemies
                .Where(count => count <= 0 && _manager.currentState.Value == GameState.WaveActive)
                .Subscribe(_ => _canSpawn = false)
                .AddTo(_disposables);
        }

        private void Start()
        {
            _timer = spawnInterval;
            _canSpawn = false;
        }

        private void Update()
        {
            if (!_canSpawn) return;

            _timer -= Time.deltaTime;

            if (_timer <= 0)
            {
                SpawnEnemy();
                _timer = spawnInterval;
            }
        }

        private void SpawnEnemy()
        {
            if (_spawnPoints.Length == 0)
            {
                Debug.LogWarning("No spawn points assigned!");
                return;
            }

            var spawnPoint = _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Length)];

            // enemy 
            var instantiate = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            var component = instantiate.GetComponent<Enemy>();

            _gameplayStorage.AddEnemy(component);


            AudioSourceAwake.gameObject.transform.position = spawnPoint.position;
            AudioSourceAwake.PlayOneShot(AudioAwake);
            // Instantiate sound effect
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}