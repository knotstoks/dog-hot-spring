using System;
using BroccoliBunnyStudios.Panel;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BroccoliBunnyStudios.Managers
{
    public class SceneManager
    {
        private static readonly Lazy<SceneManager> s_lazy = new();

        public SceneManager()
        {
            // Nothing
        }

        public static SceneManager Instance => s_lazy.Value;

        public event Action<string> OnSceneLoaded;

        public async UniTask LoadSceneAsync(string sceneName)
        {
            await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);

            // Setup all the canvases
            var renderMode = RenderMode.ScreenSpaceCamera;

            var c = PanelManager.Instance.Canvas;
            c.renderMode = renderMode;
            c.worldCamera = Camera.main;
            c.sortingLayerID = SortingLayer.NameToID("ui");

            var c2 = PanelManager.Instance.TooltipCanvas;
            c2.renderMode = renderMode;
            c2.worldCamera = Camera.main;
            c2.sortingLayerID = SortingLayer.NameToID("ui");

            var c3 = PanelManager.Instance.FadeCanvas;
            c3.renderMode = RenderMode.ScreenSpaceOverlay;
            c3.worldCamera = Camera.main;
            c3.sortingLayerID = SortingLayer.NameToID("ui_top");

            // Set sort order to force reordering within UI layer
            c.sortingOrder = 100;
            c2.sortingOrder = 150;

            this.OnSceneLoaded?.Invoke(sceneName);
        }
    }
}