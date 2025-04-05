using System;
using Core.DI.SceneContainerBuilders;
using Cysharp.Threading.Tasks;
using Reflex.Core;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Services.Global.ScenesManagement
{
    public class SceneLoader : ISceneLoader, IDisposable
    {
        public IReadOnlyReactiveProperty<SceneLoadState> sceneStateSubject => _sceneStateSubject;
        private readonly ReactiveProperty<SceneLoadState> _sceneStateSubject;

        private string _currentSceneName = null;

        private readonly CompositeDisposable _disposables = new();

        public SceneLoader()
        {
            _sceneStateSubject = new ReactiveProperty<SceneLoadState>().AddTo(_disposables);
        }

        public async UniTask LoadGamePlay()
        {
            try
            {
                await LoadSceneAsync(Scenes.Gameplay, new GameplaySceneParameters());
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while loading Gameplay: {e.Message}");
                _sceneStateSubject.Value = SceneLoadState.Error;
            }
        }

        public async UniTask LoadMainMenu()
        {
            try
            {
                await LoadSceneAsync(Scenes.MainMenu, new MainMenuSceneParameters());
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while loading MainMenu: {e.Message}");
                _sceneStateSubject.Value = SceneLoadState.Error;
            }
        }


        private async UniTask LoadSceneAsync(string sceneName, SceneParameters parameters = null)
        {
            _sceneStateSubject.Value = SceneLoadState.Loading;

            if (_currentSceneName != null && _currentSceneName != Scenes.Boot)
            {
                await UnloadCurrentSceneAsync();
            }

            await LoadBootSceneAsync();
            await LoadAndConfigureSceneAsync(sceneName, parameters);

            _currentSceneName = sceneName;

            _sceneStateSubject.Value = SceneLoadState.Loaded;
        }

        private async UniTask UnloadCurrentSceneAsync()
        {
            if (string.IsNullOrEmpty(_currentSceneName))
            {
                Debug.Log($"There is no scene to upload, _currentSceneName is empty.");
                return;
            }

            var scene = SceneManager.GetSceneByName(_currentSceneName);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.Log($"Scene {_currentSceneName} is already unloaded, skip the upload.");
                _currentSceneName = null;
                _sceneStateSubject.Value = SceneLoadState.Unloaded;

                return;
            }

            _sceneStateSubject.Value = SceneLoadState.Unloading;
            var unloadOperation = SceneManager.UnloadSceneAsync(_currentSceneName);
            if (unloadOperation != null)
            {
                try
                {
                    await unloadOperation.ToUniTask();
                    Debug.Log($"Scene {_currentSceneName} is unloaded.");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error while unloading {_currentSceneName}: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning(
                    $"Failed to initiate unloading of scene {_currentSceneName}, it may have already been unloaded.");
            }

            _sceneStateSubject.Value = SceneLoadState.Unloaded;
            _currentSceneName = null;
        }

        private async UniTask LoadBootSceneAsync()
        {
            if (_currentSceneName == Scenes.Boot)
            {
                Debug.Log($"The {Scenes.Boot} scene is already loaded, skip it.");
                return;
            }

            Debug.Log($"Start loading the scene {Scenes.Boot}");
            var loadOperation = SceneManager.LoadSceneAsync(Scenes.Boot, LoadSceneMode.Single);
            if (loadOperation == null)
                throw new NullReferenceException($"Failed to load the scene {Scenes.Boot}");

            loadOperation.allowSceneActivation = false;
            while (loadOperation.progress < 0.9f)
            {
                Debug.Log($"Loading progress {Scenes.Boot}: {loadOperation.progress * 100}%");
                await UniTask.Yield();
            }

            loadOperation.allowSceneActivation = true;
            while (!loadOperation.isDone)
            {
                Debug.Log(
                    $"Waiting for the scene to activate {Scenes.Boot}, progress: {loadOperation.progress * 100}%");
                await UniTask.Yield();
            }

            Debug.Log($"Scene {Scenes.Boot} fully loaded and activated.");
            _currentSceneName = Scenes.Boot;
        }

        private async UniTask LoadAndConfigureSceneAsync(string sceneName, SceneParameters parameters)
        {
            Debug.Log($"Begin loading the scene {sceneName}");
            var loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (loadOperation == null)
                throw new NullReferenceException($"Failed to load the scene {sceneName}");

            loadOperation.allowSceneActivation = false;
            while (loadOperation.progress < 0.9f)
            {
                Debug.Log($"Loading progress {sceneName}: {loadOperation.progress * 100}%");
                await UniTask.Yield();
            }

            if (parameters != null)
            {
                var scene = SceneManager.GetSceneByName(sceneName);
                if (scene.IsValid())
                {
                    try
                    {
                        ReflexSceneManager.PreInstallScene(scene, parameters.Configure);
                        Debug.Log($"Scene {sceneName} configured via ReflexSceneManager.");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Scene {sceneName} Setup Error: {e.Message}");
                        throw;
                    }
                }
                else
                {
                    Debug.LogError($"Failed to get a scene {sceneName} to customize.");
                    throw new InvalidOperationException($"Scene {sceneName} is not valid.");
                }
            }

            loadOperation.allowSceneActivation = true;
            while (!loadOperation.isDone)
            {
                Debug.Log($"Wait for scene {sceneName} to be activated, progress: {loadOperation.progress * 100}%");
                await UniTask.Yield();
            }

            Debug.Log($"Scene {sceneName} is fully loaded and activated.");
        }

        public void Dispose()
        {
            _sceneStateSubject.Dispose();
        }
    }
}