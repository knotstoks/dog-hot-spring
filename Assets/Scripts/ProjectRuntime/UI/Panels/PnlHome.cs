using Cinemachine;
using System.Collections.Generic;
using BroccoliBunnyStudios.Extensions;
using BroccoliBunnyStudios.Managers;
using BroccoliBunnyStudios.Panel;
using Cysharp.Threading.Tasks;
using ProjectRuntime.Managers;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BroccoliBunnyStudios.Sound;

namespace ProjectRuntime.UI.Panels
{
    public class PnlHome : MonoBehaviour
    {
        public static PnlHome Instance { get; private set; }

        [field: SerializeField, Header("Scene References")]
        private CinemachineVirtualCamera DollyCamera { get; set; }

        [field: SerializeField]
        private Button SettingsButton { get; set; }

        [field: SerializeField]
        private GameObject LevelSelectButtonParent { get; set; }

        [field: SerializeField]
        private List<Button> LevelSelectButtons { get; set; }

        [field: SerializeField]
        private List<TextMeshProUGUI> LevelSelectTMPs { get; set; }

        [field: SerializeField]
        private Button PreviousAreaButton { get; set; }

        [field: SerializeField]
        private Button NextAreaButton { get; set; }

        [field: SerializeField]
        private Button CinematicButton { get; set; }

        [field: SerializeField, Header("Button Sprites")]
        private Sprite RedButtonSprite { get; set; }

        [field: SerializeField]
        private Sprite GrayButtonSprite { get; set; }

        [field: SerializeField]
        private AudioPlaybackInfo ButtonClickSfx { get; set; }

        // Internal Variables
        private int _numberOfWorlds; // Set only once in Init
        private int _numberOfAreas; //Set only once in Init
        private bool _isTransitioningScene;
        private int _currentAreaIdx;

        private const float DOLLY_DURATION = 1f;

        private void Awake()
        {
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
            this.SettingsButton.OnClick(this.OnSettingsButtonClick);
            for (var i = 0; i < this.LevelSelectButtons.Count; i++)
            {
                var temp = i + 1; // Neccessary to create temp variable for the closure function
                this.LevelSelectButtons[i].OnClick(() => this.OnLevelSelectButtonClick(temp));
            }
            this.PreviousAreaButton.OnClick(this.OnPreviousAreaButtonClick);
            this.NextAreaButton.OnClick(this.OnNextAreaButtonClick);
            this.CinematicButton.OnClick(this.OnCinematicButtonClick);

            UserSaveDataManager.Instance.RegisterStory("STORY_1");
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
            this.ToggleAllButtonsShow(false);
            var currentLevel = UserSaveDataManager.Instance.GetCurrentWorldProgress();
            Debug.Log($"Loading current save world level: {currentLevel}");
            this._currentAreaIdx = Mathf.Min(Mathf.Max(currentLevel / 10, 0), this._numberOfAreas - 1); // 0-indexed
            await this.RefreshUI(this._currentAreaIdx);
            if (!this) return;

            await PanelManager.Instance.FadeFromBlack();
            if (!this) return;
        }

        /// <summary>
        /// This function moves the dolly over to whatever area page the ui is
        /// </summary>
        private async UniTask RefreshUI(int areaPageIdx)
        {
            this.ToggleAllButtonsShow(false);
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

            this.ToggleAllButtonsShow(true);

            // Provide 1 frame buffer for Init
            await UniTask.Yield();
            if (!this) return;
        }

        public void ToggleAllButtonsShow(bool toggle)
        {
            this.SettingsButton.gameObject.SetActive(toggle);

            this.CinematicButton.gameObject.SetActive(toggle);
            // TODO: Make signifier to click cinematic if area is locked because haven't seen cinematic

            var currentWorldProgress = UserSaveDataManager.Instance.GetCurrentWorldProgress();
            var firstLevelShown = this._currentAreaIdx * 10 + 1;
            for (var i = 0; i < 10; i++)
            {
                this.LevelSelectButtons[i].interactable = UserSaveDataManager.Instance.HasSeenStory($"STORY_{this._currentAreaIdx + 1}")
                    && currentWorldProgress >= firstLevelShown + i - 1;
            }
            this.LevelSelectButtonParent.SetActive(toggle);
            
            var maxAreaIdx = currentWorldProgress / 10;
            this.PreviousAreaButton.gameObject.SetActive(toggle && this._currentAreaIdx != 0);
            this.NextAreaButton.gameObject.SetActive(toggle && this._currentAreaIdx < maxAreaIdx);            
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

            this.ToggleAllButtonsShow(false);

            await this.RefreshUI(this._currentAreaIdx + 1);
            if (!this) return;

            this._isTransitioningScene = false;
            this.ToggleAllButtonsShow(true);
        }

        private async void OnPreviousAreaButtonClick()
        {
            if (this._isTransitioningScene)
            {
                return;
            }
            this._isTransitioningScene = true;

            SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.ButtonClickSfx, false, Vector3.zero).Forget();

            this.ToggleAllButtonsShow(false);

            await this.RefreshUI(this._currentAreaIdx - 1);
            if (!this) return;

            this._isTransitioningScene = false;
            this.ToggleAllButtonsShow(true);
        }

        private void OnSettingsButtonClick()
        {
            if (this._isTransitioningScene)
            {
                return;
            }

            SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.ButtonClickSfx, false, Vector3.zero).Forget();

            // TODO
        }

        private async void OnCinematicButtonClick()
        {
            if (this._isTransitioningScene)
            {
                return;
            }
            this._isTransitioningScene = true;

            SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.ButtonClickSfx, false, Vector3.zero).Forget();

            // TODO: Change scene and load the correct cinematic

            var storyIdToLoad = $"STORY_{this._currentAreaIdx + 1}"; // TODO: Change to static variable inside PnlCinematic
        }
        #endregion
    }
}