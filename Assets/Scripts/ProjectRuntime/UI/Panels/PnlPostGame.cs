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
    public class PnlPostGame : BasePanel
    {
        [field: SerializeField, Header("Scene References")]
        private Button ReturnToMainMenuButton { get; set; }

        [field: SerializeField]
        private TextMeshProUGUI TimeSpentTMP { get; set; }

        private TimeSpan _inGameTimeSpan;

        private void Awake()
        {
            this._inGameTimeSpan = TimeManager.Instance.GetTimeFromStart();
            this.TimeSpentTMP.text = $"{this._inGameTimeSpan.Minutes:00}:{this._inGameTimeSpan.Seconds:00}:{this._inGameTimeSpan.Milliseconds:000}";

            this.ReturnToMainMenuButton.OnClick(this.OnReturnToMainMenuButtonClick);
        }

        private async void OnReturnToMainMenuButtonClick()
        {
            await PanelManager.Instance.FadeToBlackAsync();
            if (!this) return;

            SceneManager.Instance.LoadSceneAsync("ScMain").Forget();

            this.Close();
        }
    }
}