using Reflex.Attributes;
using UnityEngine;

namespace Services.Global.Audio
{
    public class AudioSourceBinder : MonoBehaviour
    {
        [SerializeField] private SourceType audioSourceName;
        [SerializeField] private AudioSource source;

        [Inject]
        public void Inject(IAudioService audioService)
        {
            audioService.InjectSource(source, audioSourceName);
        }
    }
}