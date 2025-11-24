using BroccoliBunnyStudios.Managers;
using Cysharp.Threading.Tasks;
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
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.Instance.LoadSceneAsync("ScGame").Forget();
            }
        }

        private async void Init()
        {
            if (LevelIdToLoad <= 0)
            {
                LevelIdToLoad = EditorIdToLoad;
            }

            this.SetupLevel();
        }

        private async UniTask SetupLevel()
        {
            
        }

        public Vector2Int GetNearestTileYX()
        {
            return Vector2Int.zero;
        }
    }
}