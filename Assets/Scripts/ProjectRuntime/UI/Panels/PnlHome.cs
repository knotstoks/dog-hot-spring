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
        private Animator AreaAnimator { get; set; }

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

        private bool _isTransitioningScene;
        private int _currentAreaIdx;

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

            this.SettingsButton.OnClick(this.OnSettingsButtonClick);
            for (var i = 0; i < this.LevelSelectButtons.Count; i++)
            {
                var temp = i + 1; // Neccessary to create temp variable for the closure function
                this.LevelSelectButtons[i].OnClick(() => this.OnLevelSelectButtonClick(temp));
            }
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

            // TODO: Move the camera using dolly?

            this.ToggleAllButtonsShow(true);
        }

        public void ToggleAllButtonsShow(bool toggle)
        {
            this.SettingsButton.gameObject.SetActive(toggle);
            this.LevelSelectButtonParent.SetActive(toggle);

            var numberOfWorlds = DWorld.GetAllData().Data.Count;
            var numberOfPages = numberOfWorlds % 10 == 0
                ? numberOfWorlds / 10
                : numberOfWorlds / 10 + 1;
            this.PreviousAreaButton.gameObject.SetActive(toggle && this._currentAreaIdx != 0);
            this.NextAreaButton.gameObject.SetActive(toggle && this._currentAreaIdx < numberOfPages - 1);
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

            await this.RefreshUI(this._currentAreaIdx);
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

            await this.RefreshUI(this._currentAreaIdx);
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