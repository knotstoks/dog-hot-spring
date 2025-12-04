using BroccoliBunnyStudios.Managers;
using BroccoliBunnyStudios.Panel;
using Cysharp.Threading.Tasks;
using Sirenix.Utilities;
using UnityEngine;

namespace ProjectRuntime.Managers
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        public static int LevelIdToLoad { get; set; } = -1; // Set outside ScGame

        [field: SerializeField, Header("Editor Cheats")]
        private int EditorIdToLoad { get; set; } = 1;

        [field: SerializeField, Header("Scene References")]
        public Transform PuzzleGridTransform { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.Log("There are 2 or more BattleManagers in the scene");
            }

            PanelManager.Instance.FadeToBlackAsync(0f).Forget();
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        private void Start()
        {
            this.Init();
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.Instance.LoadSceneAsync("ScGame").Forget();
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                LevelIdToLoad++;
                SceneManager.Instance.LoadSceneAsync("ScGame").Forget();
            }
#endif
        }

        private async void Init()
        {
            if (LevelIdToLoad <= 0)
            {
                LevelIdToLoad = EditorIdToLoad;
            }

            await UniTask.WaitUntil(() => GridManager.Instance != null);
            if (!this) return;

            await GridManager.Instance.Init(LevelIdToLoad);
            if (!this) return;

            PanelManager.Instance.FadeFromBlack().Forget();
        }

        public Vector2Int GetNearestTileYX()
        {
            return Vector2Int.zero;
        }

        public async void ShowVictoryPanel()
        {
            // This is to wait for the last animals to drop
            await UniTask.WaitForSeconds(1f);
            if (!this) return;

            //PanelManager.Instance.ShowAsync<PnlPostGame>().Forget();
        }
    }
}