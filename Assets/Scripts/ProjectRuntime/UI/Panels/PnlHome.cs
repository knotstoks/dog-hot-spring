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

        [field: SerializeField, Header("Button Sprites")]
        private Sprite RedButtonSprite { get; set; }

        [field: SerializeField]
        private Sprite GrayButtonSprite { get; set; }

        // Internal Variables
        private int _numberOfWorlds; // Set only once in init
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
            this.SettingsButton.OnClick(this.OnSettingsButtonClick);
            for (var i = 0; i < this.LevelSelectButtons.Count; i++)
            {
                var temp = i + 1; // Neccessary to create temp variable for the closure function
                this.LevelSelectButtons[i].OnClick(() => this.OnLevelSelectButtonClick(temp));
            }
            this.PreviousAreaButton.OnClick(this.OnPreviousAreaButtonClick);
            this.NextAreaButton.OnClick(this.OnNextAreaButtonClick);
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
            this._currentAreaIdx = currentLevel % 10 == 0 // 0-indexed
                ? Mathf.Max(currentLevel / 10 - 1, 0)
                : currentLevel / 10;

            await PanelManager.Instance.FadeFromBlack();
            if (!this) return;

            this.RefreshUI(this._currentAreaIdx).Forget();
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
        }

        public void ToggleAllButtonsShow(bool toggle)
        {
            this.SettingsButton.gameObject.SetActive(toggle);
            this.LevelSelectButtonParent.SetActive(toggle);

            var currentWorldProgress = UserSaveDataManager.Instance.GetCurrentWorldProgress();
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

            BattleManager.LevelIdToLoad = level;

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
            this.ToggleAllButtonsShow(false);

            await this.RefreshUI(this._currentAreaIdx - 1);
            if (!this) return;

            this._isTransitioningScene = false;
            this.ToggleAllButtonsShow(true);
        }

        private void OnSettingsButtonClick()
        {
            // TODO
        }
        #endregion
    }
}