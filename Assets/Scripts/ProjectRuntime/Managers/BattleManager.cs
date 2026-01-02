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
                this.ShowVictoryPanel();
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                UserSaveDataManager.Instance.ResetCurrentWorldProgress();
                Debug.Log("Reset World Progress");
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

        public async void ShowVictoryPanel()
        {
            foreach (var emptyTile in GridManager.Instance.EmptySlideTileList)
            {
                emptyTile.ForceSnapToGrid();
            }

            // This is to wait for the last animals to drop
            await UniTask.WaitForSeconds(0.5f);
            if (!this) return;

            PanelManager.Instance.ShowAsync<PnlPostGame>().Forget();

            // For prototyping
            //var numberOfWorlds = DWorld.GetAllData().Data.Count;
            //if (LevelIdToLoad == numberOfWorlds)
            //{
            //    PanelManager.Instance.ShowAsync<PnlPostGame>().Forget();
            //}
            //else
            //{
            //    await PanelManager.Instance.FadeToBlackAsync();
            //    if (!this) return;

            //    // Continue the game
            //    LevelIdToLoad++;
            //    SceneManager.Instance.LoadSceneAsync("ScGame").Forget();
            //}
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