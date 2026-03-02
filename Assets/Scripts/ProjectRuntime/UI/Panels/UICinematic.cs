using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectRuntime.UI.Panels
{
    public class UICinematic : MonoBehaviour
    {
        [field: SerializeField, Header("Scene References")]
        private List<GameObject> Panels { get; set; }

        // Internal Variables
        private PnlCinematic _pnlCinematic;
        private int _currentIdx = 0;
        private List<CanvasGroup> _canvasGroups;

        private const float MOVE_TIME = 3f;

        private void Awake()
        {
            this._canvasGroups = new();
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

            if (this._currentIdx >= this.Panels.Count)
            {
                this._pnlCinematic.ReturnToScHome().Forget();
                return;
            }

            this.TriggerNextPanel().Forget();
        }

        private async UniTaskVoid TriggerNextPanel()
        {
            var rt = this.Panels[this._currentIdx].transform as RectTransform;
            var timer = 0f;
            if (this._currentIdx > 0)
            {

            }

            this.Panels[this._currentIdx].gameObject.SetActive(true);
            rt.anchoredPosition = this._pnlCinematic.RightScreenRT.anchoredPosition;
            timer = 0f;
            var dist = this._pnlCinematic.RightScreenRT.anchoredPosition - this._pnlCinematic.MiddleScreenRT.anchoredPosition;
            while (timer < MOVE_TIME)
            {
                var v = timer / MOVE_TIME;
                var t = DOVirtual.EasedValue(0f, 1f, v, Ease.OutQuad);
                rt.anchoredPosition = this._pnlCinematic.RightScreenRT.anchoredPosition - dist * t;
                this._canvasGroups[this._currentIdx].alpha = t;

                timer += Time.deltaTime;
                await UniTask.Yield();
                if (!this) return;
            }
            rt.anchoredPosition = this._pnlCinematic.MiddleScreenRT.anchoredPosition;
            this._canvasGroups[this._currentIdx].alpha = 1f;

            this._pnlCinematic.ShowNextSceneButton().Forget();
        }
    }
}