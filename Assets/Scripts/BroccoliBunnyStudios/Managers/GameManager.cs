using System;
using BroccoliBunnyStudios.Extensions;
using BroccoliBunnyStudios.Managers;
using ProjectRuntime.Managers;
using UnityEngine;
using Object = UnityEngine.Object;

public class GameManager
{
    private static readonly Lazy<GameManager> s_lazy = new(() => new GameManager());

    private GameManager()
    {
        this.CreateGameObject();
    }

    public static GameManager Instance => s_lazy.Value;
    public Transform Root { get; private set; }

    private void CreateGameObject()
    {
        var go = new GameObject(nameof(GameManager));
        if (Application.isPlaying)
        {
            Object.DontDestroyOnLoad(go);
        }

        this.Root = go.transform;
        UserSaveDataManager.Instance.InitUserSaveDataManager();
        AchievementManager.Instance.TriggerAnyAchievements();
#if UNITY_EDITOR
        go.FGetComp<EditorCheats>();
#endif
    }
}
