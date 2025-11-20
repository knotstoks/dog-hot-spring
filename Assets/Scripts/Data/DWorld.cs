using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BroccoliBunnyStudios.Pools;
using BroccoliBunnyStudios.Utils;
using UnityEngine;

[CreateAssetMenu(fileName = "DWorld", menuName = "Data/DWorld", order = 3)]
public class DWorld : ScriptableObject, IDataImport
{
    private static DWorld s_loadedData;
    private static Dictionary<int, WorldData> s_cachedDataDict;

    [field: SerializeField]
    public List<WorldData> Data { get; private set; }

    public static DWorld GetAllData()
    {
        if (s_loadedData == null)
        {
            // Load and cache results
            s_loadedData = ResourceLoader.Load<DWorld>("data/DWorld.asset", false);

            // Calculate and cache some results
            s_cachedDataDict = new();
            foreach (var worldData in s_loadedData.Data)
            {
#if UNITY_EDITOR
                if (s_cachedDataDict.ContainsKey(worldData.WorldId))
                {
                    Debug.LogError($"Duplicate Id {worldData.WorldId}");
                }
#endif
                s_cachedDataDict[worldData.WorldId] = worldData;
            }
        }

        return s_loadedData;
    }

    public static WorldData? GetDataById(int id)
    {
        if (s_cachedDataDict == null)
        {
            GetAllData();
        }

        return s_cachedDataDict.TryGetValue(id, out var result) ? result : null;
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
            var worldData = new WorldData
            {
                WorldId = CommonUtil.ConvertToInt32(paramList[1]),
                WorldName = paramList[2],
                ParsedLevel = paramList[3],
            };
            s_loadedData.Data.Add(worldData);
        }

        CommonUtil.SaveScriptableObject(s_loadedData);
    }
#endif
}

[Serializable]
public struct WorldData
{
    [field: SerializeField]
    public int WorldId { get; set; }

    [field: SerializeField]
    public string WorldName { get; set; }

    [field: SerializeField]
    public string ParsedLevel { get; set; }
}
