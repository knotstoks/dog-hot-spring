using System;
using BroccoliBunnyStudios.Extensions;
using BroccoliBunnyStudios.Managers;
using BroccoliBunnyStudios.Panel;
using BroccoliBunnyStudios.Sound;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectRuntime.UI.Panels
{
    public class PnlYesNoPrompt : BasePanel
    {
        [field: SerializeField, Header("Scene References")]
        private Animator PanelAnimator { get; set; }

        [field: SerializeField]
        private TextMeshProUGUI PromptTMP { get; set; }

        [field: SerializeField]
        private Button YesButton { get; set; }

        [field: SerializeField]
        private TextMeshProUGUI YesTMP { get; set; }

        [field: SerializeField]
        private Button NoButton { get; set; }

        [field: SerializeField]
        private TextMeshProUGUI NoTMP { get; set; }

        [field: SerializeField, Header("Sfxes")]
        private AudioPlaybackInfo ButtonClickSfx { get; set; }

        // Internal Variables
        private Action _yesCallback;
        private Action _noCallback;
        private string _promptText;
        private string _yesText;
        private string _noText;
        
        private const string PANEL_OUT_ANIMATION = "panel_out";
        private bool _willFadeToBlackOnYesClick;
        private bool _isTransitioning;

        private void Awake()
        {
            LocalizationManager.Instance.OnLocalizationChanged += this.OnLocalizationChanged;
        }

        private void OnDestroy()
        {
            LocalizationManager.Instance.OnLocalizationChanged -= this.OnLocalizationChanged;
        }

        public void Init(string promptLocString, Action yesCallback, Action noCallback, bool willFadeToBlackOnYesClick = false, string yesText = "LOC_YES", string noText = "LOC_NO")
        {
            this._yesCallback = yesCallback;
            this._noCallback = noCallback;
            this._willFadeToBlackOnYesClick = willFadeToBlackOnYesClick;
            this._promptText = promptLocString;
            this._yesText = yesText;
            this._noText = noText;

            this.OnLocalizationChanged();

            this.YesButton.OnClick(() => this.OnYesButtonClick().Forget());
            this.NoButton.OnClick(() => this.OnNoButtonClick().Forget());
        }

        private async UniTaskVoid OnYesButtonClick()
        {
            if (this._isTransitioning)
            {
                return;
            }
            this._isTransitioning = true;
            
            SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.ButtonClickSfx, false, Vector3.zero).Forget();

            if (this._willFadeToBlackOnYesClick)
            {
                await PanelManager.Instance.FadeToBlackAsync();
                if (!this) return;

                PanelManager.Instance.SwitchCanvasToCamera();
            }

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

            this._yesCallback?.Invoke();
            this.Close();
        }

        private async UniTaskVoid OnNoButtonClick()
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

            this._noCallback?.Invoke();
            this.Close();
        }

        private void OnLocalizationChanged()
        {
            var lm = LocalizationManager.Instance;
            this.PromptTMP.text = lm[this._promptText];
            this.YesTMP.text = lm[this._yesText];
            this.NoTMP.text = lm[this._noText];
        }
    }
}