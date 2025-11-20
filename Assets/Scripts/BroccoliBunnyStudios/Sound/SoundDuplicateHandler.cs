using System.Collections.Generic;
using UnityEngine;

namespace BroccoliBunnyStudios.Sound
{
    [DisallowMultipleComponent]
    public class SoundDuplicateHandler : MonoBehaviour
    {
        private const float Gap = 2f / 30f; // only play the same sound every after 2 frames
        private readonly Dictionary<string, float> _playingSounds = new();

        public bool CanPlaySound(string soundName)
        {
            if (this._playingSounds.TryGetValue(soundName, out var lastPlayed))
            {
                return Time.realtimeSinceStartup - lastPlayed > Gap;
            }

            return true;
        }

        public void PlayingSound(string soundName)
        {
            this._playingSounds[soundName] = Time.realtimeSinceStartup;
        }
    }
}