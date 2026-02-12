using BroccoliBunnyStudios.Extensions;
using BroccoliBunnyStudios.Panel;
using BroccoliBunnyStudios.Sound;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectRuntime.UI.Panels
{
    public class PnlGame : MonoBehaviour
    {
        [field: SerializeField, Header("Scene References")]
        private Button SettingsButton { get; set; }

        [field: SerializeField, Header("Sfxes")]
        private AudioPlaybackInfo ButtonClickSfx { get; set; }

        private void Awake()
        {
            this.SettingsButton.OnClick(this.OnSettingsButtonClick);
        }

        private void OnSettingsButtonClick()
        {
            if (PanelManager.Instance.IsPanelOpen<PnlSettings>())
            {
                return;
            }

            SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.ButtonClickSfx, false, Vector3.zero).Forget();
            PanelManager.Instance.ShowAsync<PnlSettings>().Forget();
        }
    }
}