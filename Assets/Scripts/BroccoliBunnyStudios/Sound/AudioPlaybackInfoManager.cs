using System.Collections.Generic;
using BroccoliBunnyStudios.Pools;
using Cysharp.Threading.Tasks;

namespace BroccoliBunnyStudios.Sound
{
    /// <summary>
    /// Loads audio clips and manages the references so that the clips are not reloaded again if an existing one is still
    /// referenced in the scene
    /// </summary>
    public class AudioPlaybackInfoManager
    {
        private readonly Dictionary<string, ManagedAudioPlaybackInfo> _managedAudioClips = new();

        public async UniTask<AudioPlaybackInfo> GetAudioClip(string assetPath)
        {
            if (this._managedAudioClips.TryGetValue(assetPath, out var managedAudioClip))
            {
                managedAudioClip.IncreaseRef();

                // Audio clip is still getting loaded, wait until it's loaded
                while (managedAudioClip.AudioPlaybackInfo == default)
                {
                    if (managedAudioClip.FailedToLoad)
                    {
                        this.DecreaseRef(assetPath);
                        return null;
                    }

                    await UniTask.Yield();
                }

                return managedAudioClip.AudioPlaybackInfo;
            }

            managedAudioClip = new ManagedAudioPlaybackInfo();
            this._managedAudioClips.Add(assetPath, managedAudioClip);
            managedAudioClip.IncreaseRef();
            managedAudioClip.AudioPlaybackInfo = await ResourceLoader.LoadAsync<AudioPlaybackInfo>(assetPath);
            if (!managedAudioClip.AudioPlaybackInfo)
            {
                this.DecreaseRef(assetPath);
            }

            return managedAudioClip.AudioPlaybackInfo;
        }

        public void DecreaseRef(string soundPath)
        {
            if (this._managedAudioClips.TryGetValue(soundPath, out var managedAudioClip))
            {
                managedAudioClip.DecreaseRef();
                this._managedAudioClips.Remove(soundPath);
            }
        }
    }
}