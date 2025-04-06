using System;
using Reflex.Attributes;
using Services.Gameplay.GameProcessManagement;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Gameplay
{
    public class GameProcessManagerControl : MonoBehaviour, IDisposable
    {
        private GameProcessManager _processManager;
        
        [SerializeField] private Button startWave;

        private readonly CompositeDisposable _disposables = new();

        [Inject]
        public void Inject(GameProcessManager processManager)
        {
            _processManager = processManager;

            _processManager.currentState
                .Select(x => x == GameState.Calm)
                .Subscribe(x =>
                {
                    startWave.gameObject.SetActive(x);
                })
                .AddTo(_disposables);

            startWave.onClick.AsObservable()
                .Subscribe(
                    x =>
                    {
                        _processManager.ForceStartWaveApproaching();
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