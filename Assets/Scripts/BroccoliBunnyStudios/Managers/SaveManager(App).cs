using UnityEngine;

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

        public EnumLanguage DisplayLangauge
        {
            get
            {
                // Get language the user's operating system is running in
                var systemLanguage = Application.systemLanguage;

                // Set our default language based on the system language
                EnumLanguage defaultLanguage;
                switch (systemLanguage)
                {
                    case SystemLanguage.Chinese:
                    case SystemLanguage.ChineseSimplified:
                        defaultLanguage = EnumLanguage.ZH_HANS;
                        break;

                    case SystemLanguage.Japanese:
                        defaultLanguage = EnumLanguage.JP;
                        break;

                    default:
                        defaultLanguage = EnumLanguage.EN;
                        break;
                }

                return (EnumLanguage)this.SaveConfig.GetInt(nameof(this.DisplayLangauge), (int)defaultLanguage);
            }

            set
            {
                this.SaveConfig.SetInt(nameof(this.DisplayLangauge), (int)value);
            }
        }
    }
}