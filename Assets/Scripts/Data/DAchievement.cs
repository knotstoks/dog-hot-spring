using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BroccoliBunnyStudios.Pools;
using BroccoliBunnyStudios.Utils;
using UnityEngine;

[CreateAssetMenu(fileName = "DAchievement", menuName = "Data/DAchievement", order = 3)]
public class DAchievement : ScriptableObject, IDataImport
{
    private static DAchievement s_loadedData;
    private static Dictionary<string, AchievementData> s_cachedDataDict;
    private static Dictionary<AchievementType, List<AchievementData>> s_cachedByTypeDataDict;

    [field: SerializeField]
    public List<AchievementData> Data { get; private set; }

    public static DAchievement GetAllData()
    {
        if (s_loadedData == null)
        {
            // Load and cache results
            s_loadedData = ResourceLoader.Load<DAchievement>("data/DAchievement.asset", false);

            // Calculate and cache some results
            s_cachedDataDict = new();
            s_cachedByTypeDataDict = new();
            foreach (var achievementData in s_loadedData.Data)
            {
#if UNITY_EDITOR
                if (s_cachedDataDict.ContainsKey(achievementData.AchievementId))
                {
                    Debug.LogError($"Duplicate Id {achievementData.AchievementId}");
                }
#endif
                s_cachedDataDict[achievementData.AchievementId] = achievementData;

                if (!s_cachedByTypeDataDict.ContainsKey(achievementData.AchievementType))
                {
                    s_cachedByTypeDataDict[achievementData.AchievementType] = new();
                }
                s_cachedByTypeDataDict[achievementData.AchievementType].Add(achievementData);
            }
        }

        return s_loadedData;
    }

    public static AchievementData? GetDataById(string id)
    {
        if (s_cachedDataDict == null)
        {
            GetAllData();
        }

        return s_cachedDataDict.TryGetValue(id, out var result) ? result : null;
    }

    public static List<AchievementData> GetDataForAchievementType(AchievementType achievementType)
    {
        if (s_cachedByTypeDataDict == null)
        {
            GetAllData();
        }

        return s_cachedByTypeDataDict.TryGetValue(achievementType, out var result) ? result : null;
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
            var worldData = new AchievementData
            {
                AchievementId = paramList[1],
                AchievementType = Enum.TryParse(paramList[2], out AchievementType achievementType) ? achievementType : AchievementType.NONE,
                Count = CommonUtil.ConvertToInt64(paramList[3]),
                Name = paramList[4],
                Description = paramList[5],
            };
            s_loadedData.Data.Add(worldData);
        }

        CommonUtil.SaveScriptableObject(s_loadedData);
    }
#endif
}

[Serializable]
public struct AchievementData
{
    [field: SerializeField]
    public string AchievementId { get; set; }

    [field: SerializeField]
    public AchievementType AchievementType { get; set; }

    [field: SerializeField]
    public long Count { get; set; }

    [field: SerializeField]
    public string Name { get; set; }

    [field: SerializeField]
    public string Description { get; set; }
}

public enum AchievementType
{
    NONE,
    LEVEL_COMPLETE,
}
