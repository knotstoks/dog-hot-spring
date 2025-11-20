namespace BroccoliBunnyStudios.Managers
{
    public partial class SaveManager
    {
        public float MusicVolume
        {
            get => this.SaveConfig.GetFloat(nameof(this.MusicVolume), 1f);
            set => this.SaveConfig.SetFloat(nameof(this.MusicVolume), value);
        }

        public float SfxVolume
        {
            get => this.SaveConfig.GetFloat(nameof(this.SfxVolume), 1f);
            set => this.SaveConfig.SetFloat(nameof(this.SfxVolume), value);
        }
    }
}