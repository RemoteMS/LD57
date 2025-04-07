using System;
using System.Globalization;
using Reflex.Attributes;
using Services.Gameplay.GameProcessManagement;
using Services.Global.ScenesManagement;
using Services.Storages.Gameplay;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Gameplay
{
    public class WorldData : MonoBehaviour, IDisposable
    {
        [SerializeField] private TMP_Text waveTimeRemaining_new;

        [SerializeField] private TMP_Text currentState;
        [SerializeField] private TMP_Text remainingEnemies;
        [SerializeField] private TMP_Text waveTimeRemaining;
        [SerializeField] private TMP_Text gameTimer;
        [SerializeField] private TMP_Text timeToWaveEnd;
        [SerializeField] private TMP_Text enemiesCount;
        [SerializeField] private TMP_Text currentWaveIndex;

        [SerializeField] private GameObject killThem;
        [SerializeField] private GameObject surviveForPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Button gameOverButton;

        private readonly CompositeDisposable _disposables = new();
        private GameProcessManager _gameProcessManager;

        private ISceneLoader _sceneLoader;

        [Inject]
        public void Inject(GameProcessManager gameProcessManager, GameplayStorage gameplayStorage,
            ISceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
            _gameProcessManager = gameProcessManager;

            _gameProcessManager.currentState
                .Select(x => x == GameState.WaveActive)
                .Subscribe(x => { surviveForPanel.gameObject.SetActive(x); })
                .AddTo(_disposables);

            _gameProcessManager.currentState
                .Select(x => x == GameState.WaitingForEnemies)
                .Subscribe(x => { killThem.gameObject.SetActive(x); })
                .AddTo(_disposables);

            _gameProcessManager.currentState
                .Subscribe(x => { currentState.text = x.ToString(); })
                .AddTo(_disposables);

            _gameProcessManager.remainingEnemies
                .Subscribe(x => { remainingEnemies.text = x.ToString(); })
                .AddTo(_disposables);

            _gameProcessManager.stateTimeRemaining
                .Subscribe(x =>
                {
                    waveTimeRemaining.text = x.ToString(CultureInfo.InvariantCulture);
                    waveTimeRemaining_new.text = x.ToString(CultureInfo.InvariantCulture);
                })
                .AddTo(_disposables);

            _gameProcessManager.stateTimeElapsed
                .Subscribe(x => { gameTimer.text = x.ToString(CultureInfo.InvariantCulture); })
                .AddTo(_disposables);

            _gameProcessManager.stateTimeRemaining
                .Subscribe(x =>
                {
                    timeToWaveEnd.text = x > 0
                        ? $"Wave ends in: {x.ToString("F1", CultureInfo.InvariantCulture)}"
                        : "No active wave";
                })
                .AddTo(_disposables);


            _gameProcessManager.hasLost
                .Where(x => x)
                .Subscribe(x => { ShowGameOverWindow(); })
                .AddTo(_disposables);

            _gameProcessManager.currentWaveIndex.Subscribe(x => { currentWaveIndex.text = x.ToString(); })
                .AddTo(_disposables);

            gameplayStorage.enemiesCount.Subscribe(x => { enemiesCount.text = x.ToString(); }).AddTo(_disposables);
        }

        private void ShowGameOverWindow()
        {
            gameOverPanel.SetActive(true);
            gameOverButton.OnClickAsObservable().Subscribe(
                x => { _sceneLoader.LoadMainMenu(); }
            ).AddTo(_disposables);
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