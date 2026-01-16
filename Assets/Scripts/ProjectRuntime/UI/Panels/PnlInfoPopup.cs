using BroccoliBunnyStudios.Managers;
using BroccoliBunnyStudios.Panel;
using BroccoliBunnyStudios.Sound;
using BroccoliBunnyStudios.Utils;
using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectRuntime.UI.Panels
{
    public class PnlInfoPopup : BasePanel
    {
        [field: SerializeField, Header("Scene References")]
        private GameObject PanelParent { get; set; }

        [field: SerializeField]
        private Animator PanelAnimator { get; set; }

        [field: SerializeField]
        private Image InfoImage { get; set; }

        [field: SerializeField]
        private TextMeshProUGUI HeaderTMP { get; set; }

        [field: SerializeField]
        private TextMeshProUGUI PromptTMP { get; set; }

        [field: SerializeField]
        private Button OkayButton { get; set; }

        [field: SerializeField, Header("Sfxes")]
        private AudioPlaybackInfo ButtonClickSfx { get; set; }

        public static Action OnOkay; // For Tutorial

        // Internal Variables
        private const string PANEL_OUT_ANIMATION = "panel_out";
        private bool _isTransitioning;
        private string _headerText;
        private string _promptText;

        private void Awake()
        {
            LocalizationManager.Instance.OnLocalizationChanged += this.OnLocalizationChanged;

            this.OkayButton.onClick.AddListener(() => this.OnOkayButtonClick().Forget());
        }

        private void OnDestroy()
        {
            LocalizationManager.Instance.OnLocalizationChanged -= this.OnLocalizationChanged;
        }

        public void Init(string headerText, string promptText, string imagePath)
        {
            this._headerText = headerText;
            this._promptText = promptText;
            CommonUtil.UpdateSprite(this.InfoImage, imagePath);

            this.OnLocalizationChanged();
        }

        private async UniTaskVoid OnOkayButtonClick()
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
            while (stateInfo.normalizedTime < 1f)
            {
                await UniTask.Yield();
                if (!this) return;

                stateInfo = this.PanelAnimator.GetCurrentAnimatorStateInfo(0);
            }
            // For tutorial
            OnOkay?.Invoke();

            this.Close();
        }

        private void OnLocalizationChanged()
        {
            var lm = LocalizationManager.Instance;
            this.HeaderTMP.text = lm[this._headerText];
            this.PromptTMP.text = lm[this._promptText];
        }
    }
}