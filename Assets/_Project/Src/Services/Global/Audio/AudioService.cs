using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Audio;

namespace Services.Global.Audio
{
    public enum SourceType
    {
        Background,
        SFX,
        MainGun,
        InterSyren,
    }

    public class AudioService : IAudioService, IDisposable
    {
        private Dictionary<SourceType, AudioSource> _sources = new();
        private readonly ReactiveProperty<AudioMixer> _mixer;
        private readonly CompositeDisposable _compositeDisposable = new();

        public AudioService()
        {
            _mixer = new ReactiveProperty<AudioMixer>();
            _mixer.Subscribe(_ => { Debug.Log("new Value audio loaded"); }).AddTo(_compositeDisposable);
            _mixer.AddTo(_compositeDisposable);
        }


        public void InjectAudioMixer(AudioMixer mixer)
        {
            _mixer.Value = mixer;
        }

        public void PlaySyreneComing()
        {
            _sources[SourceType.InterSyren].Play();
        }

        public void PlayShot()
        {
            _sources[SourceType.MainGun].Play();
        }

        public void PlayReload()
        {
            Debug.LogError("AudioService::PlayReload-fix");
        }

        public void Dispose()
        {
            _compositeDisposable?.Dispose();
            Debug.Log("AudioService disposed");
        }

        public void Bind(GameObject audioSource)
        {
        }

        public void InjectSource(AudioSource source, SourceType sourceName)
        {
            _sources.Add(sourceName, source);
        }

        public void UnInjectSource(SourceType sourceName)
        {
            if (_sources.ContainsKey(sourceName))
                _sources.Remove(sourceName);
        }

        public void StopPlaying()
        {
            _mixer.Value.SetFloat(AudioResources.MainMixer.Exp_VolumeBackground, -80);
        }

        public void StartPlaying()
        {
            _mixer.Value.SetFloat(AudioResources.MainMixer.Exp_VolumeBackground, 0);
        }
    }
}