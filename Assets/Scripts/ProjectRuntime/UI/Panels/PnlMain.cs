using BroccoliBunnyStudios.Extensions;
using BroccoliBunnyStudios.Managers;
using BroccoliBunnyStudios.Panel;
using BroccoliBunnyStudios.Sound;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectRuntime.UI.Panels
{
    public class PnlMain : MonoBehaviour
    {
        [field: SerializeField, Header("User Cheats")]
        private int CheatWorldProgress { get; set; }

        [field: SerializeField]
        private string CheatStoryProgress { get; set; }

        [Button]
        private void SetUserProgress()
        {
            UserSaveDataManager.Instance.SetCurrentWorldProgress(this.CheatWorldProgress);
        }

        [Button]
        private void SetUserStory()
        {
            UserSaveDataManager.Instance.RegisterStory(this.CheatStoryProgress);
        }

        [field: SerializeField, Header("Scene References")]
        private Button PlayButton { get; set; }

        [field: SerializeField]
        private Button OptionsButton { get; set; }

        [field: SerializeField]
        private Button QuitButton { get; set; }

        [field: SerializeField, Header("Sfxes")]
        private AudioPlaybackInfo ButtonClickSfx { get; set; }

        private bool _isTransitioningScene;

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
            if (this._isTransitioningScene)
            {
                return;
            }
            this._isTransitioningScene = true;

            SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.ButtonClickSfx, false, Vector3.zero).Forget();

            await PanelManager.Instance.FadeToBlackAsync();

            SceneManager.Instance.LoadSceneAsync("ScHome").Forget();

            // For playtesting
            //BattleManager.LevelIdToLoad = 1;
            //SceneManager.Instance.LoadSceneAsync("ScGame").Forget();
        }

        private void OnOptionsButtonClick()
        {
            // TODO
        }

        private void OnQuitButtonClick()
        {
            if (this._isTransitioningScene)
            {
                return;
            }
            this._isTransitioningScene = true;

            Application.Quit();
        }
    }
}