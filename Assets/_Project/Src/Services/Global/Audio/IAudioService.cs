using UnityEngine;
using UnityEngine.Audio;

namespace Services.Global.Audio
{
    public interface IAudioService
    {
        void Bind(GameObject audioSource);
        void InjectSource(AudioSource source, SourceType sourceName);
        void UnInjectSource(SourceType sourceName);
        void StopPlaying();
        void StartPlaying();
        void InjectAudioMixer(AudioMixer mixer);
        void PlayShot();
        void PlayReload();

        void PlaySyreneComing();
    }
}