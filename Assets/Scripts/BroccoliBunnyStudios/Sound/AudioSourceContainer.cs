using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Audio;

namespace BroccoliBunnyStudios.Sound
{
    public class AudioSourceContainer : MonoBehaviour
    {
        private readonly float _fadeDuration = 1f;
        private float Volume { get; set; }
        private AudioSource _audioSource;
        private GameObject _go;
        private AudioSourceContainerPoolManager _audioSourceContainerPoolManager;
        private bool _isPlaying;
        private bool _isDespawned;
        private bool _paused;
        private Tweener _tweener;

        public event Action<AudioSourceContainer> OnDespawn;

        public SoundBusType SoundBus { get; private set; }
        public AnimationCurve VolumeCurve { get; set; }
        public AudioClip Clip { get; private set; }
        public bool IsPlaying => this._isPlaying;
        public bool IsPaused => this._paused;

        public bool DespawnWhenNotPlaying { get; set; } = true;
        public bool IgnoreTimeScale { get; private set; } = false;
        public bool PauseWhenTimeScaleZero { get; private set; } = false;

        private bool _inFocus = true;

        public static AudioSourceContainer Gen(AudioSourceContainerPoolManager audioSourceContainerPoolManager)
        {
            if (audioSourceContainerPoolManager == null)
            {
                throw new ArgumentNullException(nameof(audioSourceContainerPoolManager));
            }

            var go = new GameObject(nameof(AudioSourceContainer));
            var inst = go.AddComponent<AudioSourceContainer>();

            inst._audioSourceContainerPoolManager = audioSourceContainerPoolManager;
            inst._go = go;
            inst._go.transform.SetParent(audioSourceContainerPoolManager.Transform);
            inst._audioSource = inst._go.AddComponent<AudioSource>();
            inst._go.SetActive(value: false);

            return inst;
        }

        public void Play(AudioClip clip, Vector3 pos, bool isLooping, SoundBusType soundBus, bool ignoreTimeScale, bool pauseWhenTimeScaleZero)
        {
            Assert.IsNotNull(clip, $"[fail] clip is null");
            this.Clip = clip;
            this.IgnoreTimeScale = ignoreTimeScale;
            this.PauseWhenTimeScaleZero = pauseWhenTimeScaleZero;
            this._isPlaying = true;
            this._isDespawned = false;
            this._paused = false;
            this._go.SetActive(value: true);
#if UNITY_EDITOR
            this._go.name = clip.name;
#endif
            this._go.transform.position = pos;
            this.Volume = 1f;
            this.SoundBus = soundBus;
            if (this.VolumeCurve != null)
            {
                var val = this.VolumeCurve.Evaluate(time: 0);
                this._audioSource.volume = val;
            }
            else
            {
                this._audioSource.volume = this.Volume;
            }

            if (SoundBusType.Bgm.HasFlag(soundBus))
            {
                this.FadeIn();
            }

            this._audioSource.clip = clip;
            this._audioSource.loop = isLooping;
            this._audioSource.Play();
        }

        public void SetVolume(float volume)
        {
            this.Volume = volume;
            this.UpdateVolume();
        }

        private void Stop()
        {
            if (this._audioSource)
            {
                this._audioSource.Pause();
            }

            if (this._go)
            {
                this._go.SetActive(false);
            }

            this._isPlaying = false;
        }

        public void Pause()
        {
            if (this._audioSource)
            {
                this._paused = true;
                this._audioSource.Pause();
            }
        }

        public void Unpause()
        {
            if (this._audioSource)
            {
                this._paused = false;
                this._audioSource.UnPause();
            }
        }

        public void Despawn()
        {
            if (this._isDespawned)
            {
                return;
            }

            this._isDespawned = true;
            this.VolumeCurve = null;
            this.Stop();
            this._audioSourceContainerPoolManager?.Despawn(this);
            this.OnDespawn?.Invoke(this);
        }

        public void SetAudioMixerGroup(AudioMixerGroup audioMixerGroup)
        {
            if (this._audioSource)
            {
                this._audioSource.outputAudioMixerGroup = audioMixerGroup;
            }
        }

        public async UniTask FadeOutAndStopAsync()
        {
            this.FadeVolume(0f);
            await UniTask.Delay(TimeSpan.FromSeconds(this._fadeDuration));
            this.Despawn();
        }

        private void FadeIn()
        {
            this.FadeVolume(1f);
        }

        private void FadeVolume(float endVolume)
        {
            if (this._tweener != null)
            {
                DOTween.Kill(this._tweener);
            }

            this.Volume = 0f;
            this._tweener = DOTween
                .To(() => this.Volume, x => this.Volume = x, endVolume, this._fadeDuration)
                .SetUpdate(this.IgnoreTimeScale);
        }

        private void Update()
        {
            if (!this._isPlaying)
            {
                return;
            }

            if (this._audioSource.isPlaying)
            {
                if (this.PauseWhenTimeScaleZero && !this._paused && Mathf.Approximately(Time.timeScale, 0f))
                {
                    this.Pause();
                }

                this.UpdateVolume();
            }
            else if (!this._audioSource.isPlaying)
            {
                if (this.PauseWhenTimeScaleZero && this._paused && !Mathf.Approximately(Time.timeScale, 0f))
                {
                    this.Unpause();
                    this.UpdateVolume();
                }
            }

            if (!this._audioSource.isPlaying && !this._paused && this.DespawnWhenNotPlaying && this._inFocus)
            {
                this.Despawn();
            }
        }

        private void UpdateVolume()
        {
            if (this.VolumeCurve != null)
            {
                var curNormalizedTime = this._audioSource.time / this.Clip.length;
                var val = this.VolumeCurve.Evaluate(curNormalizedTime);
                this._audioSource.volume = this.Volume * val;
            }
            else
            {
                this._audioSource.volume = this.Volume;
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            this._inFocus = hasFocus;
        }

        internal void Destroy()
        {
            this.Stop();
            this._audioSourceContainerPoolManager.CleanUp(this);
            this._audioSourceContainerPoolManager = null;
            Destroy(this._go);
        }
    }
}
