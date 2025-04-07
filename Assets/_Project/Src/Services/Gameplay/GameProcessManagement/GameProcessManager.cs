using System;
using Services.Gameplay.Economic;
using UniRx;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Services.Global.Audio;
using Services.Storages.Gameplay;
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

        public WaveSettings(float waveDuration, int enemiesPerWave)
        {
            this.waveDuration = waveDuration;
            this.enemiesPerWave = enemiesPerWave;
        }
    }

    public class GameProcessManager : IDisposable
    {
        private readonly EconomicSystem _economicSystem;
        private readonly IAudioService _audioService;
        private readonly GameplayStorage _storage;
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

        public GameProcessManager(EconomicSystem economicSystem, IAudioService audioService, GameplayStorage storage)
        {
            _economicSystem = economicSystem ?? throw new ArgumentNullException(nameof(economicSystem));
            _audioService = audioService;
            _storage = storage;
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

            // _currentState
            //     .Select(x => x == GameState.WaitingForEnemies)
            //     .Skip(1)
            //     .Subscribe(x =>
            //     {
            //         if (_storage.enemiesCount.Value <= 0)
            //         {
            //             ForceStartCalm();
            //         }
            //     })
            //     .AddTo(_disposables);
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
            new(1f, 1),
            new(10f, 5),
            new(5f, 10),
            new(10f, 15),
            new(20f, 20),
            new(30f, 25),
            new(40f, 30),
            new(50f, 35),
            new(60f, 40),
            new(70f, 45),
            new(80f, 50),
            new(90f, 55),
            new(100f, 60),
            new(110f, 65),
            new(120f, 70),
            new(130f, 75),
            new(140f, 80),
            new(150f, 85),
            new(160f, 90),
            new(170f, 95),
            new(180f, 100),
            new(190f, 210),
            new(200f, 220),
            new(210f, 1250),
        };
    }
}