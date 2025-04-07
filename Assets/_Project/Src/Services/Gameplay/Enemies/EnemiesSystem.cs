using System;
using Reflex.Attributes;
using Services.Gameplay.GameProcessManagement;
using Services.Storages.Gameplay;
using UniRx;
using UnityEngine;
using Cysharp.Threading.Tasks; // Добавляем пространство имен для UniTask

namespace Services.Gameplay.Enemies
{
    public class EnemiesSystem : MonoBehaviour, IDisposable
    {
        [SerializeField] private AudioSource AudioSourceAwake;
        [SerializeField] private AudioClip AudioAwake;

        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Transform[] _spawnPoints;

        private GameProcessManager _manager;
        private readonly CompositeDisposable _disposables = new();
        private GameplayStorage _gameplayStorage;
        private UniTask? _spawnTask; // Для хранения текущей задачи спавна

        [Inject]
        public void Inject(GameProcessManager manager, GameplayStorage gameplayStorage)
        {
            _manager = manager;
            _gameplayStorage = gameplayStorage;

            // Подписка на начало WaveActive для запуска спавна
            _manager.currentState
                .Where(state => state == GameState.WaveActive)
                .Subscribe(_ => StartSpawningEnemies())
                .AddTo(_disposables);

            // Отключаем спавн, если враги закончились или волна прервана
            _manager.currentState
                .Where(state => state != GameState.WaveActive)
                .Subscribe(_ => StopSpawning())
                .AddTo(_disposables);

            _manager.remainingEnemies
                .Where(count => count <= 0 && _manager.currentState.Value == GameState.WaveActive)
                .Subscribe(_ => StopSpawning())
                .AddTo(_disposables);
        }

        private void StartSpawningEnemies()
        {
            var currentSettings = _manager.GetCurrent();


            if (currentSettings == null)
            {
                Debug.LogWarning("No current wave settings available!");
                return;
            }

            var enemiesToSpawn = currentSettings.enemiesPerWave;
            var waveDuration = currentSettings.waveDuration;

            if (enemiesToSpawn <= 0 || waveDuration <= 0)
            {
                Debug.LogWarning("Invalid wave settings: enemies or duration is zero or negative.");
                return;
            }

            StopSpawning();


            _spawnTask = SpawnEnemiesOverTime(enemiesToSpawn, waveDuration);
        }

        private async UniTask SpawnEnemiesOverTime(int enemiesToSpawn, float duration)
        {
            if (_spawnPoints.Length == 0)
            {
                Debug.LogWarning("No spawn points assigned!");
                return;
            }

            var spawnInterval = duration / enemiesToSpawn;

            for (var i = 0; i < enemiesToSpawn; i++)
            {
                if (_manager.currentState.Value != GameState.WaveActive || !_manager.isRunning.Value)
                {
                    break;
                }

                SpawnSingleEnemy();

                await UniTask.Delay(TimeSpan.FromSeconds(spawnInterval),
                    cancellationToken: this.GetCancellationTokenOnDestroy());
            }

            Debug.Log($"Finished spawning {enemiesToSpawn} enemies for wave {_manager.currentWaveIndex.Value}");
        }

        private void SpawnSingleEnemy()
        {
            var spawnPoint = _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Length)];

            var instantiate = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            var component = instantiate.GetComponent<Enemy>();

            _gameplayStorage.AddEnemy(component);

            AudioSourceAwake.gameObject.transform.position = spawnPoint.position;
            AudioSourceAwake.PlayOneShot(AudioAwake);
        }

        private void StopSpawning()
        {
            _spawnTask = null; // UniTask автоматически остановится благодаря cancellationToken
        }

        public void Dispose()
        {
            _disposables?.Dispose();
            StopSpawning();
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}