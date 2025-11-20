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
        private Button QuitButton { get; set; }

        private void Awake()
        {
            this.PlayButton.OnClick(this.OnPlayButtonClick);
            this.QuitButton.OnClick(this.OnQuitButtonClick);
        }

        private async void OnPlayButtonClick()
        {
            await PanelManager.Instance.FadeToBlackAsync();

            SceneManager.Instance.LoadSceneAsync("ScHome").Forget();
        }

        private void OnQuitButtonClick()
        {
            Application.Quit();
        }
    }
}