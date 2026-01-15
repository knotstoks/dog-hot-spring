using BroccoliBunnyStudios.Managers;
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

        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log($"Completed Stories: {string.Join(", ", SaveManager.Instance.CompletedStories)}");
        }
#endif
    }
}
