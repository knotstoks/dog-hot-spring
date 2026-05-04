using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BroccoliBunnyStudios.Sound
{
    public class BgmTrigger : MonoBehaviour
    {
        [field: SerializeField]
        private string BgmToPlay { get; set; }

        private void Awake()
        {
            SoundManager.Instance.PlayBgm(this.BgmToPlay).Forget();
        }
    }
}