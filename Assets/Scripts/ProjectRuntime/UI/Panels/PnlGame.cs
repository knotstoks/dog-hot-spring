using BroccoliBunnyStudios.Extensions;
using BroccoliBunnyStudios.Managers;
using BroccoliBunnyStudios.Panel;
using BroccoliBunnyStudios.Sound;
using Cysharp.Threading.Tasks;
using ProjectRuntime.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectRuntime.UI.Panels
{
    public class PnlGame : MonoBehaviour
    {
        [field: SerializeField, Header("Scene References")]
        private Button SettingsButton { get; set; }

        [field: SerializeField]
        private Button ResetButton { get; set; }

        [field: SerializeField]
        private TextMeshProUGUI LevelDisplayTMP { get; set; }

        [field: SerializeField, Header("Sfxes")]
        private AudioPlaybackInfo ButtonClickSfx { get; set; }

        private const string LOC_LEVELDISPLAY = "LOC_LEVELDISPLAY";

        private void Awake()
        {
            LocalizationManager.Instance.OnLocalizationChanged += this.OnLocalizationChanged;

            this.SettingsButton.OnClick(() => this.OnSettingsButtonClick().Forget());
            this.ResetButton.OnClick(() => this.OnResetButtonClick().Forget());
        }

        private void OnDestroy()
        {
            LocalizationManager.Instance.OnLocalizationChanged -= this.OnLocalizationChanged;
        }

        public void Init()
        {
            this.OnLocalizationChanged();
        }

        private async UniTaskVoid OnSettingsButtonClick()
        {
            if (PanelManager.Instance.IsPanelOpen<PnlSettings>())
            {
                return;
            }

            SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.ButtonClickSfx, false, Vector3.zero).Forget();
            PanelManager.Instance.ShowAsync<PnlSettings>().Forget();

            await UniTask.CompletedTask;
        }

        private async UniTaskVoid OnResetButtonClick()
        {
            if (BattleManager.Instance == null && BattleManager.Instance.WillBlockResetInput)
            {
                return;
            }

            SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.ButtonClickSfx, false, Vector3.zero).Forget();
            BattleManager.Instance.TryResetLevel().Forget();

            await UniTask.CompletedTask;
        }

        private void OnLocalizationChanged()
        {
            this.LevelDisplayTMP.text = string.Format(LocalizationManager.Instance[LOC_LEVELDISPLAY], BattleManager.LevelIdToLoad);
        }
    }
}