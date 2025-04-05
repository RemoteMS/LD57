using Reflex.Core;
using Services.Global.Audio;
using Services.Global.ResourceManagement;
using Services.Global.ScenesManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class App
{
    private static App _instance;

    private readonly Container _rootContainer;

    private IDataManager _dataManager;
    private readonly ISceneLoader _sceneLoader;

    public static void AutostartGame(Container projectRootContainer)
    {
        _instance = new App(projectRootContainer);
        _instance.RunGame();
    }

    private App(Container projectRootContainer)
    {
        _rootContainer = projectRootContainer;
        _sceneLoader = _rootContainer.Resolve<ISceneLoader>();

        var asyncSceneLoader = new GameObject("[AsyncSceneLoader]");
        Object.DontDestroyOnLoad(asyncSceneLoader);

        var scopes = new GameObject("[SCOPE]");
        Object.DontDestroyOnLoad(scopes);
    }

    private async void RunGame()
    {
        _dataManager = _rootContainer.Resolve<IDataManager>();

        var audioMixer = await _dataManager.LoadAssetAsync<AudioMixer>(ResourcesConstants.Audio.Mixers.Main);
        var audioManager = await _dataManager.InstantiatePrefabAsync(ResourcesConstants.Audio.AudioManager);
        Object.DontDestroyOnLoad(audioManager);

        var audioService = _rootContainer.Resolve<IAudioService>();
        audioService.Bind(audioManager);
        audioService.InjectAudioMixer(audioMixer);


#if UNITY_EDITOR
        var sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == Scenes.Gameplay)
        {
            await _sceneLoader.LoadGamePlay();
            return;
        }

        if (sceneName == Scenes.MainMenu)
        {
            await _sceneLoader.LoadMainMenu();
            return;
        }

        if (sceneName is Scenes.Boot)
        {
            await _sceneLoader.LoadMainMenu();
            return;
        }
#endif
        await _sceneLoader.LoadMainMenu();
    }
}