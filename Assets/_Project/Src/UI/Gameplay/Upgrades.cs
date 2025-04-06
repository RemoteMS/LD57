using System;
using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using Services.Gameplay.GameProcessManagement;
using Services.Storages.Gameplay;
using UniRx;
using UnityEngine;

namespace UI.Gameplay
{
    public class Upgrades : MonoBehaviour, IDisposable
    {
        [SerializeField] private GameObject panel;

        private GameplayStorage _storage;

        [Inject]
        public void Inject(GameplayStorage storage, GameProcessManager manager)
        {
            manager.currentState
                .Select(x => x == GameState.Calm)
                .Subscribe(x => { panel.gameObject.SetActive(x); }
                )
                .AddTo(_disposables);
            _storage = storage;
        }

        private readonly CompositeDisposable _disposables = new();

        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}