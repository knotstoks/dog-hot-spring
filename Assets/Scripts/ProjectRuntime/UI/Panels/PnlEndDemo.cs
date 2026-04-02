using BroccoliBunnyStudios.Managers;
using BroccoliBunnyStudios.Panel;
using BroccoliBunnyStudios.Sound;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectRuntime.UI.Panels
{
    public class PnlEndDemo : MonoBehaviour
    {
        [field: SerializeField, Header("Scene References")]
        private Button BackToMainMenuButton { get; set; }

        [field: SerializeField, Header("Sfxes")]
        private AudioPlaybackInfo ButtonClickSfx { get; set; }

        private bool _isTransitioning;

        private void Awake()
        {
            PanelManager.Instance.FadeToBlackAsync(0f).Forget();

            this.BackToMainMenuButton.onClick.AddListener(() => this.OnBackToMainMenuButtonClick().Forget());

            PanelManager.Instance.FadeFromBlack().Forget();
        }

        private async UniTaskVoid OnBackToMainMenuButtonClick()
        {
            if (this._isTransitioning)
            {
                return;
            }
            this._isTransitioning = true;

            SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.ButtonClickSfx, false, Vector3.zero).Forget();

            await PanelManager.Instance.FadeToBlackAsync();
            if (!this) return;

            SceneManager.Instance.LoadSceneAsync("ScMain").Forget();
        }
    }
}