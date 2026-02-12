using System;
using BroccoliBunnyStudios.Extensions;
using BroccoliBunnyStudios.Managers;
using BroccoliBunnyStudios.Panel;
using BroccoliBunnyStudios.Sound;
using Cysharp.Threading.Tasks;
using ProjectRuntime.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectRuntime.UI.Panels
{
    public class PnlSettings : BasePanel
    {
        [field: SerializeField, Header("Scene References")]
        private Animator PanelAnimator { get; set; }

        [field: SerializeField]
        private Slider MusicSlider { get; set; }

        [field: SerializeField]
        private Slider SfxSlider { get; set; }

        [field: SerializeField]
        private Button LocalizationButton { get; set; }

        [field: SerializeField]
        private Button BackToMainMenuButton { get; set; }

        [field: SerializeField]
        private Button CloseButton { get; set; }

        [field: SerializeField, Header("Localization")]
        private GameObject LocalizationPanelContainer { get; set; }

        [field: SerializeField]
        private Animator LocalizationPanelAnimator { get; set; }

        [field: SerializeField]
        private Button LocalizationPanelCloseButton { get; set; }

        [field: SerializeField]
        private RectTransform LanguageSelectRT { get; set; }

        [field: SerializeField]
        private UIChooseLanguage UIChooseLanguagePrefab { get; set; }

        [field: SerializeField, Header("Sfxes")]
        private AudioPlaybackInfo ButtonClickSfx { get; set; }

        // Internal Variables
        private const string PANEL_IN_ANIMATION = "panel_in";
        private const string PANEL_OUT_ANIMATION = "panel_out";
        private const string LOC_RETURN_HOME = "LOC_RETURN_HOME";
        private const string LOC_RETURN_MAINMENU = "LOC_RETURN_MAINMENU";
        private bool _isTransitioning;

        private void Awake()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "ScMain")
            {
                // Disable the go back to main menu button if on ScMain
                this.BackToMainMenuButton.gameObject.SetActive(false);
            }

            var sm = SaveManager.Instance;
            this.MusicSlider.value = sm.MusicVolume;
            this.SfxSlider.value = sm.SfxVolume;

            this.MusicSlider.onValueChanged.AddListener(this.OnMusicSliderValueChange);
            this.SfxSlider.onValueChanged.AddListener(this.OnSfxSliderValueChange);
            this.LocalizationButton.OnClick(() => this.OnLocalizationButtonClick().Forget());
            this.BackToMainMenuButton.OnClick(this.OnBackToMainMenuButtonClick);
            this.CloseButton.OnClick(() => this.OnCloseButtonClick().Forget());
            this.LocalizationPanelCloseButton.OnClick(() => this.OnLocalizationPanelCloseButtonClick().Forget());

            this.InitChangeLanguagePanel();
        }

        private void InitChangeLanguagePanel()
        {
            foreach (EnumLanguage enumLanguage in Enum.GetValues(typeof(EnumLanguage)))
            {
                var uiChooseLanguage = Instantiate(this.UIChooseLanguagePrefab, LanguageSelectRT);
                uiChooseLanguage.Init(enumLanguage);
            }
        }

        private void OnMusicSliderValueChange(float value)
        {
            if (this._isTransitioning)
            {
                return;
            }

            SaveManager.Instance.MusicVolume = value;
            SoundManager.Instance.SetMusicVolume(value);
        }

        private void OnSfxSliderValueChange(float value)
        {
            if (this._isTransitioning)
            {
                return;
            }

            SaveManager.Instance.SfxVolume = value;
            SoundManager.Instance.SetSoundVolume(value);
        }

        private async UniTaskVoid OnLocalizationButtonClick()
        {
            if (this._isTransitioning)
            {
                return;
            }
            this._isTransitioning = true;
            SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.ButtonClickSfx, false, Vector3.zero).Forget();

            this.LocalizationPanelContainer.SetActive(true);
            var stateInfo = this.LocalizationPanelAnimator.GetCurrentAnimatorStateInfo(0);
            while (!stateInfo.IsName(PANEL_IN_ANIMATION))
            {
                await UniTask.Yield();
                if (!this) return;

                stateInfo = this.LocalizationPanelAnimator.GetCurrentAnimatorStateInfo(0);
            }
            while (stateInfo.IsName(PANEL_IN_ANIMATION) && stateInfo.normalizedTime < 1f)
            {
                await UniTask.Yield();
                if (!this) return;

                stateInfo = this.LocalizationPanelAnimator.GetCurrentAnimatorStateInfo(0);
            }

            this._isTransitioning = false;
        }

        private void OnBackToMainMenuButtonClick()
        {
            if (this._isTransitioning)
            {
                return;
            }
            this._isTransitioning = true;
            SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.ButtonClickSfx, false, Vector3.zero).Forget();

            // Return to level select
            if (BattleManager.Instance != null)
            {
                PanelManager.Instance.ShowAsync<PnlYesNoPrompt>((pnl) =>
                {
                    pnl.Init(LOC_RETURN_HOME, this.GoToLevelSelect, null, true);
                }).Forget();
            }
            // Return to ScMain
            else
            {
                PanelManager.Instance.ShowAsync<PnlYesNoPrompt>((pnl) =>
                {
                    pnl.Init(LOC_RETURN_MAINMENU, this.GoToMainMenu, null, true);
                }).Forget();
            }


            this._isTransitioning = false;
        }

        private void GoToLevelSelect()
        {
            SceneManager.Instance.LoadSceneAsync("ScHome").Forget();

            this.Close();
        }

        private void GoToMainMenu()
        {
            SceneManager.Instance.LoadSceneAsync("ScMain").Forget();

            this.Close();
        }

        private async UniTaskVoid OnLocalizationPanelCloseButtonClick()
        {
            if (this._isTransitioning)
            {
                return;
            }
            this._isTransitioning = true;
            SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.ButtonClickSfx, false, Vector3.zero).Forget();

            this.LocalizationPanelAnimator.Play(PANEL_OUT_ANIMATION);
            var stateInfo = this.LocalizationPanelAnimator.GetCurrentAnimatorStateInfo(0);
            while (!stateInfo.IsName(PANEL_OUT_ANIMATION))
            {
                await UniTask.Yield();
                if (!this) return;

                stateInfo = this.LocalizationPanelAnimator.GetCurrentAnimatorStateInfo(0);
            }
            while (stateInfo.IsName(PANEL_OUT_ANIMATION) && stateInfo.normalizedTime < 1f)
            {
                await UniTask.Yield();
                if (!this) return;

                stateInfo = this.LocalizationPanelAnimator.GetCurrentAnimatorStateInfo(0);
            }

            this.LocalizationPanelContainer.SetActive(false);

            this._isTransitioning = false;
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
            while (stateInfo.normalizedTime < 1f)
            {
                await UniTask.Yield();
                if (!this) return;

                stateInfo = this.PanelAnimator.GetCurrentAnimatorStateInfo(0);
            }

            this.Close();
        }
    }
}