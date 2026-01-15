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
                var dStory = DStory.GetDataById(StoryIdToLoad).Value;
                var numberOfAreas = DWorld.GetAllData().Data.Count / 10;
                if (dStory.StoryNumber == 1 || dStory.StoryNumber > numberOfAreas)
                {
                    // Edge case where Player just started game so no transition
                    // OR
                    // Edge case where Player is watching last world so no transition to next
                    PnlHome.AreaToTransition = -1;
                }
                else
                {
                    // This is 0 indexed so transition to next area
                    PnlHome.AreaToTransition = dStory.StoryNumber - 1;
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