using System;
using Reflex.Core;
using Services.Gameplay.BulletSystem;
using Services.Gameplay.BulletSystem.Particles;
using Services.Gameplay.Economic;
using Services.Gameplay.Enemies;

namespace Core.DI.SceneContainerBuilders
{
    public class GameplaySceneParameters : SceneParameters
    {
        public override void Configure(ContainerBuilder builder)
        {
            builder.SetName($"GameplayScene");

            builder.AddSingleton(typeof(ProjectileSettings), typeof(ProjectileSettings));

            builder.AddSingleton(typeof(RaycastBatchProcessor), typeof(RaycastBatchProcessor), typeof(IDisposable));
            builder.AddSingleton(typeof(InGameEffectSystem),    typeof(InGameEffectSystem),    typeof(IDisposable));
            builder.AddSingleton(typeof(BulletManager),         typeof(BulletManager),         typeof(IDisposable));
            builder.AddSingleton(typeof(EconomicSystem),        typeof(EconomicSystem),         typeof(IDisposable));

            builder.OnContainerBuilt += OnBuild;
        }

        private void OnBuild(Container container)
        {
            container.Resolve<BulletManager>();
        }
    }
}