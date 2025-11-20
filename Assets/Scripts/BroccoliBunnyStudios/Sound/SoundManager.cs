using System;
using System.Collections.Generic;
using BroccoliBunnyStudios.Managers;
using BroccoliBunnyStudios.Pools;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

namespace BroccoliBunnyStudios.Sound
{
    public class SoundManager : BaseSoundManager
    {
        private static readonly Lazy<SoundManager> s_lazy = new(() => new SoundManager());
        private List<AudioSourceContainer> SfxList { get; } = new();
        private AudioSourceContainer _currentBgm;
        private string _currentBgmName = string.Empty;

        private SoundManager()
        {
            var soundManagerObject = new GameObject
            {
                name = nameof(SoundManager)
            };
            Object.DontDestroyOnLoad(soundManagerObject);
            this.Init(soundManagerObject, 5);
        }

        public static SoundManager Instance => s_lazy.Value;

        public SoundDuplicateHandler DuplicateHandler { get; set; }

        /// <summary>
        /// Used by resume, prevent sfx from getting played
        /// </summary>
        public bool DisableSFXTemporarily { get; set; }

        public new void Init(GameObject obj, int initialSfxCount)
        {
            this.DuplicateHandler = obj.AddComponent<SoundDuplicateHandler>();
            base.Init(obj, initialSfxCount);
            var mainMixer = ResourceLoader.Load<AudioMixer>("audio/MainMixer.mixer");
            this.AudioMixerManager.Initialize(mainMixer);
            this.AudioMixerManager.SetMixerGroupOutput(this.AudioSource, SoundBusType.Music1);
            this.AudioPoolManager.Init(this.AudioMixerManager);

            this.SetBgmVolume(SaveManager.Instance.MusicVolume);
            this.SetSoundVolume(SaveManager.Instance.SfxVolume);
        }

        public void StopBgmAsync()
        {
            this.AudioPoolManager.StopAllBgm();
            this._currentBgm = null;
            this._currentBgmName = string.Empty;
        }

        public async UniTask PlayBgm(string sound)
        {
            if (this._currentBgmName == sound)
            {
                return;
            }

            var path = $"bgm/{sound}.asset";
            var newBgm = await this.PlayAudioAsync(path, true);
            newBgm.DespawnWhenNotPlaying = false;
            if (this._currentBgm)
            {
                this._currentBgm.Despawn();
            }

            this._currentBgm = newBgm;
            this._currentBgmName = sound;
        }

        public async UniTask PlayBgm(SimpleAudioPlaybackInfo playbackInfo)
        {
            if (this._currentBgmName == playbackInfo.name)
            {
                return;
            }

            var newBgm = await this.PlayAudioPlaybackInfoAsync(playbackInfo, true, Vector3.zero);
            newBgm.DespawnWhenNotPlaying = false;
            if (this._currentBgm)
            {
                this._currentBgm.Despawn();
            }

            this._currentBgm = newBgm;
            this._currentBgmName = playbackInfo.name;
        }

        public async UniTask<AudioSourceContainer> PlaySfx(string sound, bool isLooping = false)
        {
            var path = $"sfx/{sound}.asset";
            return await this.PlayAudioAsync(path, isLooping);
        }

        public async UniTask<AudioSourceContainer> PlayAudioAsync(string sound, bool isLooping = false)
        {
            return await this.PlayAudioAsync(sound, true, isLooping, null, default);
        }

        public override async UniTask<AudioSourceContainer> PlayAudioAsync(
            string soundPath,
            bool isSfx,
            bool isLooping,
            AnimationCurve curve,
            Vector3 pos)
        {
            if (this.DisableSFXTemporarily)
            {
                return null;
            }

            if (string.IsNullOrEmpty(soundPath))
            {
                return null;
            }

            var playbackInfo = await this.AudioPlaybackInfoManager.GetAudioClip(soundPath);
            if (!playbackInfo)
            {
                Debug.LogError($"Failed to load {soundPath}");
                return null;
            }

            return await this.PlayAudioPlaybackInfoAsync(playbackInfo, isLooping, pos);
        }

        public async UniTask<AudioSourceContainer> PlayAudioPlaybackInfoAsync(
            AudioPlaybackInfo playbackInfo,
            bool isLooping,
            Vector3 pos,
            bool ignoreTimeScale = false)
        {
            if (this.DisableSFXTemporarily)
            {
                return null;
            }

            Assert.IsNotNull(playbackInfo);
            var clip = await playbackInfo.GetAudioClip();

            if (!(isLooping || playbackInfo.Looped)
                && this.DuplicateHandler
                && playbackInfo.PreventSimultaneousDuplicatePlayback
                && !this.DuplicateHandler.CanPlaySound(clip.name))
            {
                return null;
            }

            if (!isLooping && this.DuplicateHandler)
            {
                this.DuplicateHandler.PlayingSound(clip.name);
            }

            var fx = this.AudioPoolManager.Spawn(
                clip,
                pos,
                isLooping || playbackInfo.Looped,
                playbackInfo.SoundBusTypes,
                playbackInfo.AnimationCurve,
                ignoreTimeScale,
                playbackInfo.PauseWhenTimeScaleZero);

            if (SoundBusType.Sfx.HasFlag(playbackInfo.SoundBusTypes))
            {
                this.SfxList.Add(fx);
            }

            fx.OnDespawn += this.OnDespawn;
            return fx;
        }

        public async UniTask<AudioSourceContainer> PlayVoiceAsync(string voice, AnimationCurve curve = null)
        {
            if (string.IsNullOrEmpty(voice))
            {
                return null;
            }

            // TODO: Right now voice are just treated like normal sfx, need to add support later for localised voice
            return await this.PlayAudioAsync(voice, false, false, curve, default);
        }

        public void ClearEffects()
        {
            var list = new List<AudioSourceContainer>(this.SfxList);
            foreach (var sfx in list)
            {
                if (sfx)
                {
                    sfx.Despawn();
                }
            }

            this.SfxList.Clear();
        }

        private void OnDespawn(AudioSourceContainer fx)
        {
            this.AudioPlaybackInfoManager.DecreaseRef(fx.Clip.name);
            this.SfxList.Remove(fx);
            fx.OnDespawn -= this.OnDespawn;
        }

        public void SetMusicVolume(float volume)
        {
            this.SetBgmVolume(volume);
        }

        public void SetSoundVolume(float volume)
        {
            this.SetSfxVolume(volume);
        }
    }
}