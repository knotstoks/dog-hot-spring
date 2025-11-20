using System;
using System.Collections.Generic;
using BroccoliBunnyStudios.Extensions;
using BroccoliBunnyStudios.Pools;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace BroccoliBunnyStudios.Panel
{
    public class PanelManager
    {
        public static readonly Lazy<PanelManager> s_lazy = new(() => new PanelManager());

        private PanelManager()
        {
            this.SetRoot(GameManager.Instance);
        }

        public static PanelManager Instance => s_lazy.Value;

        public Canvas Canvas => this._canvas;
        public Canvas TooltipCanvas => this._tooltipCanvas;
        public Canvas FadeCanvas => this._fadeCanvas;

        private readonly List<BasePanel> _panelStack = new();
        private GameObject _panelManagerRoot;
        private Canvas _canvas;
        private Canvas _tooltipCanvas;
        private Canvas _fadeCanvas;
        private Image _fade;
        private UISceneTransitionMask _transitionMask;

        public void SetRoot(GameManager gm)
        {
            var go = new GameObject(nameof(PanelManager));
            go.SetParent(gm.Root);
            go.layer = LayerMask.NameToLayer("UI");

            this._canvas = go.FGetComp<Canvas>();

            // Camera Setup
            this._canvas.renderMode = RenderMode.ScreenSpaceCamera;
            this._canvas.worldCamera = Camera.main;
            this._canvas.sortingLayerID = SortingLayer.NameToID("ui");
            this._canvas.planeDistance = 10f;

            // Fix layering issue for popup
            this._canvas.sortingOrder = 100;

            this._panelManagerRoot = go;
            this._panelManagerRoot.FGetComp<GraphicRaycaster>();
            var scaler = this._panelManagerRoot.FGetComp<CanvasScaler>();
            if (scaler)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
                scaler.referenceResolution = new Vector2(x: 1920, y: 1080);
            }

            this.SetUpTooltip(go, gm);
            this.SetUpFade(go, gm);
        }

        private void SetUpTooltip(GameObject rootObject, GameManager gm)
        {
            var tooltip = Object.Instantiate(rootObject, gm.Root);
            tooltip.name = "tooltip";
            var tooltipCanvas = tooltip.FGetComp<Canvas>();
            tooltipCanvas.sortingLayerID = SortingLayer.NameToID("ui");
            tooltipCanvas.sortingOrder = 150;
            tooltipCanvas.planeDistance = 10f;

            this._tooltipCanvas = tooltipCanvas;
        }

        private void SetUpFade(GameObject rootObject, GameManager gm)
        {
            var fade = Object.Instantiate(rootObject, gm.Root);
            fade.name = "fade";
            var fadeCanvas = fade.FGetComp<Canvas>();
            fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            fadeCanvas.sortingLayerID = SortingLayer.NameToID("ui_top");
            this._fade = fade.FGetComp<Image>();
            this._fade.raycastTarget = true;
            this._fade.color = new Color(0f, 0f, 0f, 0f);
            this._fade.SetActive(false);

            this._fadeCanvas = fadeCanvas;

            var prefab = ResourceLoader.Load<GameObject>("ui/prefab/uiscenetransitionmask.prefab");
            var instance = Object.Instantiate(prefab, this._fadeCanvas.transform);
            this._transitionMask = instance.GetComponent<UISceneTransitionMask>();
        }

        public async UniTask<T> ShowAsync<T>(Action<T> action = null)
            where T : BasePanel
        {
            var pnl = await this.LoadPanelAsync<T>();
            action?.Invoke(pnl);

            return pnl;
        }

        private async UniTask<T> LoadPanelAsync<T>()
            where T : BasePanel
        {
            var parent = this._panelManagerRoot.transform;
            var name = this.GetPanelResourcePath<T>();
            var go = await ResourceLoader.InstantiateAsync(name);

            go.name = typeof(T).Name;
            go.transform.SetParent(parent);
            go.transform.ResetLocalTransform();
            ((RectTransform)go.transform).sizeDelta = Vector2.zero;

            var comp = go.FGetComp<T>();
            this._panelStack.Add(comp);

            return comp;
        }

        private string GetPanelResourcePath<T>()
            where T : BasePanel
        {
            return typeof(T).Name;
        }

        public void OnClose(BasePanel pnl)
        {
            ResourceLoader.Destroy(pnl.gameObject);
        }

        public async UniTask FadeToBlackAsync(float duration = 1f)
        {
            if (duration <= 0f)
            {
                this._fade.color = new Color(0, 0, 0, 1);
                this._fade.SetActive(true);
                await this._transitionMask.FadeToBlackAsync(duration);
                return;
            }

            this._fade.color = new Color(0, 0, 0, 0);
            this._fade.SetActive(true);

            await this._transitionMask.FadeToBlackAsync(duration);
            this._fade.color = new Color(0, 0, 0, 1);

            // Add one frame to make sure it goes full black
            await UniTask.Yield();
        }

        public async UniTask FadeFromBlack(float duration = 1f)
        {
            if (duration <= 0f)
            {
                this._fade.color = new Color(0, 0, 0, 0);
                this._fade.SetActive(false);
                await this._transitionMask.FadeFromBlack(duration);
                return;
            }

            this._fade.SetActive(true);

            this._fade.color = new Color(0, 0, 0, 0);
            await this._transitionMask.FadeFromBlack(duration);

            this._fade.SetActive(false);
        }
    }
}