using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(DStory))]
public class DStoryEditor : MultiTextBoxEditor<DStory> { }
#endif