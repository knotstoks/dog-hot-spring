using BroccoliBunnyStudios.Extensions;
using BroccoliBunnyStudios.Panel;
using BroccoliBunnyStudios.Sound;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectRuntime.UI.Panels
{
    public class PnlCredits : BasePanel
    {
        [field: SerializeField, Header("Scene References")]
        private Animator PanelAnimator { get; set; }

        [field: SerializeField]
        private Button CloseButton { get; set; }

        [field: SerializeField, Header("Sfxes")]
        private AudioPlaybackInfo ButtonClickSfx { get; set; }

        // Internal Variables        
        private const string PANEL_OUT_ANIMATION = "panel_out";
        private bool _isTransitioning;

        private void Awake()
        {
            this.CloseButton.OnClick(() => this.OnCloseButtonClick().Forget());
        }

        private async UniTaskVoid OnCloseButtonClick()
        {
            if (this._isTransitioning)
            {
                return;
            }
            this._isTransitioning = true;

            SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.ButtonClickSfx, false, Vector3.zero).Forget();

            this.PanelAnimator.Play(PANEL_OUT_ANIMATION);
            var stateInfo = this.PanelAnimator.GetCurrentAnimatorStateInfo(0);
            while (!stateInfo.IsName(PANEL_OUT_ANIMATION))
            {
                await UniTask.Yield();
                if (!this) return;

                stateInfo = this.PanelAnimator.GetCurrentAnimatorStateInfo(0);
            }

            while (stateInfo.IsName(PANEL_OUT_ANIMATION) && stateInfo.normalizedTime < 1f)
            {
                await UniTask.Yield();
                if (!this) return;

                stateInfo = this.PanelAnimator.GetCurrentAnimatorStateInfo(0);
            }

            this.Close();
        }
    }
}