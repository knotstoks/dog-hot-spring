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

        [field: SerializeField]
        private Button CreditsButton { get; set; }

        [field: SerializeField, Header("Sfxes")]
        private AudioPlaybackInfo ButtonClickSfx { get; set; }

        private bool _isTransitioningScene;

        private void Awake()
        {
            PanelManager.Instance.FadeToBlackAsync(0).Forget();

            this.PlayButton.OnClick(() => this.OnPlayButtonClick().Forget());
            this.OptionsButton.OnClick(this.OnOptionsButtonClick);
            this.QuitButton.OnClick(this.OnQuitButtonClick);
            this.CreditsButton.OnClick(() => this.OnCreditsButtonClick().Forget());

            PanelManager.Instance.FadeFromBlack().Forget();
        }

        private async UniTaskVoid OnPlayButtonClick()
        {
            if (this._isTransitioningScene)
            {
                return;
            }
            this._isTransitioningScene = true;
            SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.ButtonClickSfx, false, Vector3.zero).Forget();

            await PanelManager.Instance.FadeToBlackAsync();

            if (!UserSaveDataManager.Instance.HasSeenStory("STORY_1"))
            {
                // Go to cinematic instead
                PnlCinematic.StoryIdToLoad = "STORY_1";
                SceneManager.Instance.LoadSceneAsync("ScCinematic").Forget();
            }
            else
            {
                // Go to ScHome
                SceneManager.Instance.LoadSceneAsync("ScHome").Forget();
            }
        }

        private void OnOptionsButtonClick()
        {
            if (this._isTransitioningScene)
            {
                return;
            }
            this._isTransitioningScene = true;
            SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.ButtonClickSfx, false, Vector3.zero).Forget();

            PanelManager.Instance.ShowAsync<PnlSettings>().Forget();

            this._isTransitioningScene = false;
        }

        private async void OnQuitButtonClick()
        {
            if (this._isTransitioningScene)
            {
                return;
            }
            this._isTransitioningScene = true;
            SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.ButtonClickSfx, false, Vector3.zero).Forget();

            await UniTask.WaitForSeconds(1f);
            if (!this) return;

            Application.Quit();
        }

        private async UniTaskVoid OnCreditsButtonClick()
        {
            if (this._isTransitioningScene)
            {
                return;
            }
            this._isTransitioningScene = true;
            SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.ButtonClickSfx, false, Vector3.zero).Forget();

            await PanelManager.Instance.ShowAsync<PnlCredits>();
            if (!this) return;

            this._isTransitioningScene = false;
        }
    }
}