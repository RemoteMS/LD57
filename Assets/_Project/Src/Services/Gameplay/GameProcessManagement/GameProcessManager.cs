using System;
using Services.Gameplay.Economic;
using UniRx;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Services.Gameplay.GameProcessManagement
{
    public enum GameState
    {
        Calm,
        WaveActive,
        WaveCooldown
    }

    [Serializable]
    public class WaveSettings
    {
        public float WaveDuration;
        public float CalmDuration;
        public int EnemiesPerWave;
    }

    public class GameProcessManager : IDisposable
    {
        private readonly EconomicSystem _economicSystem;
        private readonly CompositeDisposable _disposables = new();
        private readonly List<WaveSettings> _waveSettings;

        private readonly ReactiveProperty<GameState> _currentState;
        private readonly ReactiveProperty<float> _waveTimeRemaining;
        private readonly ReactiveProperty<int> _remainingEnemies;
        private readonly ReactiveProperty<float> _gameTimer;
        private readonly ReactiveProperty<float> _timeToWaveEnd;

        private readonly ReactiveProperty<bool> _hasLost;

        public IReadOnlyReactiveProperty<GameState> currentState => _currentState;
        public IReadOnlyReactiveProperty<float> waveTimeRemaining => _waveTimeRemaining;
        public IReadOnlyReactiveProperty<int> remainingEnemies => _remainingEnemies;
        public IReadOnlyReactiveProperty<float> gameTimer => _gameTimer;
        public IReadOnlyReactiveProperty<float> timeToWaveEnd => _timeToWaveEnd;
        public IReadOnlyReactiveProperty<bool> hasLost => _hasLost;

        private int _currentWaveIndex;
        private bool _isRunning;

        public GameProcessManager(EconomicSystem economicSystem)
        {
            _economicSystem = economicSystem ?? throw new ArgumentNullException(nameof(economicSystem));
            _waveSettings = CreateDefaultWaveSettings();

            _currentState = new ReactiveProperty<GameState>(GameState.Calm).AddTo(_disposables);
            _waveTimeRemaining = new ReactiveProperty<float>(0f).AddTo(_disposables);
            _remainingEnemies = new ReactiveProperty<int>(0).AddTo(_disposables);
            _gameTimer = new ReactiveProperty<float>(0f).AddTo(_disposables);
            _timeToWaveEnd = new ReactiveProperty<float>(0f).AddTo(_disposables);
            _hasLost = new ReactiveProperty<bool>(false).AddTo(_disposables);

            SetupSubscriptions();
        }

        public async UniTask RunGameAsync()
        {
            if (_isRunning) return;
            _isRunning = true;
            _currentWaveIndex = 0;
            _gameTimer.Value = 0f; // Сброс таймера при старте

            StartGameTimer(); // Запуск общего таймера

            try
            {
                while (IsGameActive())
                {
                    await ProcessCurrentState();
                }
            }
            finally
            {
                _isRunning = false;
            }
        }

        public void StopGame()
        {
            _isRunning = false;
        }

        public void RegisterEnemyDefeat()
        {
            if (_remainingEnemies.Value > 0)
            {
                _remainingEnemies.Value--;
            }
        }

        public void Dispose()
        {
            _isRunning = false;
            _disposables?.Dispose();
        }

        private async UniTask ProcessCurrentState()
        {
            switch (_currentState.Value)
            {
                case GameState.Calm:
                    await ProcessCalmState();
                    break;
                case GameState.WaveActive:
                    await ProcessWaveState();
                    break;
                case GameState.WaveCooldown:
                    await ProcessCooldownState();
                    break;
            }
        }

        private async UniTask ProcessCalmState()
        {
            ResetWaveState();
            if (IsLastWaveCompleted())
            {
                _isRunning = false;
                return;
            }

            await UniTask.Delay(TimeSpan.FromSeconds(_waveSettings[_currentWaveIndex].CalmDuration));
            if (_isRunning) BeginWave();
        }

        private async UniTask ProcessWaveState()
        {
            await UniTask.WaitUntil(() => _waveTimeRemaining.Value <= 0 || !_isRunning);
            if (_isRunning) TransitionToCooldown();
        }

        private async UniTask ProcessCooldownState()
        {
            await UniTask.WaitUntil(() => _remainingEnemies.Value <= 0 || !_isRunning);
            if (_isRunning) PrepareNextWave();
        }

        private void BeginWave()
        {
            if (IsLastWaveCompleted()) return;

            WaveSettings settings = _waveSettings[_currentWaveIndex];
            _currentState.Value = GameState.WaveActive;
            _waveTimeRemaining.Value = settings.WaveDuration;
            _remainingEnemies.Value = settings.EnemiesPerWave;
            _timeToWaveEnd.Value = settings.WaveDuration;

            StartWaveTimer();
        }

        private void StartWaveTimer()
        {
            Observable.Timer(TimeSpan.FromSeconds(0.1f), TimeSpan.FromSeconds(0.1f))
                .TakeWhile(_ => _waveTimeRemaining.Value > 0 && _isRunning)
                .Subscribe(_ =>
                {
                    _waveTimeRemaining.Value -= 0.1f;
                    if (_currentState.Value == GameState.WaveActive)
                    {
                        _timeToWaveEnd.Value = _waveTimeRemaining.Value; // Синхронизируем
                    }
                })
                .AddTo(_disposables);
        }

        private void StartGameTimer()
        {
            Observable.Timer(TimeSpan.FromSeconds(0.1f), TimeSpan.FromSeconds(0.1f))
                .TakeWhile(_ => _isRunning)
                .Subscribe(_ => _gameTimer.Value += 0.1f)
                .AddTo(_disposables);
        }

        private void SetupSubscriptions()
        {
            _economicSystem.globalHealthCount
                .Where(health => health <= 0)
                .Subscribe(_ => HandleGameOver())
                .AddTo(_disposables);
        }

        private void HandleGameOver()
        {
            Debug.LogError($"Game Over - {_economicSystem.globalHealthCount.Value}");

            _currentState.Value = GameState.Calm;
            _currentWaveIndex = 0;
            _isRunning = false;

            _hasLost.Value = true;
        }

        private void ResetWaveState()
        {
            _waveTimeRemaining.Value = 0;
            _remainingEnemies.Value = 0;
            _timeToWaveEnd.Value = 0; // Сбрасываем время до конца волны
        }

        private void TransitionToCooldown() => _currentState.Value = GameState.WaveCooldown;

        private void PrepareNextWave()
        {
            _currentWaveIndex++;
            _currentState.Value = GameState.Calm;
        }

        private bool IsGameActive() => _economicSystem.globalHealthCount.Value > 0 && _isRunning;
        private bool IsLastWaveCompleted() => _currentWaveIndex >= _waveSettings.Count;

        private static List<WaveSettings> CreateDefaultWaveSettings() => new List<WaveSettings>
        {
            new WaveSettings { WaveDuration = 30f, CalmDuration = 10f, EnemiesPerWave = 5 },
            new WaveSettings { WaveDuration = 40f, CalmDuration = 15f, EnemiesPerWave = 10 }
        };
    }
}