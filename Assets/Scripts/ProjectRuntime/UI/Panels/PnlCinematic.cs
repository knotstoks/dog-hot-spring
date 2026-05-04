using BroccoliBunnyStudios.Managers;
using BroccoliBunnyStudios.Panel;
using BroccoliBunnyStudios.Sound;
using BroccoliBunnyStudios.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectRuntime.UI.Panels
{
    public class PnlCinematic : MonoBehaviour
    {
        public static int StoryIdToLoad { get; set; } = 0;

        [field: SerializeField, Header("Scene References")]
        private RectTransform CanvasRT { get; set; }

        [field: SerializeField]
        public RectTransform LeftScreenRT { get; private set; }

        [field: SerializeField]
        public RectTransform MiddleScreenRT { get; private set; }

        [field: SerializeField]
        public RectTransform RightScreenRT { get; private set; }

        [field: SerializeField]
        private Button NextSceneButton { get; set; }

        [field: SerializeField]
        private Button PreviousSceneButton { get; set; }

        [field: SerializeField, Header("Sfxes")]
        private AudioPlaybackInfo ButtonClickSfx { get; set; }

        private UICinematic _uiCinematic;
        private bool _isTransitioning;

        private void Awake()
        {
            this.NextSceneButton.onClick.AddListener(this.OnNextSceneButtonClick);
            this.PreviousSceneButton.onClick.AddListener(this.OnPreviousSceneButtonClick);
            this.NextSceneButton.gameObject.SetActive(false);
            this.PreviousSceneButton.gameObject.SetActive(false);
        }

        private void Start()
        {
            this.Init().Forget();
        }

        private async UniTaskVoid Init()
        {
            // TODO: Only for Demo
            if (StoryIdToLoad == 3)
            {
                await PanelManager.Instance.FadeToBlackAsync();

                SceneManager.Instance.LoadSceneAsync("ScEndDemo").Forget();
                return;
            }

            var cinematicObject = CommonUtil.InstantiatePrefab(DStory.GetDataById(StoryIdToLoad).Value.StoryPrefabPath, this.CanvasRT); // TODO: Hardcoded for now
            cinematicObject.transform.SetAsFirstSibling();

            await PanelManager.Instance.FadeFromBlack();
            if (!this) return;

            await UniTask.WaitForSeconds(0.5f);
            if (!this) return;

            this._uiCinematic = cinematicObject.GetComponent<UICinematic>();
            this._uiCinematic.InitAndPlay(this);
        }

        public async UniTask ShowNextSceneButton()
        {
            this.NextSceneButton.gameObject.SetActive(true);

            await this.NextSceneButton.image.DOFade(1f, 1f);
            if (!this) return;
        }

        public async UniTask ShowPreviousSceneButton()
        {
            this.PreviousSceneButton.gameObject.SetActive(true);

            await this.PreviousSceneButton.image.DOFade(1f, 1f);
            if (!this) return;
        }

        public async UniTask HideNextSceneButton()
        {
            this.NextSceneButton.interactable = false;

            await this.NextSceneButton.image.DOFade(0f, 1f);
            if (!this) return;

            this.NextSceneButton.gameObject.SetActive(false);
            this.NextSceneButton.interactable = true;
        }

        public async UniTask HidePreviousSceneButton()
        {
            this.PreviousSceneButton.interactable = false;

            await this.PreviousSceneButton.image.DOFade(0f, 1f);
            if (!this) return;

            this.PreviousSceneButton.gameObject.SetActive(false);
            this.PreviousSceneButton.interactable = true;
        }

        private void OnNextSceneButtonClick()
        {
            SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.ButtonClickSfx, false, Vector3.zero).Forget();

            this._uiCinematic.MoveNextScene();
        }

        private void OnPreviousSceneButtonClick()
        {
            SoundManager.Instance.PlayAudioPlaybackInfoAsync(this.ButtonClickSfx, false, Vector3.zero).Forget();

            this._uiCinematic.MovePreviousScene();
        }

        public async UniTaskVoid ReturnToScHome()
        {
            if (this._isTransitioning)
            {
                return;
            }
            this._isTransitioning = true;

            var usdm = UserSaveDataManager.Instance;
            if (!usdm.HasSeenStory(StoryIdToLoad))
            {
                var dStory = DStory.GetDataById(StoryIdToLoad).Value;
                var numberOfAreas = DWorld.GetAllData().Data.Count / 10;
                if (dStory.StoryId == 1 || dStory.StoryId > numberOfAreas)
                {
                    // Edge case where Player just started game so no transition
                    // OR
                    // Edge case where Player is watching last world so no transition to next
                    PnlHome.AreaToTransition = -1;
                }
                else
                {
                    // This is 0 indexed so transition to next area
                    PnlHome.AreaToTransition = dStory.StoryId - 1;
                }
                
                UserSaveDataManager.Instance.RegisterStory(StoryIdToLoad);
            }
            else
            {
                PnlHome.AreaToTransition = -1;
            }

            await PanelManager.Instance.FadeToBlackAsync();
            if (!this) return;

            SceneManager.Instance.LoadSceneAsync("ScHome").Forget();
        }
    }
}