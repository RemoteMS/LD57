using Reflex.Core;

namespace Core.DI.SceneContainerBuilders
{
    public class MainMenuSceneParameters : SceneParameters
    {
        public override void Configure(ContainerBuilder builder)
        {
            builder.SetName($"MainMenu");
        }
    }
}