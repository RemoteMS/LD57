using System;
using Reflex.Attributes;
using Services.Global.ScenesManagement;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Src.UI.World
{
    public class MainMenu : MonoBehaviour
    {
        private CompositeDisposable _disposables = new();

        public Button toGame;

        private ISceneLoader _loader;

        [Inject]
        public void Inject(ISceneLoader loader)
        {
            _loader = loader;

            toGame.OnClickAsObservable().Subscribe(x => { _loader.LoadGamePlay(); }).AddTo(_disposables);
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}