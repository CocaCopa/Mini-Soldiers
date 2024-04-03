using Cinemachine.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Controller))]
public class ControllerEditor : Editor {

    SerializedProperty onGameStart;
    SerializedProperty lookAtTargetObjectSpeed;
    SerializedProperty maximumHealthPoints;
    SerializedProperty currentHealthPoints;

    public virtual void OnEnable() {
        onGameStart = serializedObject.FindProperty(nameof(onGameStart));
        lookAtTargetObjectSpeed = serializedObject.FindProperty(nameof(lookAtTargetObjectSpeed));
        maximumHealthPoints = serializedObject.FindProperty(nameof(maximumHealthPoints));
        currentHealthPoints = serializedObject.FindProperty(nameof(currentHealthPoints));
    }

    public override void OnInspectorGUI() {
        DisplayScriptReference();
        DrawControllerProperties();
        DrawCurrentHealthDisabled();
    }

    private void DisplayScriptReference() {
        MonoBehaviour scriptComponent = target as MonoBehaviour;
        SerializedObject m_serializedObject = new SerializedObject(scriptComponent);
        SerializedProperty scriptProperty = m_serializedObject.FindProperty("m_Script");
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(scriptProperty, true, new GUILayoutOption[0]);
        EditorGUI.EndDisabledGroup();
    }

    private void DrawControllerProperties() {
        serializedObject.Update();
        EditorGUILayout.PropertyField(onGameStart);
        EditorGUILayout.PropertyField(lookAtTargetObjectSpeed);
        EditorGUILayout.PropertyField(maximumHealthPoints);
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawCurrentHealthDisabled() {
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(currentHealthPoints);
        EditorGUI.EndDisabledGroup();
    }
}
