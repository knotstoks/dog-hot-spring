using BroccoliBunnyStudios.Pools;

namespace BroccoliBunnyStudios.Sound
{
    public class ManagedAudioPlaybackInfo
    {
        public AudioPlaybackInfo AudioPlaybackInfo { get; set; }
        public bool FailedToLoad { get; set; }
        public int ReferenceCount { get; private set; }

        public void IncreaseRef()
        {
            this.ReferenceCount++;
        }

        public void DecreaseRef()
        {
            this.ReferenceCount--;
            // unload the audio clip if no more references
            if (this.ReferenceCount <= 0 && this.AudioPlaybackInfo)
            {
                ResourceLoader.Unload(this.AudioPlaybackInfo);
            }
        }
    }
}