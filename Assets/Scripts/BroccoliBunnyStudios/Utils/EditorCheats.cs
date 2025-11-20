using System.Collections;
using System.Collections.Generic;
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
#endif
    }
}
