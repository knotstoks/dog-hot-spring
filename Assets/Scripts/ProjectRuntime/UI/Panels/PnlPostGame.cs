using BroccoliBunnyStudios.Extensions;
using BroccoliBunnyStudios.Managers;
using BroccoliBunnyStudios.Panel;
using BroccoliBunnyStudios.Sound;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ProjectRuntime.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectRuntime.UI.Panels
{
    /* PnlPostGame Flow
     * 1) Victory Sfx will play
     * 2) Panel will fade in to black
     * 3) Bath Animation from below will move up and level clear animation will move down
     * 4) Random Animal animation will pop up and the button parent will animate in
     * 5) Steam vfx from the onsen will start playing
     */

    public class PnlPostGame : BasePanel
    {
        [field: SerializeField, Header("Scene References")]
        private Animator BathAnimator { get; set; }

        [field: SerializeField]
        private Animator AnimalAnimator { get; set; }

        [field: SerializeField]
        private Image BackgroundImage { get; set; }

        [field: SerializeField]
        private float BackgroundFinalAlphaValue { get; set; } = 0.78f;

        [field: SerializeField]
        private float BackgroundFadeDuration { get; set; } = 1f;

        [field: SerializeField]
        private Button CinematicNextButton { get; set; }

        [field: SerializeField]
        private GameObject NormalButtonParent { get; set; }

        [field: SerializeField]
        private Button NextStageButton { get; set; }

        [field: SerializeField]
        private Button ReturnToMainMenuButton { get; set; }

        [field: SerializeField, Header("Sfxes")]
        private AudioPlaybackInfo VictorySfx { get; set; }

        // Constant Variables
        private const string BATH_IN_ANIM = "in";
        private static readonly string[] RANDOM_ANIMAL_ANIM = { "red", "blue", "green", "yellow", "orange", "white", "black", "purple", "pink" };

        // Internal Variables
        private bool _isTransitioningScene;

        private void Awake()
        {
            this.NormalButtonParent.SetActive(false);
            this.CinematicNextButton.gameObject.SetActive(false);
            this.Init().Forget();
            this.NextStageButton.OnClick(this.OnNextStageButtonClick);
            this.ReturnToMainMenuButton.OnClick(this.OnReturnToMainMenuButtonClick);
        }

        private async UniTaskVoid Init()
        {
            //SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.VictorySfx, false, Vector3.zero).Forget();

            await this.BackgroundImage.DOFade(this.BackgroundFinalAlphaValue, this.BackgroundFadeDuration);
            if (!this) return;

            var stateInfo = this.BathAnimator.GetCurrentAnimatorStateInfo(0);
            this.BathAnimator.Play(BATH_IN_ANIM);
            while (!stateInfo.IsName(BATH_IN_ANIM))
            {
                await UniTask.Yield();
                if (!this) return;

                stateInfo = this.BathAnimator.GetCurrentAnimatorStateInfo(0);
            }

            while (stateInfo.IsName(BATH_IN_ANIM) && stateInfo.normalizedTime < 1f)
            {
                await UniTask.Yield();
                if (!this) return;

                stateInfo = this.BathAnimator.GetCurrentAnimatorStateInfo(0);
            }

            //var randomizedAnimalAnim = RANDOM_ANIMAL_ANIM[Random.Range(0, RANDOM_ANIMAL_ANIM.Length)];
            //stateInfo = this.AnimalAnimator.GetCurrentAnimatorStateInfo(0);
            //this.AnimalAnimator.Play(randomizedAnimalAnim);
            //while (!stateInfo.IsName(randomizedAnimalAnim))
            //{
            //    await UniTask.Yield();
            //    if (!this) return;

            //    stateInfo = this.AnimalAnimator.GetCurrentAnimatorStateInfo(0);
            //}

            //while (stateInfo.IsName(randomizedAnimalAnim) && stateInfo.normalizedTime < 1f)
            //{
            //    await UniTask.Yield();
            //    if (!this) return;

            //    stateInfo = this.AnimalAnimator.GetCurrentAnimatorStateInfo(0);
            //}

            this.NormalButtonParent.SetActive(true);
            //this.CinematicNextButton.gameObject.SetActive(true);
        }

        private async void OnNextStageButtonClick()
        {
            if (this._isTransitioningScene)
            {
                return;
            }
            this._isTransitioningScene = true;

            await PanelManager.Instance.FadeToBlackAsync();
            if (!this) return;

            BattleManager.LevelIdToLoad++;
            SceneManager.Instance.LoadSceneAsync("ScGame").Forget();
        }

        private async void OnReturnToMainMenuButtonClick()
        {
            if (this._isTransitioningScene)
            {
                return;
            }
            this._isTransitioningScene = true;

            await PanelManager.Instance.FadeToBlackAsync();
            if (!this) return;

            SceneManager.Instance.LoadSceneAsync("ScMain").Forget();

            this.Close();
        }
    }
}