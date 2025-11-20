using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BroccoliBunnyStudios.Sound
{
    /// <summary>
    /// Simple playback info, single file only
    /// </summary>
    [CreateAssetMenu(menuName = "Sound/AudioPlaybackInfo/SimpleAudioPlaybackInfo", fileName = "AudioPlaybackInfo")]
    public class SimpleAudioPlaybackInfo : AudioPlaybackInfo
    {
        [field: SerializeField] private AudioClip AudioClip { get; set; } = null;

        public override UniTask<AudioClip> GetAudioClip()
        {
            return new UniTask<AudioClip>(this.AudioClip);
        }

        public void SetAudioClip(AudioClip audioClip)
        {
            this.AudioClip = audioClip;
        }
    }
}