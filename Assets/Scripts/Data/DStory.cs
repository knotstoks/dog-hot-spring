using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BroccoliBunnyStudios.Pools;
using BroccoliBunnyStudios.Utils;
using UnityEngine;

[CreateAssetMenu(fileName = "DStory", menuName = "Data/DStory", order = 3)]
public class DStory : ScriptableObject, IDataImport
{
    private static DStory s_loadedData;
    private static Dictionary<string, StoryData> s_cachedDataDict;

    [field: SerializeField]
    public List<StoryData> Data { get; private set; }

    public static DStory GetAllData()
    {
        if (s_loadedData == null)
        {
            // Load and cache results
            s_loadedData = ResourceLoader.Load<DStory>("data/DStory.asset", false);

            // Calculate and cache some results
            s_cachedDataDict = new();
            foreach (var storyData in s_loadedData.Data)
            {
#if UNITY_EDITOR
                if (s_cachedDataDict.ContainsKey(storyData.StoryId))
                {
                    Debug.LogError($"Duplicate Id {storyData.StoryId}");
                }
#endif
                s_cachedDataDict[storyData.StoryId] = storyData;
            }
        }

        return s_loadedData;
    }

    public static StoryData? GetDataById(string id)
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
        text = text.Replace("\n", "|");
        text = Regex.Replace(text, pattern, "");

        // Split data into lines
        var lines = text.Split(new char[] { '\r', '|' }, StringSplitOptions.None);

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
            var worldData = new StoryData
            {
                StoryId = paramList[1],
                StoryNumber = CommonUtil.ConvertToInt32(paramList[2]),
                StoryPrefabPath = paramList[3],
            };
            s_loadedData.Data.Add(worldData);
        }

        CommonUtil.SaveScriptableObject(s_loadedData);
    }
#endif
}

[Serializable]
public struct StoryData
{
    [field: SerializeField]
    public string StoryId { get; set; }

    [field: SerializeField]
    public int StoryNumber { get; set; }

    [field: SerializeField]
    public string StoryPrefabPath { get; set; }
}
