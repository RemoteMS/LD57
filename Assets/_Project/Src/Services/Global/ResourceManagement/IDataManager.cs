using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

namespace Services.Global.ResourceManagement
{
    public interface IDataManager
    {
        UniTask<GameObject> InstantiatePrefabAsync(string path, Vector3 position = default,
            Quaternion rotation = default, Transform parent = null);

        UniTask<T> LoadAssetAsync<T>(string path) where T : Object;
        UniTask<AudioMixer> GetAudioMixer(string address);
        UniTask<Sprite> GetSprite(string address);
        UniTask<GameObject> GetObject(string address);
        UniTask<GameObject> GetObjectCopy(string address);
        GameObject GetObjectCopyFast(string address);
        void RealizeObject(string address);
        void DestroyAndRealizeObject(string address, GameObject gameObject);
    }
}