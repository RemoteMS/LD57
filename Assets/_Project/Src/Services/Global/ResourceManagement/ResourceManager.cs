using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

namespace Services.Global.ResourceManagement
{
    public class ResourceManager : IDataManager, IDisposable
    {
        public void Dispose()
        {
            Resources.UnloadUnusedAssets();
        }

        public async UniTask<GameObject> InstantiatePrefabAsync(string path, Vector3 position = default,
            Quaternion rotation = default,
            Transform parent = null)
        {
            try
            {
                var prefab = await LoadAssetAsync<GameObject>(path);
                if (prefab == null)
                {
                    Debug.LogError($"Failed to load prefab at path: {path}");
                    return null;
                }

                var instance = Object.Instantiate(prefab, position, rotation, parent);
                return instance;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error instantiating prefab {path}: {ex.Message}");
                return null;
            }
        }

        public async UniTask<T> LoadAssetAsync<T>(string path) where T : Object
        {
            try
            {
                var resourceRequest = Resources.LoadAsync<T>(path);
                await resourceRequest.ToUniTask();
                var asset = resourceRequest.asset as T;

                if (asset == null)
                {
                    Debug.LogError($"Failed to load resource at path: {path}");
                    return null;
                }

                return asset;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading resource {path}: {ex.Message}");
                return null;
            }
        }

        public async UniTask<AudioMixer> GetAudioMixer(string address)
        {
            return await Resources.LoadAsync<AudioMixer>(address) as AudioMixer;
        }

        public async UniTask<Sprite> GetSprite(string address)
        {
            var resourceRequest = await Resources.LoadAsync<Sprite>(address);
            return resourceRequest as Sprite;
        }

        public async UniTask<GameObject> GetObject(string address)
        {
            return await Resources.LoadAsync<GameObject>(address) as GameObject;
        }

        public async UniTask<GameObject> GetObjectCopy(string address)
        {
            var gameObject = await Resources.LoadAsync<GameObject>(address) as GameObject;
            return Object.Instantiate(gameObject);
        }

        public void RealizeObject(string address)
        {
            Resources.UnloadUnusedAssets();
            Debug.LogWarning($"{nameof(ResourceManager)} {nameof(RealizeObject)} called with {address}");
        }

        public void DestroyAndRealizeObject(string address, GameObject gameObject)
        {
            Object.Destroy(gameObject);
            RealizeObject(address);
        }
    }
}