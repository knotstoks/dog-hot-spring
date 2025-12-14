using System;
using System.Collections.Generic;
using UnityEngine;
using BroccoliBunnyStudios.Utils;
using BroccoliBunnyStudios.Pools;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "DLocale", menuName = "Data/DLocale", order = 3)]
public class DLocale : ScriptableObject, IDataImport
{
    [field: SerializeField]
    public EnumLanguage Language { get; private set; }

    [field: SerializeField]
    public List<LocalizationData> Data { get; private set; }

    public static Dictionary<string, string> GetAllData(EnumLanguage lang)
    {
        var lowerLang = lang.ToString().ToLowerInvariant();

        // Load
        var so = ResourceLoader.Load<DLocale>($"data/dlocale_{lowerLang}.asset", false);

        // Put everything into a dict
        var lookup = new Dictionary<string, string>();
        foreach (var dLocale in so.Data)
        {
#if UNITY_EDITOR
            if (lookup.ContainsKey(dLocale.Key))
            {
                Debug.LogError($"Duplicate Id {dLocale.Key}");
            }
#endif
            lookup[dLocale.Key] = dLocale.Value;
        }

        // Unload, we don't need it anymore
        ResourceLoader.Unload(so);

        return lookup;
    }

#if UNITY_EDITOR
    public static void ImportData(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        var languages = (EnumLanguage[])Enum.GetValues(typeof(EnumLanguage));
        var locales = new DLocale[languages.Length];

        // Load each locale file
        for (var i = 0; i < languages.Length; i++)
        {
            var lowerLang = languages[i].ToString().ToLowerInvariant();
            var path = $"Assets/GameAssets/data/DLocale_{lowerLang}.asset";
            locales[i] = AssetDatabase.LoadAssetAtPath<DLocale>(path);
            if (locales[i] == null)
            {
                locales[i] = CreateInstance<DLocale>();
                AssetDatabase.CreateAsset(locales[i], path);
            }

            // Reset the data
            locales[i].Language = languages[i];
            if (locales[i].Data == null)
            {
                locales[i].Data = new List<LocalizationData>();
            }
            else
            {
                locales[i].Data.Clear();
            }
        }

        text = text.Replace("\r\n", "\n");      // handle window line break

        // Split data into lines
        var lines = text.Split(new char[] { '\r', '\n' }, StringSplitOptions.None);
        for (var i = 0; i < lines.Length; i++)
        {
            // Comment and Header
            if (lines[i][0].Equals('#') || lines[i][0].Equals('$'))
            {
                continue;
            }

            // Empty line
            var trimLine = lines[i].Trim();
            var testList = trimLine.Split('\t');
            if (testList.Length == 1 && string.IsNullOrEmpty(testList[0]))
            {
                continue;
            }

            // Split
            var paramList = lines[i].Split('\t');
            for (var j = 0; j < paramList.Length; j++)
            {
                paramList[j] = paramList[j].Replace("\\n", "\n");  // handle manual line breaks
                paramList[j] = paramList[j].Trim();
            }

            // New item for each language
            for (var j = 0; j < languages.Length; j++)
            {
                var dLocale = new LocalizationData
                {
                    Key = paramList[1],
                    Value = paramList[j + 2],
                };

                locales[j].Data.Add(dLocale);
            }
        }

        // Save each locale file
        for (var i = 0; i < languages.Length; i++)
        {
            EditorUtility.SetDirty(locales[i]);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("ScriptableObject updated and saved!");
    }
#endif
}

public enum EnumLanguage
{
    EN = 0,      // English
    ZH_HANS = 1, // Simplified Chinese
    JP = 2,      // Japanese
}

[Serializable]
public struct LocalizationData
{
    [field: SerializeField]
    public string Key { get; set; }

    [field: SerializeField]
    public string Value { get; set; }
}