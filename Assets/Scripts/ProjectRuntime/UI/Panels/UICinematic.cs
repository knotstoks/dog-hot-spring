using System.Collections.Generic;
using BroccoliBunnyStudios.Extensions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace ProjectRuntime.UI.Panels
{
    public class UICinematic : MonoBehaviour
    {
        [field: SerializeField, Header("Scene References")]
        private PlayableDirector ScenePlayableDirector { get; set; }

        [field: SerializeField]
        private List<PlayableAsset> PlayableAssets { get; set; }

        [field: SerializeField]
        private Button FullScreenButton { get; set; }

        private int _idx = -1; // Initialized as -1 because of adding 1 to idx for the initial state
        private bool _isInCutscene = false;
        private bool _wasFullscreenButtonClicked = false;

        private void Awake()
        {
            this.FullScreenButton.OnClick(this.OnFullScreenButtonClick);
        }

        public async UniTask InitAndPlay()
        {
            this.FullScreenButton.gameObject.SetActive(false);
            this._isInCutscene = true;

            while (this._idx < this.PlayableAssets.Count)
            {
                this._idx++;

                await this.PlayNextCinematic();
                if (!this) return;
            }

            this._isInCutscene = false;
            this.FullScreenButton.gameObject.SetActive(false);
            await UniTask.CompletedTask;
        }

        private async UniTask PlayNextCinematic()
        {
            this.ScenePlayableDirector.Play(this.PlayableAssets[this._idx]);
            await UniTask.WaitUntil(() => this.ScenePlayableDirector.state == PlayState.Paused);
            if (!this) return;

            await this.WaitForPlayerInput();
            if (!this) return;
        }

        private async UniTask WaitForPlayerInput()
        {
            this.FullScreenButton.gameObject.SetActive(true);

            await UniTask.WaitUntil(() => this._wasFullscreenButtonClicked);
            if (!this) return;

            this.FullScreenButton.gameObject.SetActive(false);
            this._wasFullscreenButtonClicked = false;
        }

        private void OnFullScreenButtonClick()
        {
            if (!this._isInCutscene)
            {
                return;
            }

            this._wasFullscreenButtonClicked = true;
        }
    }
}