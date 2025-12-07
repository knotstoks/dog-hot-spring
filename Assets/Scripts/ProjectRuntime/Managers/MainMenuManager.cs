using BroccoliBunnyStudios.Extensions;
using BroccoliBunnyStudios.Managers;
using BroccoliBunnyStudios.Panel;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectRuntime.Managers
{
    public class MainMenuManager : MonoBehaviour
    {
        [field: SerializeField, Header("Scene References")]
        private Button PlayButton { get; set; }

        [field: SerializeField]
        private Button OptionsButton { get; set; }

        [field: SerializeField]
        private Button QuitButton { get; set; }

        private void Awake()
        {
            PanelManager.Instance.FadeToBlackAsync(0).Forget();

            this.PlayButton.OnClick(this.OnPlayButtonClick);
            this.OptionsButton.OnClick(this.OnOptionsButtonClick);
            this.QuitButton.OnClick(this.OnQuitButtonClick);

            PanelManager.Instance.FadeFromBlack().Forget();
        }

        private async void OnPlayButtonClick()
        {
            await PanelManager.Instance.FadeToBlackAsync();

            // TODO
            //SceneManager.Instance.LoadSceneAsync("ScHome").Forget();

            BattleManager.LevelIdToLoad = 1;
            SceneManager.Instance.LoadSceneAsync("ScGame").Forget();

            // TEMP for playtesting
            TimeManager.Instance.SetStartTime();
        }

        private void OnOptionsButtonClick()
        {
            // TODO
        }

        private void OnQuitButtonClick()
        {
            Application.Quit();
        }
    }
}