using System;
using Reflex.Attributes;
using UnityEngine;

namespace Services.Global.Audio
{
    public class AudioSourceBinder : MonoBehaviour
    {
        [SerializeField] private SourceType audioSourceName;
        [SerializeField] private AudioSource source;

        private IAudioService _audioService;

        [Inject]
        public void Inject(IAudioService audioService)
        {
            _audioService = audioService;
            _audioService.InjectSource(source, audioSourceName);
        }

        private void OnDestroy()
        {
            _audioService?.UnInjectSource(audioSourceName);
        }
    }
}