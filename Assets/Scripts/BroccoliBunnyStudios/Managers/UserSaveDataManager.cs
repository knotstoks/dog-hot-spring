using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using static BroccoliBunnyStudios.Managers.SaveManager;

namespace BroccoliBunnyStudios.Managers
{
    public partial class UserSaveDataManager
    {
        private static readonly Lazy<UserSaveDataManager> s_lazy = new(() => new UserSaveDataManager());
        public static UserSaveDataManager Instance => s_lazy.Value;

        private Dictionary<string, int> _storyFlagDict;

        public static Action<string> OnCurrentTasksChanged;

        private UserSaveDataManager()
        {
            var sm = SaveManager.Instance;

            this._storyFlagDict = JsonConvert.DeserializeObject<Dictionary<string, int>>(sm.StoryFlags) ?? new Dictionary<string, int>();

            // User Snails
            //this._userSnails = JsonConvert.DeserializeObject<Dictionary<string, UserSnail>>(sm.UserSnails) ?? new Dictionary<string, UserSnail>();
            //this._readOnlyUserSnails = new(this._userSnails);

            //this.InitUserSnails();
        }

        public GameDifficulty GetGameDifficulty()
        {
            return SaveManager.Instance.DifficultySettings;
        }

        public void SetGameDifficulty(GameDifficulty gameDifficulty)
        {
            SaveManager.Instance.DifficultySettings = gameDifficulty;
        }

        public int GetCurrentChapter()
        {
            return SaveManager.Instance.CurrentChapter;
        }

        public void SetCurrentChapter(int chapter)
        {
            SaveManager.Instance.CurrentChapter = chapter;
        }

        public bool CheckFlag(string flag, int value)
        {
            if (this._storyFlagDict.ContainsKey(flag))
            {
                return value == this._storyFlagDict[flag];
            }
            else
            {
                return value == 0;
            }
        }

        public int GetFlag(string flag)
        {
            if (this._storyFlagDict.ContainsKey(flag))
            {
                return this._storyFlagDict[flag];
            }

            return 0;
        }

        public void SaveFlag(string flag, int value)
        {
            this._storyFlagDict[flag] = value;
            SaveManager.Instance.StoryFlags = JsonConvert.SerializeObject(this._storyFlagDict);
        }

        public List<string> GetTasks()
        {
            return SaveManager.Instance.TaskList;
        }

        public void AddTask(string task)
        {
            var lst = SaveManager.Instance.TaskList;
            lst.Add(task);
            SaveManager.Instance.TaskList = lst;

            OnCurrentTasksChanged?.Invoke(task);
        }

        public void RemoveTask(string task)
        {
            var lst = SaveManager.Instance.TaskList;
            lst.Remove(task);
            SaveManager.Instance.TaskList = lst;

            OnCurrentTasksChanged?.Invoke(task);
        }

        public void ClearStoryData()
        {
            SaveManager.Instance.CurrentChapter = 0;
            SaveManager.Instance.StoryFlags = string.Empty;
            SaveManager.Instance.TaskList = new();
            this._storyFlagDict = new();
        }

        public void ClearAllData()
        {
            SaveManager.Instance.DeleteSaveFile();
        }

        public bool HasAnySaveData()
        {
            return SaveManager.Instance.CurrentChapter > 0;
        }
    }
}