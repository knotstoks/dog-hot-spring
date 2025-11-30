using BroccoliBunnyStudios.Pools;
using BroccoliBunnyStudios.Utils;
using ProjectRuntime.Gameplay;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[CreateAssetMenu(fileName = "DTileSprite", menuName = "Data/DTileSprite", order = 3)]
public class DTileSprite : ScriptableObject, IDataImport
{
    private static DTileSprite s_loadedData;
    private static Dictionary<TileColor, TileSpriteData> s_cachedDataDict;

    [field: SerializeField]
    public List<TileSpriteData> Data { get; private set; }

    public static DTileSprite GetAllData()
    {
        if (s_loadedData == null)
        {
            // Load and cache results
            s_loadedData = ResourceLoader.Load<DTileSprite>("data/DTileSprite.asset", false);

            // Calculate and cache some results
            s_cachedDataDict = new();
            foreach (var tileSpriteData in s_loadedData.Data)
            {
#if UNITY_EDITOR
                if (s_cachedDataDict.ContainsKey(tileSpriteData.TileColor))
                {
                    Debug.LogError($"Duplicate Id {tileSpriteData.TileColor}");
                }
#endif

                s_cachedDataDict[tileSpriteData.TileColor] = tileSpriteData;
            }
        }

        return s_loadedData;
    }

    public static string GetSpritePath(int shapeId, TileColor tileColor)
    {
        if (s_cachedDataDict == null)
        {
            GetAllData();
        }

        if (!s_cachedDataDict.ContainsKey(tileColor))
        {
            Debug.LogError($"Invalid color {tileColor}!");
            return string.Empty;
        }

        return s_cachedDataDict.TryGetValue(tileColor, out var tileSpriteData)
            ? string.Format(tileSpriteData.SpritePrefabPath, shapeId.ToString())
            : string.Empty;
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
        var pattern = @"[""]";
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
            var tileSpriteData = new TileSpriteData
            {
                TileColor = Enum.TryParse(paramList[1], out TileColor tileColor) ? tileColor : TileColor.NONE,
                SpritePrefabPath = paramList[2],
            };
            s_loadedData.Data.Add(tileSpriteData);
        }

        CommonUtil.SaveScriptableObject(s_loadedData);
    }
#endif
}

[Serializable]
public struct TileSpriteData
{
    [field: SerializeField]
    public TileColor TileColor { get; set; }

    [field: SerializeField]
    public string SpritePrefabPath { get; set; }
}
