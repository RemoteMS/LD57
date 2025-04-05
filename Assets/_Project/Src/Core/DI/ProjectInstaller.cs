using System;
using Reflex.Core;
using Services.Global.Audio;
using Services.Global.ResourceManagement;
using Services.Global.ScenesManagement;
using UnityEngine;

namespace Core.DI
{
    public class ProjectInstaller : MonoBehaviour, IInstaller
    {
        public void InstallBindings(ContainerBuilder builder)
        {
            builder.AddSingleton(typeof(AudioService),    new[] { typeof(IAudioService), typeof(IDisposable) });
            builder.AddSingleton(typeof(SceneLoader),     new[] { typeof(ISceneLoader), typeof(IDisposable) });
            builder.AddSingleton(typeof(ResourceManager), new[] { typeof(IDataManager), typeof(IDisposable) });

            builder.OnContainerBuilt += ContainerBuild;
        }

        private static void ContainerBuild(Container builder)
        {
            builder.Resolve<IAudioService>();
            builder.Resolve<ISceneLoader>();
            builder.Resolve<IDataManager>();

            App.AutostartGame(builder);
        }
    }
}