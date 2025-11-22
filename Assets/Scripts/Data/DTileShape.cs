/*
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using XiiRuntime.Application.Manager.Resource;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProjectRuntime.Data
{
    [CreateAssetMenu(fileName = "DTileShape", menuName = "Catchef/DTileShape", order = 4)]
    public class DTileShape : ScriptableObject
    {
        private static DTileShape s_loadedData;
        private static Dictionary<int, TileShapeData> s_cachedDataDict;

        [field: SerializeField, ListDrawerSettings(NumberOfItemsPerPage = 5)]
        public List<TileShapeData> Data { get; private set; }

        public static DTileShape GetAllData()
        {
            if (s_loadedData == null)
            {
                // Load and cache results
                s_loadedData = ResourceLoader.Load<DTileShape>($"data/DTileShape.asset", false);

                // Calculate and cache some results
                s_cachedDataDict = new Dictionary<int, TileShapeData>();
                foreach (var dWeaponShape in s_loadedData.Data)
                {
#if UNITY_EDITOR
                    if (s_cachedDataDict.ContainsKey(dWeaponShape.WeaponShapeId))
                    {
                        Debug.LogError($"More than 1 WeaponShape has WeaponShapeID {dWeaponShape.WeaponShapeId}");
                    }
#endif
                    s_cachedDataDict[dWeaponShape.WeaponShapeId] = dWeaponShape;
                }
            }
            return s_loadedData;
        }

        public static TileShapeData? GetDataById(int weaponShapeId)
        {
            if (s_cachedDataDict == null)
            {
                GetAllData();
            }
            return s_cachedDataDict.TryGetValue(weaponShapeId, out var result) ? result : null;
        }

        #region Inspector buttons
        [Button("Toggle Weapon Shape Editing"), PropertyOrder(-10)]
        private void ToggleEditButtons()
        {
            WeaponShape.ShowEditButtons = !WeaponShape.ShowEditButtons;
        }

        [Button(SdfIconType.CalendarPlus, "Add New WeaponShape", IconAlignment = IconAlignment.LeftEdge)]
        private void AddNewWeaponShape()
        {
            var newId = (this.Data.Count > 0) ? this.Data[^1].WeaponShapeId + 1 : 1;
            var dWeaponShape = new TileShapeData(newId);
            this.Data.Add(dWeaponShape);
        }

        [Button(SdfIconType.SortNumericUp, "Sort WeaponShapes by ID", IconAlignment = IconAlignment.LeftEdge)]
        private void SortWeaponShapesById()
        {
            this.Data.Sort(TileShapeData.CompareTo);
        }

        [Button(SdfIconType.Check, "Check for Duplicate Shapes", IconAlignment = IconAlignment.LeftEdge)]
        private void CheckForDuplicateShapes()
        {
            for (var index = 0; index < this.Data.Count - 1; index++)
            {
                for (var index2 = index + 1; index2 < this.Data.Count; index2++)
                {
                    if (this.Data[index].WeaponShape.EqualShapeWith(this.Data[index2].WeaponShape))
                    {
                        Debug.LogError($"WeaponShape Id {this.Data[index].WeaponShapeId} and {this.Data[index2].WeaponShapeId} have the same shape");
                    }
                }
            }
            Debug.Log("Check completed");
        }

        [Button(SdfIconType.Check, "Check for Duplicate IDs", IconAlignment = IconAlignment.LeftEdge)]
        private void CheckForDuplicateIds()
        {
            var usedIds = new Dictionary<int, int>();
            for (var index = 0; index < this.Data.Count; index++)
            {
                var id = this.Data[index].WeaponShapeId;
                if (usedIds.TryGetValue(id, out var prevIndex))
                {
                    Debug.LogError($"WeaponShape at index {prevIndex} and {index} both specify ID {id}");
                }
                else
                {
                    usedIds.Add(id, index);
                }
            }
            Debug.Log("Check completed");
        }
        #endregion
    }

    [Serializable]
    public struct TileShapeData
    {
        [field: SerializeField, Min(0), Header("WeaponShape data")]
        public int WeaponShapeId { get; private set; }

        [field: SerializeField]
        public string ShapeIcon { get; private set; }

        [field: SerializeField]
        public string BlueprintItemId { get; private set; }

        [field: SerializeField]
        public WeaponShape WeaponShape { get; private set; }

        public TileShapeData(int id)
        {
            this.WeaponShapeId = id;
            this.ShapeIcon = string.Empty;
            this.BlueprintItemId = string.Empty;
            this.WeaponShape = new(4, 4);
        }

        public static int CompareTo(TileShapeData lhs, TileShapeData rhs)
        {
            return lhs.WeaponShapeId.CompareTo(rhs.WeaponShapeId);
        }
    }

    /// <summary>
    /// This class is a wrapper for a row of cells within a weapon shape 2d array
    /// </summary>
    [Serializable]
    public struct RowWrapper
    {
        [field: SerializeField]
        public bool[] Cols { get; private set; }

        public RowWrapper(int width)
        {
            this.Cols = new bool[width];
        }

        // Define indexer to shortcut access Cols property
        public bool this[int i]
        {
            get { return this.Cols[i]; }
            set { this.Cols[i] = value; }
        }
    }

    /// <summary>
    /// This class represents a weapon shape
    /// </summary>
    [Serializable]
    public struct WeaponShape
    {
        public static bool ShowEditButtons = false;

        [field: SerializeField]
        public int Width { get; private set; }

        [field: SerializeField]
        public int Height { get; private set; }

        [field: SerializeField]
        public RowWrapper[] Rows { get; private set; }

        public WeaponShape(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this.Rows = new RowWrapper[height];
            for (var i = 0; i < height; i++)
            {
                this.Rows[i] = new RowWrapper(width);
            }
        }

        // Define indexer to shortcut access Rows property
        public RowWrapper this[int i]
        {
            get { return this.Rows[i]; }
            set { this.Rows[i] = value; }
        }

        /// <summary>
        /// Returns true if the width, height and shape is the same as the input WeaponShape
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EqualShapeWith(WeaponShape other)
        {
            if (this.Width != other.Width || this.Height != other.Height)
            {
                return false;
            }

            for (var y = 0; y < this.Width; y++)
            {
                for (var x = 0; x < this.Height; x++)
                {
                    if (this.Rows[x][y] != other.Rows[x][y])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public List<Vector2Int> GetGridInfoList()
        {
            List<Vector2Int> list = new();
            for (var y = 0; y < this.Width; y++)
            {
                for (var x = 0; x < this.Height; x++)
                {
                    if (this.Rows[x][y])
                    {
                        list.Add(new Vector2Int(x, y));
                    }
                }
            }

            return list;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(WeaponShape))]
    public class WeaponShapeInspector : PropertyDrawer
    {
        private GUIStyle _buttonStyle = null;
        private GUIStyle _textCenterStyle = null;
        private Color _onColor = new(0f, 1f, 0f, 0.5f);
        private Color _offColor = new(0.1f, 0.1f, 0.1f, 0.5f);

        private const float GridSize = 30f;
        private const float RowLabelWidth = 20f;

        private void InitStyles()
        {
            if (this._buttonStyle == null)
            {
                this._buttonStyle = new(GUI.skin.box);
                this._buttonStyle.normal.background = Texture2D.whiteTexture;

                this._textCenterStyle = new(GUI.skin.GetStyle("label"));
                this._textCenterStyle.alignment = TextAnchor.MiddleCenter;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var heightProp = property.FindPropertyRelative("<Height>k__BackingField");
            var baseHeight = base.GetPropertyHeight(property, label);
            var additionalHeight = heightProp.intValue * GridSize + baseHeight;
            if (!WeaponShape.ShowEditButtons)
            {
                additionalHeight -= baseHeight;
            }
            return baseHeight + additionalHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            this.InitStyles();

            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            var widthProp = property.FindPropertyRelative("<Width>k__BackingField");
            var heightProp = property.FindPropertyRelative("<Height>k__BackingField");
            var rowsProp = property.FindPropertyRelative("<Rows>k__BackingField");
            var width = widthProp.intValue;
            var height = heightProp.intValue;

            var rect = new Rect(position.x, position.y, 70f, 15f);
            if (WeaponShape.ShowEditButtons && GUI.Button(rect, "Add row"))
            {
                heightProp.intValue += 1;
                height += 1;

                // Insert row
                rowsProp.InsertArrayElementAtIndex(height - 1);

                // Insert elements into the new row
                var rowProp = rowsProp.GetArrayElementAtIndex(height - 1);
                var colsProp = rowProp.FindPropertyRelative("<Cols>k__BackingField");
                colsProp.ClearArray();  // Remove elements of the cloned row
                for (var i = 0; i < width; i++)
                {
                    colsProp.InsertArrayElementAtIndex(0);
                }
            }

            rect = new Rect(position.x + 70f, position.y, 70f, 15f);
            if (WeaponShape.ShowEditButtons && GUI.Button(rect, "Add col"))
            {
                widthProp.intValue += 1;
                width += 1;
                // For each row, add element at the end
                for (var i = 0; i < height; i++)
                {
                    var rowProp = rowsProp.GetArrayElementAtIndex(i);
                    var colsProp = rowProp.FindPropertyRelative("<Cols>k__BackingField");
                    colsProp.InsertArrayElementAtIndex(width - 1);
                    var colProp = colsProp.GetArrayElementAtIndex(width - 1);
                    colProp.boolValue = false;
                }
            }

            rect = new Rect(position.x + 140f, position.y, 70f, 15f);
            if (WeaponShape.ShowEditButtons && GUI.Button(rect, "Shrink"))
            {
                // For each row from the bottom, check if it is completely empty
                for (var x = 0; x < height; x++)
                {
                    var rowProp = rowsProp.GetArrayElementAtIndex(x);
                    var colsProp = rowProp.FindPropertyRelative("<Cols>k__BackingField");
                    var anyTrue = false;
                    for (var y = 0; y < width; y++)
                    {
                        var colProp = colsProp.GetArrayElementAtIndex(y);
                        if (colProp.boolValue)
                        {
                            anyTrue = true;
                            break;
                        }
                    }

                    if (anyTrue)
                    {
                        break;
                    }
                    else
                    {
                        // Remove this row, entire row is all false
                        heightProp.intValue -= 1;
                        height -= 1;
                        rowsProp.DeleteArrayElementAtIndex(x);
                        x -= 1;
                    }
                }

                // For each row from the top, check if it is completely empty
                for (var x = height - 1; x >= 0; x--)
                {
                    var rowProp = rowsProp.GetArrayElementAtIndex(x);
                    var colsProp = rowProp.FindPropertyRelative("<Cols>k__BackingField");
                    var anyTrue = false;
                    for (var y = 0; y < width; y++)
                    {
                        var colProp = colsProp.GetArrayElementAtIndex(y);
                        if (colProp.boolValue)
                        {
                            anyTrue = true;
                            break;
                        }
                    }

                    if (anyTrue)
                    {
                        break;
                    }
                    else
                    {
                        // Remove this row, entire row is all false
                        heightProp.intValue -= 1;
                        height -= 1;
                        rowsProp.DeleteArrayElementAtIndex(x);
                    }
                }

                // For each col from the left, check if it is completely empty
                for (var y = 0; y < width; y++)
                {
                    var anyTrue = false;
                    for (var x = 0; x < height; x++)
                    {
                        var rowProp = rowsProp.GetArrayElementAtIndex(x);
                        var colsProp = rowProp.FindPropertyRelative("<Cols>k__BackingField");
                        var colProp = colsProp.GetArrayElementAtIndex(y);
                        if (colProp.boolValue)
                        {
                            anyTrue = true;
                            break;
                        }
                    }

                    if (anyTrue)
                    {
                        break;
                    }
                    else
                    {
                        widthProp.intValue -= 1;
                        width -= 1;
                        for (var x = 0; x < height; x++)
                        {
                            var rowProp = rowsProp.GetArrayElementAtIndex(x);
                            var colsProp = rowProp.FindPropertyRelative("<Cols>k__BackingField");
                            colsProp.DeleteArrayElementAtIndex(y);
                        }
                        y -= 1;
                    }
                }

                // For each col from the right, check if it is completely empty
                for (var y = width - 1; y >= 0; y--)
                {
                    var anyTrue = false;
                    for (var x = 0; x < height; x++)
                    {
                        var rowProp = rowsProp.GetArrayElementAtIndex(x);
                        var colsProp = rowProp.FindPropertyRelative("<Cols>k__BackingField");
                        var colProp = colsProp.GetArrayElementAtIndex(y);
                        if (colProp.boolValue)
                        {
                            anyTrue = true;
                            break;
                        }
                    }

                    if (anyTrue)
                    {
                        break;
                    }
                    else
                    {
                        widthProp.intValue -= 1;
                        width -= 1;
                        for (var x = 0; x < height; x++)
                        {
                            var rowProp = rowsProp.GetArrayElementAtIndex(x);
                            var colsProp = rowProp.FindPropertyRelative("<Cols>k__BackingField");
                            colsProp.DeleteArrayElementAtIndex(y);
                        }
                    }
                }
            }

            // Draw grid
            var origColor = GUI.color;

            // Column Y-label
            for (var y = 0; y < width; y++)
            {
                var offsetX = y * GridSize + RowLabelWidth;
                var offsetY = EditorGUIUtility.singleLineHeight;
                if (!WeaponShape.ShowEditButtons)
                {
                    offsetY -= EditorGUIUtility.singleLineHeight;
                }
                rect = new Rect(position.x + offsetX, position.y + offsetY, 29f, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(rect, y.ToString(), this._textCenterStyle);
            }

            var rowProp2 = height > 0 ? rowsProp.GetArrayElementAtIndex(0) : null;
            for (var x = 0; x < height; x++, rowProp2.Next(false))
            {
                var colsProp = rowProp2.FindPropertyRelative("<Cols>k__BackingField");
                var offsetY = (height - x - 1) * GridSize + EditorGUIUtility.singleLineHeight * 2;
                if (!WeaponShape.ShowEditButtons)
                {
                    offsetY -= EditorGUIUtility.singleLineHeight;
                }

                // Row X-label
                rect = new Rect(position.x, position.y + offsetY, RowLabelWidth, GridSize);
                EditorGUI.LabelField(rect, x.ToString(), this._textCenterStyle);

                var colProp = width > 0 ? colsProp.GetArrayElementAtIndex(0) : null;
                for (var y = 0; y < width; y++, colProp.Next(false))
                {
                    var offsetX = y * GridSize + RowLabelWidth;
                    rect = new Rect(position.x + offsetX, position.y + offsetY, GridSize - 1f, GridSize - 1f);
                    if (WeaponShape.ShowEditButtons)
                    {
                        GUI.color = colProp.boolValue ? this._onColor : this._offColor;
                        if (GUI.Button(rect, "", this._buttonStyle))
                        {
                            colProp.boolValue = !colProp.boolValue;
                        }
                        GUI.color = origColor;
                    }
                    else
                    {
                        EditorGUI.DrawRect(rect, colProp.boolValue ? this._onColor : this._offColor);
                    }
                }
            }

            EditorGUI.EndProperty();
        }
    }
#endif
}
*/