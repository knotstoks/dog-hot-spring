using System;
using System.Collections.Generic;
using BroccoliBunnyStudios.Managers;
using BroccoliBunnyStudios.Panel;
using Cysharp.Threading.Tasks;
using ProjectRuntime.Tutorial;
using ProjectRuntime.UI.Panels;
using UnityEngine;

namespace ProjectRuntime.Managers
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        public static int LevelIdToLoad { get; set; } = -1; // Set outside ScGame

        public bool IsPaused => this._pauseType != PauseType.None;
        private PauseType _pauseType;

        [field: SerializeField, Header("Editor Cheats")]
        private int EditorIdToLoad { get; set; } = 1;

        [field: SerializeField, Header("Scene References")]
        public Transform PuzzleGridTransform { get; private set; }

        [field: SerializeField]
        private List<TutorialGame> TutorialGameControllers { get; set; }

        [field: SerializeField, Header("Containers")]
        public Transform VfxContainer { get; private set; }

        // Accessible Variables
        public bool WillBlockResetInput { get; set; } = false;

        // Internal Variables
        private const string LOC_LEVELRESET = "LOC_LEVELRESET";

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
            this.Init().Forget();
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.N))
            {
                LevelIdToLoad++;
                SceneManager.Instance.LoadSceneAsync("ScGame").Forget();
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                UserSaveDataManager.Instance.ResetAllTutorials();
                Debug.Log("Reset All Tutorials");
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                UserSaveDataManager.Instance.ResetAllStories();
                Debug.Log("Reset All Stories");
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                this.ShowVictoryPanel().Forget();
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                UserSaveDataManager.Instance.ResetCurrentWorldProgress();
                Debug.Log("Reset World Progress");
            }
#endif
            if (Input.GetKeyDown(KeyCode.R) )
            {
                this.TryResetLevel().Forget();
            }
        }

        private async UniTaskVoid Init()
        {
            // Start from black fullscreen
            PanelManager.Instance.FadeToBlackAsync(0).Forget();

            if (LevelIdToLoad <= 0)
            {
                LevelIdToLoad = EditorIdToLoad;
            }

            await UniTask.WaitUntil(() => GridManager.Instance != null);
            if (!this) return;

            await GridManager.Instance.Init(LevelIdToLoad);
            if (!this) return;

            foreach (var tutorialController in this.TutorialGameControllers)
            {
                if (LevelIdToLoad == tutorialController.WorldId)
                {
                    // Only activate at most 1 tutorial
                    var activated = tutorialController.CheckAndActivate();
                    if (activated)
                    {
                        break;
                    }
                }
            }

            // Fade out
            PanelManager.Instance.FadeFromBlack().Forget();
        }

        public Vector2Int GetNearestTileYX()
        {
            return Vector2Int.zero;
        }

        public async UniTaskVoid ShowVictoryPanel()
        {
            foreach (var emptyTile in GridManager.Instance.EmptySlideTileList)
            {
                emptyTile.ForceSnapToGrid();
            }

            // This is to wait for the last animals to drop
            await UniTask.WaitForSeconds(0.5f);
            if (!this) return;

            PanelManager.Instance.ShowAsync<PnlPostGame>().Forget();
        }

        public async UniTaskVoid TryResetLevel()
        {
            if (this.WillBlockResetInput)
            {
                return;
            }
            this.WillBlockResetInput = true;

            var pnlYesNoPrompt = await PanelManager.Instance.ShowAsync<PnlYesNoPrompt>((pnl) =>
            {
                pnl.Init(LOC_LEVELRESET, () => this.ResetLevel().Forget(), null, true);
            });
            await pnlYesNoPrompt.WaitWhilePanelIsAlive();
            if (!this) return;

            this.WillBlockResetInput = false;
        }

        private async UniTaskVoid ResetLevel()
        {
            SceneManager.Instance.LoadSceneAsync("ScGame").Forget();

            await UniTask.CompletedTask;
        }

        #region Pause Logic
        [Flags]
        public enum PauseType
        {
            None = 0,
            PnlPause = 1 << 0,
            PnlTutorial = 1 << 1,
        }

        public void PauseGame(PauseType pauseType)
        {
            this._pauseType |= pauseType;
            if (this.IsPaused)
            {
                Time.timeScale = 0f;
            }
        }

        public void ResumeGame(PauseType pauseType)
        {
            this._pauseType &= ~pauseType;
            if (!this.IsPaused)
            {
                Time.timeScale = 1f;
            }
        }
        #endregion
    }
}