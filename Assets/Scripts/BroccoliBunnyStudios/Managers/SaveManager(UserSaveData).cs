using System.Collections.Generic;

namespace BroccoliBunnyStudios.Managers
{
    public partial class SaveManager
    {
        /// <summary>
        /// This tracks what version number the player's save is
        /// </summary>
        public int VersionNumber
        {
            get => this.SaveConfig.GetInt(nameof(this.VersionNumber), 0);
            set => this.SaveConfig.SetInt(nameof(this.VersionNumber), value);
        }

        /// <summary>
        /// This returns the latest world the player has completed
        /// </summary>
        public int CurrentLevelProgress
        {
            get => this.SaveConfig.GetInt(nameof(this.CurrentLevelProgress), 0);
            set => this.SaveConfig.SetInt(nameof(this.CurrentLevelProgress), value);
        }

        public List<string> CompletedStories
        {
            get => this.SaveConfig.GetCollection<List<string>, string>(nameof(this.CompletedStories), new());
            set => this.SaveConfig.SetCollection<List<string>, string>(nameof(this.CompletedStories), value);
        }

        public List<string> CompletedTutorials
        {
            get => this.SaveConfig.GetCollection<List<string>, string>(nameof(this.CompletedTutorials), new());
            set => this.SaveConfig.SetCollection<List<string>, string>(nameof(this.CompletedTutorials), value);
        }

        public string UserAchievements
        {
            get => this.SaveConfig.GetString(nameof(this.UserAchievements), string.Empty);
            set => this.SaveConfig.SetString(nameof(this.UserAchievements), value);
        }
    }
}