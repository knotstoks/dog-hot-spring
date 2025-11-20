using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BroccoliBunnyStudios.Sound
{
    public class AudioSourceContainerPoolManager
    {
        private readonly Queue<AudioSourceContainer> _inactive = new();
        private readonly HashSet<AudioSourceContainer> _active = new();
        private int _initialSfxCount;
        private AudioMixerManager _audioMixerManager;

        public Transform Transform { get; private set; }

        public void Init(AudioMixerManager audioMixerManager)
        {
            this._audioMixerManager = audioMixerManager;
        }

        public void CleanUp(AudioSourceContainer audioSourceContainer)
        {
            this._active.Remove(audioSourceContainer);
        }

        public AudioSourceContainer Spawn(AudioClip clip, Vector3 pos, bool isLooping, SoundBusType soundBusType, AnimationCurve curve = null, bool ignoreTimeScale = false, bool pauseWhenTimeScaleZero = false)
        {
            // handle bgm
            if (SoundBusType.Bgm.HasFlag(soundBusType))
            {
                // check if another container is playing the same clip
                var sameAudio = this._active.FirstOrDefault(x => x.Clip.name == clip.name);
                if (sameAudio != null)
                {
                    if (sameAudio.IsPlaying)
                    {
                        return sameAudio;
                    }

                    sameAudio.Play(clip, pos, isLooping, soundBusType, ignoreTimeScale, pauseWhenTimeScaleZero);
                    return sameAudio;
                }

                var otherBgm = this._active.Where(x => x.SoundBus == soundBusType);
                foreach (var bgm in otherBgm)
                {
                    bgm.FadeOutAndStopAsync().Forget();
                }
            }

            var audioSourceContainer = this._inactive.Count > 0 ? this._inactive.Dequeue() : AudioSourceContainer.Gen(this);
            if (curve != null)
            {
                audioSourceContainer.VolumeCurve = curve;
            }
             
            audioSourceContainer.SetAudioMixerGroup(this._audioMixerManager.GetMixerGroup(soundBusType));
            audioSourceContainer.DespawnWhenNotPlaying = true;

            audioSourceContainer.Play(clip, pos, isLooping, soundBusType, ignoreTimeScale, pauseWhenTimeScaleZero);
            this._active.Add(audioSourceContainer);

            return audioSourceContainer;
        }

        public void Despawn(AudioSourceContainer inst)
        {
            if (this._active.Remove(inst))
            {
                if (this._inactive.Count < this._initialSfxCount)
                {
                    this._inactive.Enqueue(inst);
                    return;
                }

                // resize inactive queue
                var diff = this._inactive.Count - this._initialSfxCount;
                for (var i = 0; i < diff; ++i)
                {
                    this._inactive.Dequeue().Destroy();
                }
            }

            inst.Destroy();
        }

        public void StopAllBgm()
        {
            foreach (var bgm in this._active.Where(x => SoundBusType.Bgm.HasFlag(x.SoundBus)))
            {
                bgm.FadeOutAndStopAsync().Forget();
            }
        }

        internal void Init(
            BaseSoundManager soundManager,
            Transform transform,
            int initialSfxCount)
        {
            this.Transform = transform;
            for (var i = 0; i < initialSfxCount; ++i)
            {
                this._inactive.Enqueue(AudioSourceContainer.Gen(this));
            }

            this._initialSfxCount = initialSfxCount;
        }
    }
}