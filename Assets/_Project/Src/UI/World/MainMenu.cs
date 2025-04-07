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
        public Button exit;

        private ISceneLoader _loader;

        [Inject]
        public void Inject(ISceneLoader loader)
        {
            _loader = loader;

            toGame.OnClickAsObservable().Subscribe(x => { _loader.LoadGamePlay(); }).AddTo(_disposables);
            exit.OnClickAsObservable().Subscribe(x =>
            {
#if UNITY_EDITOR
                if (UnityEditor.EditorApplication.isPlaying)
                {
                    UnityEditor.EditorApplication.isPlaying = false;
                }
#else
                Application.Quit();
#endif
            }).AddTo(_disposables);
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}