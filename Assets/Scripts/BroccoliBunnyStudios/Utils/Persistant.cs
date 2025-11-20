using BroccoliBunnyStudios.Panel;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BroccoliBunnyStudios.Utils
{
    public class Persistant : MonoBehaviour
    {
        public static Persistant Instance { get; private set; }

        public bool IsPaused => GamePauseState != PauseType.None;
        public PauseType GamePauseState { get; private set; } = PauseType.None;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(this);
        }
    }

    //    public void PauseGame(PauseType pauseType)
    //    {
    //        GamePauseState |= pauseType;
    //        if (IsPaused)
    //        {
    //            Time.timeScale = 0f;
    //            PanelManager.Instance.ShowAsync<PnlPause>().Forget();
    //        }
    //    }

    //    public void ResumeGame(PauseType pauseType)
    //    {
    //        GamePauseState &= ~pauseType;
    //        if (!IsPaused)
    //        {
    //            Time.timeScale = 1f;
    //        }
    //    }
    //}

    public enum PauseType
    {
        None = 0,
        PnlPause = 1 << 0,
    }
}
