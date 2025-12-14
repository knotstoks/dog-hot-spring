using BroccoliBunnyStudios.Utils;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(DLocale))]
public class DLocaleEditor : MultiTextBoxEditor<DLocale> { }
#endif