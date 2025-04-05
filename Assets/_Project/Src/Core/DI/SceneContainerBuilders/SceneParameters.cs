using Reflex.Core;

namespace Core.DI.SceneContainerBuilders
{
    public abstract class SceneParameters
    {
        public abstract void Configure(ContainerBuilder builder);
    }
}