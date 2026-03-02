using System.Collections.Generic;
using BroccoliBunnyStudios.Extensions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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
        private List<GameObject> Panels { get; set; }

        // Internal Variables
        private PnlCinematic _pnlCinematic;
        private int _currentIdx = 0;
        private List<CanvasGroup> _canvasGroups;

        private const float MOVE_TIME = 0.5f;

        private void Awake()
        {
            foreach (var panel in this.Panels)
            {
                this._canvasGroups.Add(panel.GetComponent<CanvasGroup>());
                panel.SetActive(false);
            }

            if (this.Panels.Count != this._canvasGroups.Count)
            {
                Debug.LogError($"{this.Panels.Count} panels but {this._canvasGroups.Count} canvas groups!");
            }
        }

        public void InitAndPlay(PnlCinematic pnlCinematic)
        {
            this._pnlCinematic = pnlCinematic;
            this._currentIdx = 0;

            this.TriggerNextPanel().Forget();
        }

        public void MoveNextScene()
        {
            this._pnlCinematic.HideNextSceneButton().Forget();

            this._currentIdx++;
            this.TriggerNextPanel().Forget();
        }

        private async UniTaskVoid TriggerNextPanel()
        {
            //var rt = this.Panels[this._currentIdx].transform as RectTransform;

            //if (this._currentIdx > 0)
            //{
                
            //}

            //rt.anchoredPosition = this._pnlCinematic.RightScreenRT.anchoredPosition;
            //var timer = 0f;
            //while (timer < MOVE_TIME)
            //{
            //    this.

            //    timer += Time.deltaTime;
            //    await UniTask.Yield();
            //    if (!this) return;
            //}
            


            //rt.DOAnchorPos3D(this._pnlCinematic.MiddleScreenRT)

            //this._pnlCinematic.ShowNextSceneButton().Forget();
        }
    }
}