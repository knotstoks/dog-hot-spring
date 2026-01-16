using BroccoliBunnyStudios.Managers;
using ProjectRuntime.Managers;
using UnityEngine;

public class EditorCheats : MonoBehaviour
{
    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.D))
        {
            SaveManager.Instance.DeleteSaveFile();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log($"Current World Progress: {UserSaveDataManager.Instance.GetCurrentWorldProgress()}");
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log($"Completed Stories: {string.Join(", ", SaveManager.Instance.CompletedStories)}");
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            var uAchievements = AchievementManager.Instance.UserAchievements.Values;

            foreach (var uAch in  uAchievements)
            {
                Debug.Log($"{uAch.AchievementId}: {uAch.CurrentCount}");
            }
        }
#endif
    }
}
