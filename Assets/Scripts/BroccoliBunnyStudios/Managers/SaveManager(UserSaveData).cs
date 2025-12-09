using System.Collections.Generic;

namespace BroccoliBunnyStudios.Managers
{
    public partial class SaveManager
    {
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
    }
}