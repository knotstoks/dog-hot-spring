using System;
using System.Collections.Generic;

namespace BroccoliBunnyStudios.Managers
{
    public partial class UserSaveDataManager
    {
        private static readonly Lazy<UserSaveDataManager> s_lazy = new(() => new UserSaveDataManager());
        public static UserSaveDataManager Instance => s_lazy.Value;

        // Internal variables
        private readonly List<string> _completedStories;
        private readonly List<string> _completedTutorials;

        // Callbacks
        public event Action<int> OnUserWorldProgressModified;
        public event Action<string> OnUserStoryProgressModified;
        public event Action<string> OnTutorialProgressModified;

        private UserSaveDataManager()
        {
            var sm = SaveManager.Instance;

            this._completedStories = sm.CompletedStories;
            this._completedTutorials = sm.CompletedTutorials;

            //JsonConvert.DeserializeObject<Dictionary<string, int>>(sm.StoryFlags) ?? new Dictionary<string, int>();
            // User Snails
            //this._userSnails = JsonConvert.DeserializeObject<Dictionary<string, UserSnail>>(sm.UserSnails) ?? new Dictionary<string, UserSnail>();
            //this._readOnlyUserSnails = new(this._userSnails);

            //this.InitUserSnails();
        }

        /// <summary>
        /// This function is to trigger the constructor from GameManager
        /// </summary>
        public void InitUserSaveDataManager()
        {
            // Empty for now
        }

        #region Progression
        public int GetCurrentWorldProgress()
        {
            return SaveManager.Instance.CurrentLevelProgress;
        }

        public void SetCurrentWorldProgress(int levelNumber)
        {
            SaveManager.Instance.CurrentLevelProgress = levelNumber;
            this.OnUserWorldProgressModified?.Invoke(levelNumber);
        }

        public void ResetCurrentWorldProgress()
        {
            SaveManager.Instance.CurrentLevelProgress = 0;
            this.OnUserWorldProgressModified?.Invoke(0);
        }

        public bool IsWorldRequirementMet(int worldNumber)
        {
            return worldNumber >= SaveManager.Instance.CurrentLevelProgress;
        }
        #endregion

        #region Stories
        public void RegisterStory(string storyId)
        {
            if (this._completedStories.Contains(storyId))
            {
                return;
            }
            this._completedStories.Add(storyId);
            this.SaveStories();
        }

        public bool HasSeenStory(string storyId)
        {
            return this._completedStories.Contains(storyId);
        }

        public void ResetAllStories()
        {
            this._completedStories.Clear();
            this.SaveStories();
        }

        private void SaveStories()
        {
            SaveManager.Instance.CompletedStories = this._completedStories;
        }
        #endregion

        #region Tutorials
        public void RegisterTutorial(string tutorialId)
        {
            if (this._completedTutorials.Contains(tutorialId))
            {
                return;
            }
            this._completedTutorials.Add(tutorialId);
            this.SaveTutorials();
        }

        public bool HasCompletedTutorial(string tutorialId)
        {
            return this._completedTutorials.Contains(tutorialId);
        }

        public void ResetAllTutorials()
        {
            this._completedTutorials.Clear();
            this.SaveTutorials();
        }

        private void SaveTutorials()
        {
            SaveManager.Instance.CompletedTutorials = this._completedTutorials;
        }
        #endregion

        public void ClearAllData()
        {
            SaveManager.Instance.DeleteSaveFile();
        }

        public bool HasAnySaveData()
        {
            return SaveManager.Instance.CurrentLevelProgress > 0;
        }
    }
}