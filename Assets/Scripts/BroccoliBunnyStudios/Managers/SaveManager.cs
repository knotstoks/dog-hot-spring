using System;
using System.IO;
using BroccoliBunnyStudios.Save;
using UnityEngine;

namespace BroccoliBunnyStudios.Managers
{
    public partial class SaveManager
    {
        private static readonly Lazy<SaveManager> s_lazy = new(() => new SaveManager());
        private SaveConfigDb _saveConfig;

        private SaveManager()
        {
            // Nothing for now
        }

        public static SaveManager Instance => s_lazy.Value;
        private SaveConfigDb SaveConfig =>
            this._saveConfig ??= new SaveConfigDb($"{Application.persistentDataPath}/app.settings");

        public void DeleteSaveFile()
        {
            File.Delete($"{Application.persistentDataPath}/app.settings");
            this._saveConfig = null;
        }
    }
}