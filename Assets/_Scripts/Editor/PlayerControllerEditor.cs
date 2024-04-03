using UnityEditor;

[CustomEditor(typeof(PlayerController))]
public class PlayerControllerEditor : ControllerEditor {

    SerializedProperty maxLookUpAngle;

    public override void OnEnable() {
        base.OnEnable();
        maxLookUpAngle = serializedObject.FindProperty(nameof(maxLookUpAngle));
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(maxLookUpAngle);
    }
}
