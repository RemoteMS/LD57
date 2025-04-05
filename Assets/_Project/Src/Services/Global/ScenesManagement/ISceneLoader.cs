using Cysharp.Threading.Tasks;
using UniRx;

namespace Services.Global.ScenesManagement
{
    public interface ISceneLoader
    {
        UniTask LoadGamePlay();
        UniTask LoadMainMenu();
        IReadOnlyReactiveProperty<SceneLoadState> sceneStateSubject { get; }
    }
}