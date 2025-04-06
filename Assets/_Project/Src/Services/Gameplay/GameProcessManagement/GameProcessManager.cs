using System;
using Services.Gameplay.Economic;
using UniRx;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Services.Global.Audio;
using UnityEngine;

namespace Services.Gameplay.GameProcessManagement
{
    public enum GameState
    {
        Calm,
        WaveApproaching,
        WaveActive,
        WaitingForEnemies,
        Lost
    }

    [Serializable]
    public class WaveSettings
    {
        public float waveDuration;
        public int enemiesPerWave;
    }

    public class GameProcessManager : IDisposable
    {
        private readonly EconomicSystem _economicSystem;
        private readonly IAudioService _audioService;
        private readonly CompositeDisposable _disposables = new();
        private readonly List<WaveSettings> _waveSettings;

        private readonly ReactiveProperty<GameState> _currentState;
        private readonly ReactiveProperty<int> _remainingEnemies;
        private readonly ReactiveProperty<bool> _isRunning;
        private readonly ReactiveProperty<bool> _hasLost;

        // Таймеры для каждого состояния
        private readonly ReactiveProperty<float> _stateTimeRemaining; // Сколько осталось до конца текущего состояния
        private readonly ReactiveProperty<float> _stateTimeElapsed;   // Сколько времени прошло в текущем состоянии

        public IReadOnlyReactiveProperty<int> currentWaveIndex => _currentWaveIndex;
        private readonly ReactiveProperty<int> _currentWaveIndex;

        private IDisposable _currentTimerSubscription; // Храним текущую подписку на таймер

        public IReadOnlyReactiveProperty<GameState> currentState => _currentState;
        public IReadOnlyReactiveProperty<int> remainingEnemies => _remainingEnemies;
        public IReadOnlyReactiveProperty<bool> isRunning => _isRunning;
        public IReadOnlyReactiveProperty<bool> hasLost => _hasLost;
        public IReadOnlyReactiveProperty<float> stateTimeRemaining => _stateTimeRemaining;
        public IReadOnlyReactiveProperty<float> stateTimeElapsed => _stateTimeElapsed;

        private const float WaveApproachingDuration = 5f; // Длительность "волна приближается" в секундах

        public GameProcessManager(EconomicSystem economicSystem, IAudioService audioService)
        {
            _economicSystem = economicSystem ?? throw new ArgumentNullException(nameof(economicSystem));
            _audioService = audioService;
            _waveSettings = CreateDefaultWaveSettings();

            _currentState = new ReactiveProperty<GameState>(GameState.Calm).AddTo(_disposables);
            _remainingEnemies = new ReactiveProperty<int>(0).AddTo(_disposables);
            _isRunning = new ReactiveProperty<bool>(false).AddTo(_disposables);
            _hasLost = new ReactiveProperty<bool>(false).AddTo(_disposables);
            _stateTimeRemaining = new ReactiveProperty<float>(0f).AddTo(_disposables);
            _stateTimeElapsed = new ReactiveProperty<float>(0f).AddTo(_disposables);

            _currentWaveIndex = new ReactiveProperty<int>(0).AddTo(_disposables);

            SetupSubscriptions();
        }

        public async UniTask RunGameAsync()
        {
            if (_isRunning.Value) return;
            _isRunning.Value = true;
            _currentWaveIndex.Value = 0;

            try
            {
                while (IsGameActive())
                {
                    await ProcessCurrentState();
                }
            }
            finally
            {
                _isRunning.Value = false;
            }
        }

        public void StopGame()
        {
            _isRunning.Value = false;
            StopTimer();
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
            _isRunning.Value = false;
            StopTimer();
            _disposables?.Dispose();
        }

        private async UniTask ProcessCurrentState()
        {
            switch (_currentState.Value)
            {
                case GameState.Calm:
                    await ProcessCalmState();
                    break;
                case GameState.WaveApproaching:
                    await ProcessWaveApproachingState();
                    break;
                case GameState.WaveActive:
                    await ProcessWaveActiveState();
                    break;
                case GameState.WaitingForEnemies:
                    await ProcessWaitingForEnemiesState();
                    break;
                case GameState.Lost:
                    await UniTask.CompletedTask;
                    break;
            }
        }

        private async UniTask ProcessCalmState()
        {
            ResetStateTimers();
            _currentWaveIndex.Value += 1;
            await UniTask.WaitWhile(() => _currentState.Value == GameState.Calm && _isRunning.Value);
        }

        private async UniTask ProcessWaveApproachingState()
        {
            StartStateTimer(WaveApproachingDuration);
            await UniTask.WaitUntil(() => _stateTimeRemaining.Value <= 0 || !_isRunning.Value);
            if (_isRunning.Value) ForceStartWaveActive();
        }

        private async UniTask ProcessWaveActiveState()
        {
            await UniTask.WaitUntil(() => _stateTimeRemaining.Value <= 0 || !_isRunning.Value);
            if (_isRunning.Value) ForceStartWaitingForEnemies();
        }

        private async UniTask ProcessWaitingForEnemiesState()
        {
            ResetStateTimers();
            await UniTask.WaitUntil(() => _remainingEnemies.Value <= 0 || !_isRunning.Value);
            if (_isRunning.Value) ForceStartCalm();
        }

        // Методы принудительного переключения состояний
        public void ForceStartCalm()
        {
            if (!_isRunning.Value) return;
            _currentState.Value = GameState.Calm;
            ResetStateTimers();
            _remainingEnemies.Value = 0;
        }

        public void ForceStartWaveApproaching()
        {
            if (!_isRunning.Value || IsLastWaveCompleted()) return;

            _currentState.Value = GameState.WaveApproaching;

            _audioService.PlaySyreneComing();
            StartStateTimer(WaveApproachingDuration);
        }

        public WaveSettings GetCurrent()
        {
            Debug.LogWarning(
                $"current - {_currentWaveIndex.Value}, {_waveSettings[_currentWaveIndex.Value].waveDuration}, {_waveSettings[_currentWaveIndex.Value].enemiesPerWave}");
            return _waveSettings[_currentWaveIndex.Value];
        }

        public void ForceStartWaveActive()
        {
            if (!_isRunning.Value || IsLastWaveCompleted()) return;
            var settings = _waveSettings[_currentWaveIndex.Value];
            _currentState.Value = GameState.WaveActive;
            _remainingEnemies.Value = settings.enemiesPerWave;
            StartStateTimer(settings.waveDuration);
        }

        public void ForceStartWaitingForEnemies()
        {
            if (!_isRunning.Value) return;
            _currentState.Value = GameState.WaitingForEnemies;
            ResetStateTimers();
        }

        public void ForceStartLost()
        {
            _currentState.Value = GameState.Lost;
            _isRunning.Value = false;
            _hasLost.Value = true;
            ResetStateTimers();
        }

        private void StartStateTimer(float duration)
        {
            StopTimer();

            _stateTimeRemaining.Value = duration;
            _stateTimeElapsed.Value = 0f;

            Debug.Log($"Starting timer for state {_currentState.Value} with duration {duration}s");

            _currentTimerSubscription = Observable.Timer(TimeSpan.FromSeconds(0.1f), TimeSpan.FromSeconds(0.1f))
                .TakeWhile(_ => _stateTimeRemaining.Value > 0 && _isRunning.Value)
                .Subscribe(_ =>
                {
                    _stateTimeRemaining.Value -= 0.1f;
                    _stateTimeElapsed.Value += 0.1f;
                }, () => Debug.Log($"Timer for state {_currentState.Value} completed"))
                .AddTo(_disposables);
        }

        private void StopTimer()
        {
            _currentTimerSubscription?.Dispose();
            _currentTimerSubscription = null;
        }

        private void ResetStateTimers()
        {
            StopTimer();
            _stateTimeRemaining.Value = 0f;
            _stateTimeElapsed.Value = 0f;
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
            ForceStartLost();
        }

        private bool IsGameActive() => _economicSystem.globalHealthCount.Value > 0 && _isRunning.Value &&
                                       _currentState.Value                     != GameState.Lost;

        private bool IsLastWaveCompleted() => _currentWaveIndex.Value >= _waveSettings.Count;

        private static List<WaveSettings> CreateDefaultWaveSettings() => new List<WaveSettings>
        {
            new WaveSettings { waveDuration = 5f, enemiesPerWave = 5 },
            new WaveSettings { waveDuration = 5f, enemiesPerWave = 5 },
            new WaveSettings { waveDuration = 5f, enemiesPerWave = 10 },
            new WaveSettings { waveDuration = 10f, enemiesPerWave = 15 },
            new WaveSettings { waveDuration = 20f, enemiesPerWave = 20 },
            new WaveSettings { waveDuration = 30f, enemiesPerWave = 25 },
            new WaveSettings { waveDuration = 40f, enemiesPerWave = 30 },
            new WaveSettings { waveDuration = 50f, enemiesPerWave = 35 },
            new WaveSettings { waveDuration = 60f, enemiesPerWave = 40 },
            new WaveSettings { waveDuration = 70f, enemiesPerWave = 45 },
            new WaveSettings { waveDuration = 80f, enemiesPerWave = 50 },
            new WaveSettings { waveDuration = 90f, enemiesPerWave = 55 },
            new WaveSettings { waveDuration = 100f, enemiesPerWave = 60 },
            new WaveSettings { waveDuration = 110f, enemiesPerWave = 65 },
            new WaveSettings { waveDuration = 120f, enemiesPerWave = 70 },
            new WaveSettings { waveDuration = 130f, enemiesPerWave = 75 },
            new WaveSettings { waveDuration = 140f, enemiesPerWave = 80 },
            new WaveSettings { waveDuration = 150f, enemiesPerWave = 85 },
            new WaveSettings { waveDuration = 160f, enemiesPerWave = 90 },
            new WaveSettings { waveDuration = 170f, enemiesPerWave = 95 },
            new WaveSettings { waveDuration = 180f, enemiesPerWave = 100 },
            new WaveSettings { waveDuration = 190f, enemiesPerWave = 210 },
            new WaveSettings { waveDuration = 200f, enemiesPerWave = 220 },
            new WaveSettings { waveDuration = 210f, enemiesPerWave = 1250 },
        };
    }
}