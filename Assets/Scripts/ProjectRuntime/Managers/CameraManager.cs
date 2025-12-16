using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectRuntime.Managers
{
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Instance { get; private set; }

        [field: SerializeField, Header("Scene References")]
        private Camera SceneCamera { get; set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError("There are 2 or more CameraManagers in the scene");
            }
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public void SetCameraScale(int gridHeight)
        {
            this.SceneCamera.orthographicSize = (float)gridHeight / 2 + 1;
        }
    }
}