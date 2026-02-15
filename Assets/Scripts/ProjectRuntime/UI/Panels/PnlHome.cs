using System.Collections.Generic;
using BroccoliBunnyStudios.Extensions;
using BroccoliBunnyStudios.Managers;
using BroccoliBunnyStudios.Panel;
using BroccoliBunnyStudios.Sound;
using Cinemachine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ProjectRuntime.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectRuntime.UI.Panels
{
    public class PnlHome : MonoBehaviour
    {
        public static PnlHome Instance { get; private set; }

        // This is 0-indexed
        public static int AreaToTransition { get; set; } = -1;

        [field: SerializeField, Header("Scene References")]
        private GameObject PanelParent { get; set; }

        [field: SerializeField]
        private CanvasGroup CanvasGroup { get; set; }

        [field: SerializeField]
        private CinemachineVirtualCamera DollyCamera { get; set; }

        [field: SerializeField]
        private Button SettingsButton { get; set; }

        [field: SerializeField]
        private GameObject LevelSelectButtonParent { get; set; }

        [field: SerializeField]
        private List<Button> LevelSelectButtons { get; set; }

        [field: SerializeField]
        private List<GameObject> LockIcons { get; set; }

        [field: SerializeField]
        private List<TextMeshProUGUI> LevelSelectTMPs { get; set; }

        [field: SerializeField]
        private Button PreviousAreaButton { get; set; }

        [field: SerializeField]
        private Button NextAreaButton { get; set; }

        [field: SerializeField]
        private Button CinematicButton { get; set; }

        [field: SerializeField, Header("Button Sprites")]
        private Sprite ActiveButtonSprite { get; set; }

        [field: SerializeField]
        private Sprite InactiveButtonSprite { get; set; }

        [field: SerializeField, Header("Sfxes")]
        private AudioPlaybackInfo ButtonClickSfx { get; set; }

        // Internal Variables
        private int _numberOfWorlds; // Set only once in Init
        private int _numberOfAreas; //Set only once in Init
        private bool _isTransitioningScene;
        private int _currentAreaIdx;

        private const float DOLLY_DURATION = 1f;
        private const float BUTTON_FADE_DURATION = 0.2f;

        private void Awake()
        {
            PanelManager.Instance.FadeToBlackAsync(0f).Forget();

            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError("There are 2 or more PnlHomes in the scene");
            }

            this._numberOfWorlds = DWorld.GetAllData().Data.Count;
            this._numberOfAreas = this._numberOfWorlds / 10;
            this.SettingsButton.OnClick(() => this.OnSettingsButtonClick().Forget());
            for (var i = 0; i < this.LevelSelectButtons.Count; i++)
            {
                var temp = i + 1; // Neccessary to create temp variable for the closure function
                this.LevelSelectButtons[i].OnClick(() => this.OnLevelSelectButtonClick(temp));
            }
            this.PreviousAreaButton.OnClick(this.OnPreviousAreaButtonClick);
            this.NextAreaButton.OnClick(this.OnNextAreaButtonClick);
            this.CinematicButton.OnClick(this.OnCinematicButtonClick);
        }

        private void Start()
        {
            this.Init();
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        private async void Init()
        {
            await this.ToggleAllButtonsShow(false);
            if (!this) return;
            var currentLevelProgress = UserSaveDataManager.Instance.GetCurrentWorldProgress();
            Debug.Log($"Loading current save world level: {currentLevelProgress}");
            this._currentAreaIdx = Mathf.Min(Mathf.Max(currentLevelProgress / 10, 0), this._numberOfAreas - 1); // 0-indexed
            Debug.Log($"Loading area index: {this._currentAreaIdx}");

            if (AreaToTransition == -1)
            {
                await this.RefreshUI(this._currentAreaIdx);
                if (!this) return;

                await PanelManager.Instance.FadeFromBlack();
                if (!this) return;
            }
            else
            {
                await this.RefreshUI(AreaToTransition - 1);
                if (!this) return;

                await this.ToggleAllButtonsShow(false);
                if (!this) return;

                await PanelManager.Instance.FadeFromBlack();
                if (!this) return;

                await this.RefreshUI(AreaToTransition);
                if (!this) return;

                AreaToTransition = -1;
            }
        }

        /// <summary>
        /// This function moves the dolly over to whatever area page the ui is
        /// </summary>
        private async UniTask RefreshUI(int areaPageIdx)
        {
            await this.ToggleAllButtonsShow(false);
            if (!this) return;
            this._currentAreaIdx = areaPageIdx;

            // Change the level TMPs on the buttons
            var firstLevelNumber = this._currentAreaIdx * 10 + 1;
            for (var i = 0; i < this.LevelSelectTMPs.Count; i++)
            {
                this.LevelSelectTMPs[i].text = (firstLevelNumber + i).ToString();
            }

            // Move the camera using dolly
            var initialValue = this.DollyCamera.GetCinemachineComponent<CinemachineTrackedDolly>().m_PathPosition;
            var finalValue = (float)this._currentAreaIdx / (this._numberOfWorlds / 10 - 1);
            var elapsed = 0f;
            while (elapsed < DOLLY_DURATION)
            {
                elapsed += Time.deltaTime;

                this.DollyCamera.GetCinemachineComponent<CinemachineTrackedDolly>().m_PathPosition = Mathf.Lerp(initialValue, finalValue, elapsed / DOLLY_DURATION);

                await UniTask.Yield();
                if (!this) return;
            }

            await this.ToggleAllButtonsShow(true);
            if (!this) return;

            // Provide 1 frame buffer for Init
            await UniTask.Yield();
            if (!this) return;
        }

        public async UniTask ToggleAllButtonsShow(bool toggle)
        {
            if (!toggle)
            {
                await this.CanvasGroup.DOFade(0f, BUTTON_FADE_DURATION);
                return;
            }

            this.SettingsButton.gameObject.SetActive(true);
            this.CinematicButton.gameObject.SetActive(true);
            // TODO: Make signifier to click cinematic if area is locked because haven't seen cinematic

            var usdm = UserSaveDataManager.Instance;
            var currentWorldProgress = usdm.GetCurrentWorldProgress();
            var firstLevelShown = this._currentAreaIdx * 10 + 1;
            for (var i = 0; i < 10; i++)
            {
                var isActive = usdm.HasSeenStory($"STORY_{this._currentAreaIdx + 1}")
                    && currentWorldProgress >= firstLevelShown + i - 1;
                this.LevelSelectButtons[i].image.sprite = isActive
                    ? this.ActiveButtonSprite
                    : this.InactiveButtonSprite;
                this.LevelSelectButtons[i].interactable = isActive;
                this.LockIcons[i].SetActive(!isActive);
            }
            this.LevelSelectButtonParent.SetActive(true);
            
            var maxAreaIdx = this._numberOfWorlds / 10 - 1;
            this.PreviousAreaButton.gameObject.SetActive(this._currentAreaIdx != 0);
            this.NextAreaButton.gameObject.SetActive(this._currentAreaIdx < maxAreaIdx
                && currentWorldProgress / 10 > this._currentAreaIdx);

            await this.CanvasGroup.DOFade(1f, BUTTON_FADE_DURATION);
            if (!this) return;
        }

        #region Button Click
        public async void OnLevelSelectButtonClick(int level)
        {
            if (this._isTransitioningScene)
            {
                return;
            }
            this._isTransitioningScene = true;
            SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.ButtonClickSfx, false, Vector3.zero).Forget();

            BattleManager.LevelIdToLoad = this._currentAreaIdx * 10 + level;

            await PanelManager.Instance.FadeToBlackAsync();
            if (!this) return;

            SceneManager.Instance.LoadSceneAsync("ScGame").Forget();
        }

        private async void OnNextAreaButtonClick()
        {
            if (this._isTransitioningScene)
            {
                return;
            }
            this._isTransitioningScene = true;
            SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.ButtonClickSfx, false, Vector3.zero).Forget();

            await this.ToggleAllButtonsShow(false);
            if (!this) return;

            await this.RefreshUI(this._currentAreaIdx + 1);
            if (!this) return;

            await this.ToggleAllButtonsShow(true);
            if (!this) return;

            this._isTransitioningScene = false;
        }

        private async void OnPreviousAreaButtonClick()
        {
            if (this._isTransitioningScene)
            {
                return;
            }
            this._isTransitioningScene = true;
            SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.ButtonClickSfx, false, Vector3.zero).Forget();

            await this.ToggleAllButtonsShow(false);
            if (!this) return;

            await this.RefreshUI(this._currentAreaIdx - 1);
            if (!this) return;

            await this.ToggleAllButtonsShow(true);
            if (!this) return;

            this._isTransitioningScene = false;
        }

        private async UniTaskVoid OnSettingsButtonClick()
        {
            if (this._isTransitioningScene)
            {
                return;
            }
            this._isTransitioningScene = true;
            SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.ButtonClickSfx, false, Vector3.zero).Forget();

            await this.ToggleAllButtonsShow(false);
            if (!this) return;

            PanelManager.Instance.SwitchCanvasToOverlay();
            var pnlSettings = await PanelManager.Instance.ShowAsync<PnlSettings>();
            await pnlSettings.WaitWhilePanelIsAlive();
            if (!this) return;
            PanelManager.Instance.SwitchCanvasToCamera();

            await this.ToggleAllButtonsShow(true);
            if (!this) return;

            this._isTransitioningScene = false;
        }

        private async void OnCinematicButtonClick()
        {
            if (this._isTransitioningScene)
            {
                return;
            }
            this._isTransitioningScene = true;
            SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.ButtonClickSfx, false, Vector3.zero).Forget();

            // Change scene and load the correct cinematic
            PnlCinematic.StoryIdToLoad = $"STORY_{this._currentAreaIdx + 1}";

            await PanelManager.Instance.FadeToBlackAsync();
            if (!this) return;

            SceneManager.Instance.LoadSceneAsync("ScCinematic").Forget();
        }
        #endregion
    }
}