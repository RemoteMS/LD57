using System;
using System.Globalization;
using Reflex.Attributes;
using Services.Gameplay.GameProcessManagement;
using TMPro;
using UniRx;
using UnityEngine;

namespace UI.Gameplay
{
    public class WorldData : MonoBehaviour, IDisposable
    {
        [SerializeField] private TMP_Text currentState;
        [SerializeField] private TMP_Text remainingEnemies;
        [SerializeField] private TMP_Text waveTimeRemaining;
        [SerializeField] private TMP_Text gameTimer;
        [SerializeField] private TMP_Text timeToWaveEnd;

        private readonly CompositeDisposable _disposables = new();
        private GameProcessManager _gameProcessManager;

        [Inject]
        public void Inject(GameProcessManager gameProcessManager)
        {
            _gameProcessManager = gameProcessManager;

            _gameProcessManager.currentState
                .Subscribe(x => { currentState.text = x.ToString(); })
                .AddTo(_disposables);

            _gameProcessManager.remainingEnemies
                .Subscribe(x => { remainingEnemies.text = x.ToString(); })
                .AddTo(_disposables);

            _gameProcessManager.waveTimeRemaining
                .Subscribe(x => { waveTimeRemaining.text = x.ToString(CultureInfo.InvariantCulture); })
                .AddTo(_disposables);

            _gameProcessManager.gameTimer
                .Subscribe(x => { gameTimer.text = x.ToString(CultureInfo.InvariantCulture); })
                .AddTo(_disposables);

            _gameProcessManager.timeToWaveEnd
                .Subscribe(x =>
                {
                    timeToWaveEnd.text = x > 0
                        ? $"Wave ends in: {x.ToString("F1", CultureInfo.InvariantCulture)}"
                        : "No active wave";
                })
                .AddTo(_disposables);
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