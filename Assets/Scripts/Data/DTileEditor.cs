using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(DTile))]
public class DTileEditor : MultiTextBoxEditor<DTile> { }
#endif