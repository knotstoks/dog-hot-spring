using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BroccoliBunnyStudios.Sound
{
    public abstract class AudioPlaybackInfo : ScriptableObject
    {
        [field: SerializeField] public AnimationCurve AnimationCurve { get; private set; } = new(new Keyframe(0, 1), new Keyframe(1, 1));
        [field: SerializeField] public SoundBusType SoundBusTypes { get; private set; }
        [field: SerializeField] public float Delay { get; private set; }
        [field: SerializeField] public bool Looped { get; private set; }
        [field: SerializeField] public bool PreventSimultaneousDuplicatePlayback { get; private set; } = true;
        [field: SerializeField] public bool PauseWhenTimeScaleZero { get; private set; }

        public abstract UniTask<AudioClip> GetAudioClip();

        public void SetSoundBusType(SoundBusType soundBusTypes)
        {
            this.SoundBusTypes = soundBusTypes;
        }
    }
}