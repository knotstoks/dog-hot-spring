using BroccoliBunnyStudios.Managers;
using BroccoliBunnyStudios.Panel;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ProjectRuntime.UI.Panels
{
    public class PnlCinematic : MonoBehaviour
    {
        public static string StoryIdToLoad { get; set; } = string.Empty;

        private void Start()
        {
            this.Init().Forget();
        }

        private async UniTaskVoid Init()
        {
            // TODO: Get the prefab to load

            await PanelManager.Instance.FadeFromBlack();
            if (!this) return;

            // TEMP
            this.ReturnToScHome().Forget();
        }

        private async UniTaskVoid ReturnToScHome()
        {
            // TEMP
            await UniTask.WaitForSeconds(2f);
            if (!this) return;

            var usdm = UserSaveDataManager.Instance;
            if (!usdm.HasSeenStory(StoryIdToLoad))
            {
                PnlHome.AreaToTransition = 1; // TEMP
                UserSaveDataManager.Instance.RegisterStory(StoryIdToLoad);
            }

            await PanelManager.Instance.FadeToBlackAsync();
            if (!this) return;

            SceneManager.Instance.LoadSceneAsync("ScHome").Forget();
        }
    }
}