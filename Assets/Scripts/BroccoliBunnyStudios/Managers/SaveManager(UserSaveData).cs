using System.Collections.Generic;

namespace BroccoliBunnyStudios.Managers
{
    public partial class SaveManager
    {
        public enum GameDifficulty
        {
            Easy = 0,
            Normal = 1,
            Hard = 2,
        }

        public GameDifficulty DifficultySettings
        {
            get => (GameDifficulty)this.SaveConfig.GetInt(nameof(this.DifficultySettings), (int)GameDifficulty.Easy);
            set => this.SaveConfig.SetInt(nameof(this.DifficultySettings), (int)value);
        }

        public int CurrentChapter
        {
            get => this.SaveConfig.GetInt(nameof(this.CurrentChapter), 0);
            set => this.SaveConfig.SetInt(nameof(this.CurrentChapter), value);
        }

        public string StoryFlags
        {
            get => this.SaveConfig.GetString(nameof(this.StoryFlags), string.Empty);
            set => this.SaveConfig.SetString(nameof(this.StoryFlags), value);
        }

        public List<string> TaskList
        {
            get => this.SaveConfig.GetCollection<List<string>, string>(nameof(this.TaskList), new());
            set => this.SaveConfig.SetCollection<List<string>, string>(nameof(this.TaskList), value);
        }

        public bool HasCompletedTutorial
        {
            get => this.SaveConfig.GetBool(nameof(this.HasCompletedTutorial), false);
            set => this.SaveConfig.SetBool(nameof(this.HasCompletedTutorial), value);
        }
    }
}