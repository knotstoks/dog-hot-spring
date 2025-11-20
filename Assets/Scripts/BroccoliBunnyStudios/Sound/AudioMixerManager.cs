using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace BroccoliBunnyStudios.Sound
{
    /// <summary>
    /// Manages and exposes the controls for the audio mixers
    /// </summary>
    public class AudioMixerManager
    {
        private readonly Dictionary<string, AudioMixerGroup> _allMixerGroups = new();
        private readonly Dictionary<SoundBusType, AudioMixerGroup> _subTypeMixerGroups = new();

        private AudioMixer _master;

        public void Initialize(AudioMixer masterMixer)
        {
            this._master = masterMixer;
            var matchingGroups = this._master.FindMatchingGroups(string.Empty);
            this._allMixerGroups.Clear();
            foreach (var audioMixerGroup in matchingGroups)
            {
                this._allMixerGroups.Add(audioMixerGroup.name, audioMixerGroup);
            }

            foreach (SoundBusType subType in Enum.GetValues(typeof(SoundBusType)))
            {
                if (this._allMixerGroups.TryGetValue(subType.ToString(), out var audioMixerGroup))
                {
                    this._subTypeMixerGroups.Add(subType, audioMixerGroup);
                }
            }
        }

        public void SetVolumeFx(float volume)
        {
            this._master.SetFloat("VolumeFx", this.GetLogVolume(volume));
        }

        public void SetVolumeMusic(float volume)
        {
            this._master.SetFloat("VolumeMusic", this.GetLogVolume(volume));
        }

        public float GetVolumeFx()
        {
            return this._master.GetFloat("VolumeFx", out var volume) ? volume : 0f;
        }

        public float GetVolumeMusic()
        {
            return this._master.GetFloat("VolumeMusic", out var volume) ? volume : 0f;
        }

        public AudioMixerGroup GetMixerGroup(SoundBusType soundBusTypes)
        {
            return this._subTypeMixerGroups.TryGetValue(soundBusTypes, out var mixerGroup) ? mixerGroup : null;
        }

        public void SetMixerGroupOutput(AudioSource audioSource, SoundBusType soundBusTypes)
        {
            var mixerGroup = this.GetMixerGroup(soundBusTypes);
            if (mixerGroup == null)
            {
                Debug.LogWarning($"Missing mixer {soundBusTypes}");
                return;
            }

            if (audioSource == null)
            {
                Debug.LogWarning("AudioSource should not be null");
                return;
            }

            audioSource.outputAudioMixerGroup = mixerGroup;
        }

        private float GetLogVolume(float linearVolume)
        {
            return Mathf.Log(Mathf.Max(linearVolume, 0.001f)) * 20f;
        }
    }
}