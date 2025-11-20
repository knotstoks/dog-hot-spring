using System.Collections.Generic;
using BroccoliBunnyStudios.Pools;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace BroccoliBunnyStudios.Sound
{
    /// <summary>
    /// Playback info with support to playing random files
    /// </summary>
    [CreateAssetMenu(menuName = "Sound/AudioPlaybackInfo/RandomAudioPlaybackInfo", fileName = "RandomAudioPlaybackInfo")]
    public class RandomAudioPlaybackInfo : AudioPlaybackInfo
    {
        [field: SerializeField] private List<string> SoundFiles { get; set; } = new();

        public override async UniTask<AudioClip> GetAudioClip()
        {
            Assert.IsTrue(this.SoundFiles.Count != 0);
            if (this.SoundFiles.Count == 0)
            {
                Debug.LogError($"No Soundfiles provided. {nameof(RandomAudioPlaybackInfo)}:{this.name}");
            }

            var randomIndex = Random.Range(0, this.SoundFiles.Count);
            var audioClip = await ResourceLoader.LoadAsync<AudioClip>(this.SoundFiles[randomIndex]);
            return audioClip;
        }

        public void SetSoundFilesList(IEnumerable<string> soundFiles)
        {
            this.SoundFiles.Clear();
            this.SoundFiles.AddRange(soundFiles);
        }
    }
}