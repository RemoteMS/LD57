using Reflex.Core;

namespace Core.DI.SceneContainerBuilders
{
    public class GameplaySceneParameters : SceneParameters
    {
        public override void Configure(ContainerBuilder builder)
        {
            builder.SetName($"GameplayScene");
        }
    }
}