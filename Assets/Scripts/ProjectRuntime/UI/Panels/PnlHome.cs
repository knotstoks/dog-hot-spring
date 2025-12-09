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
        private Button SettingsButton { get; set; }

        [field: SerializeField]
        private GameObject LevelSelectButtonParent { get; set; }

        [field: SerializeField]
        private List<Button> LevelSelectButtons { get; set; }

        [field: SerializeField]
        private Button PreviousAreaButton { get; set; }

        [field: SerializeField]
        private Button NextAreaButton { get; set; }

        [field: SerializeField, Header("Button Sprites")]
        private Sprite GrayButtonSprite { get; set; }

        private bool _isTransitioningScene;

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

            await PanelManager.Instance.FadeFromBlack();
            if (!this) return;


        }

        public void ToggleAllButtonsShow(bool toggle)
        {
            this.SettingsButton.gameObject.SetActive(toggle);
            this.LevelSelectButtonParent.SetActive(toggle);
            this.PreviousAreaButton.gameObject.SetActive(toggle);
            this.NextAreaButton.gameObject.SetActive(toggle);
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

            // TODO


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

            // TODO


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