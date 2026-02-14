using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TMPro;

namespace BroccoliBunnyStudios.Managers
{
    public class LocalizationManager
    {
        // Singleton
        private static readonly Lazy<LocalizationManager> s_lazy = new(() => new LocalizationManager());
        public static LocalizationManager Instance => s_lazy.Value;

        // Acessible variables
        public ReadOnlyDictionary<string, string> LocaleDict => this._readOnlyCurrent;
        public event Action OnLocalizationChanged;

        // Internal variables
        private Dictionary<string, string> _current;
        private ReadOnlyDictionary<string, string> _readOnlyCurrent;

        private LocalizationManager()
        {
            var lang = SaveManager.Instance.DisplayLangauge;
            this._current = DLocale.GetAllData(lang);
            this._readOnlyCurrent = new(this._current);

            // Update fallback font
            this.UpdateFallbackFont(lang);
        }

        public string this[string key]
        {
            get
            {
                return this._current.TryGetValue(key, out var value) ? (string.IsNullOrEmpty(value) ? key : value) : key;
            }
        }

        public EnumLanguage GetLanguage()
        {
            return SaveManager.Instance.DisplayLangauge;
        }

        public void SetLanguage(EnumLanguage lang)
        {
            var currentLanguage = SaveManager.Instance.DisplayLangauge;
            if (lang == currentLanguage)
            {
                return;
            }

            // Save language
            SaveManager.Instance.DisplayLangauge = lang;

            // Update dictionary
            this._current = DLocale.GetAllData(lang);
            this._readOnlyCurrent = new(this._current);

            // Update fallback font
            this.UpdateFallbackFont(lang);

            OnLocalizationChanged?.Invoke();
        }

        private void UpdateFallbackFont(EnumLanguage lang)
        {
            // Maybe should rewrite with a custom scriptable object referencing each fallback font
            var searchString = string.Empty;
            switch (lang)
            {
                case EnumLanguage.EN:
                case EnumLanguage.ZH_HANS:
                    searchString = "SC";
                    break;
                case EnumLanguage.JP:
                    searchString = "JP";
                    break;
                default:
                    break;
            }

            // Find the index of the font in the list containing this search string
            var fallbackFonts = TMP_Settings.fallbackFontAssets;
            var index = 0;
            for (var i = 0; i < fallbackFonts.Count; i++)
            {
                if (fallbackFonts[i].name.Contains(searchString))
                {
                    index = i;
                    break;
                }
            }

            // Make sure this fallback font is in front
            if (index > 0)
            {
                var fallbackFont = fallbackFonts[index];
                fallbackFonts.RemoveAt(index);
                fallbackFonts.Insert(0, fallbackFont);
            }
        }
    }
}