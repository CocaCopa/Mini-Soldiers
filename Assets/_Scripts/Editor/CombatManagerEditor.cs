using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CombatManager))]
public class CombatManagerEditor : Editor {

    SerializedProperty drawAnimations;
    SerializedProperty reloadAnimation;

    private bool animationsFoldout;

    private void OnEnable() {
        drawAnimations = serializedObject.FindProperty(nameof(drawAnimations));
        reloadAnimation = serializedObject.FindProperty(nameof(reloadAnimation));
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();
        DisplayScriptReference();
        DrawAnimationReferencesFoldout();
        EditorGUILayout.Space(10);
        DrawDefaultExcludingCustomFields();
        serializedObject.ApplyModifiedProperties();
    }

    private void DisplayScriptReference() {
        MonoBehaviour scriptComponent = target as MonoBehaviour;
        SerializedObject m_serializedObject = new SerializedObject(scriptComponent);
        SerializedProperty scriptProperty = m_serializedObject.FindProperty("m_Script");
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(scriptProperty, true, new GUILayoutOption[0]);
        EditorGUI.EndDisabledGroup();
    }

    private void DrawAnimationReferencesFoldout() {
        animationsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(animationsFoldout, "Animation References");
        if (animationsFoldout) {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(reloadAnimation);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        if (animationsFoldout) {
            EditorGUILayout.PropertyField(drawAnimations);
            EditorGUI.indentLevel--;
        }
    }

    private void DrawDefaultExcludingCustomFields() {
        string[] excludeFields = new string[4];
        excludeFields[0] = "m_Script";
        excludeFields[1] = nameof(drawAnimations);
        excludeFields[2] = nameof(reloadAnimation);
        DrawPropertiesExcluding(serializedObject, excludeFields);
    }
}
