using UnityEngine;
using UnityEngine.Audio;

namespace Services.Global.Audio
{
    public interface IAudioService
    {
        void Bind(GameObject audioSource);

        void StopPlaying();
        void StartPlaying();
        void InjectAudioMixer(AudioMixer mixer);
    }
}