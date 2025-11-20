using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BroccoliBunnyStudios.Sound
{
    public abstract class BaseSoundManager
    {
        protected AudioSourceContainerPoolManager AudioPoolManager { get; } = new();
        protected AudioSource AudioSource { get; private set; }
        protected AudioPlaybackInfoManager AudioPlaybackInfoManager { get; } = new();
        protected AudioMixerManager AudioMixerManager { get; } = new();

        public abstract UniTask<AudioSourceContainer> PlayAudioAsync(
            string soundPath,
            bool isSfx,
            bool isLooping,
            AnimationCurve curve,
            Vector3 pos);

        protected void Init(GameObject obj, int initialSfxCount)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            this.AudioSource = obj.AddComponent<AudioSource>();
            this.AudioSource.loop = true;
            this.AudioPoolManager.Init(this, obj.transform, initialSfxCount);
        }

        protected void SetBgmVolume(float volume)
        {
            this.AudioMixerManager.SetVolumeMusic(volume);
        }

        protected void SetSfxVolume(float volume)
        {
            this.AudioMixerManager.SetVolumeFx(volume);
        }
    }
}