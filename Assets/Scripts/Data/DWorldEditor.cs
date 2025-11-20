using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(DWorld))]
public class DPuzzleEditor : MultiTextBoxEditor<DWorld> { }
#endif