using BroccoliBunnyStudios.Pools;
using BroccoliBunnyStudios.Utils;
using ProjectRuntime.Gameplay;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[CreateAssetMenu(fileName = "DAnimal", menuName = "Data/DAnimal", order = 3)]
public class DAnimal : ScriptableObject, IDataImport
{
    private static DAnimal s_loadedData;
    private static Dictionary<TileColor, AnimalData> s_cachedDataDict;

    [field: SerializeField]
    public List<AnimalData> Data { get; private set; }

    public static DAnimal GetAllData()
    {
        if (s_loadedData == null)
        {
            // Load and cache results
            s_loadedData = ResourceLoader.Load<DAnimal>("data/DAnimal.asset", false);

            // Calculate and cache some results
            s_cachedDataDict = new();
            foreach (var animalData in s_loadedData.Data)
            {
#if UNITY_EDITOR
                if (s_cachedDataDict.ContainsKey(animalData.AnimalColor))
                {
                    Debug.LogError($"Duplicate Id {animalData.AnimalColor}");
                }
#endif
                s_cachedDataDict[animalData.AnimalColor] = animalData;
            }
        }

        return s_loadedData;
    }

    public static AnimalData? GetDataById(TileColor tileColor)
    {
        if (s_cachedDataDict == null)
        {
            GetAllData();
        }

        return s_cachedDataDict.TryGetValue(tileColor, out var result) ? result : null;
    }

#if UNITY_EDITOR
    public static void ImportData(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        s_loadedData = GetAllData();
        if (s_loadedData == null)
        {
            return;
        }

        if (s_loadedData.Data == null)
        {
            s_loadedData.Data = new();
        }
        else
        {
            s_loadedData.Data.Clear();
        }

        // special handling for shape parameter and percentage
        var pattern = @"[{}""]";
        text = text.Replace("\r\n", "\n");      // handle window line break
        text = text.Replace(",\n", ",");
        text = Regex.Replace(text, pattern, "");

        // Split data into lines
        var lines = text.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.None);
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
                paramList[j] = paramList[j].Trim();
            }

            // New item
            var worldData = new AnimalData
            {
                AnimalColor = Enum.TryParse(paramList[1], out TileColor tileColor) ? tileColor : TileColor.NONE,
                PrefabPath = paramList[2],
            };
            s_loadedData.Data.Add(worldData);
        }

        CommonUtil.SaveScriptableObject(s_loadedData);
    }
#endif
}

[Serializable]
public struct AnimalData
{
    [field: SerializeField]
    public TileColor AnimalColor { get; set; }

    [field: SerializeField]
    public string PrefabPath { get; set; }
}
