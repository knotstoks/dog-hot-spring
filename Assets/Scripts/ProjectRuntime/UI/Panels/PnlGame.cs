using BroccoliBunnyStudios.Extensions;
using BroccoliBunnyStudios.Panel;
using BroccoliBunnyStudios.Sound;
using Cysharp.Threading.Tasks;
using ProjectRuntime.Managers;
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

        [field: SerializeField, Header("Sfxes")]
        private AudioPlaybackInfo ButtonClickSfx { get; set; }

        private void Awake()
        {
            this.SettingsButton.OnClick(() => this.OnSettingsButtonClick().Forget());
            this.ResetButton.OnClick(() => this.OnResetButtonClick().Forget());
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
    }
}