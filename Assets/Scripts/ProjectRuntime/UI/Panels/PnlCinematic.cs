using BroccoliBunnyStudios.Managers;
using BroccoliBunnyStudios.Panel;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ProjectRuntime.UI.Panels
{
    public class PnlCinematic : MonoBehaviour
    {
        public static string CinematicIdToLoad { get; set; } = string.Empty;

        private void Start()
        {
            this.Init().Forget();
        }

        private async UniTaskVoid Init()
        {
            // TEMP
            await UniTask.WaitForSeconds(2f);
            if (!this) return;

            // TODO: Get the prefab to load

            PnlHome.AreaToTransition = 1;

            await PanelManager.Instance.FadeToBlackAsync();
            if (!this) return;

            SceneManager.Instance.LoadSceneAsync("ScHome").Forget();
        }
    }
}