using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(DAnimal))]
public class DAnimalEditor : MultiTextBoxEditor<DAnimal> { }
#endif