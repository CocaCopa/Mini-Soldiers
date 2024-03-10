using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomCamera)) ]
public class CustomCameraEditor : Editor {

    private const string buttonName = "Align rotation with target";
    SerializedProperty m_Camera;
    SerializedProperty cameraPivot;
    SerializedProperty followTransform;
    SerializedProperty lookAtOffset;
    SerializedProperty followOffset;
    SerializedProperty stabilizeCameraLookAt;

    private bool referencesFoldout;

    private void OnEnable() {
        m_Camera = serializedObject.FindProperty(nameof(m_Camera));
        cameraPivot = serializedObject.FindProperty(nameof(cameraPivot));
        followTransform = serializedObject.FindProperty(nameof(followTransform));
        lookAtOffset = serializedObject.FindProperty(nameof(lookAtOffset));
        followOffset = serializedObject.FindProperty(nameof(followOffset));
        stabilizeCameraLookAt = serializedObject.FindProperty(nameof(stabilizeCameraLookAt));
    }

    public override void OnInspectorGUI() {
        DisplayScriptReference();
        serializedObject.Update();
        referencesFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(referencesFoldout, "References");
        if (referencesFoldout) {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_Camera);
            EditorGUILayout.PropertyField(followTransform);
            EditorGUILayout.PropertyField(cameraPivot);
            EditorGUI.indentLevel--;
        }
        DrawDefaultExcludingCustomFields();
        serializedObject.ApplyModifiedProperties();
        GUILayout.Space(15);
        Button_AllignRotationWithTarget();
        GUILayout.Space(10);
    }

    private void DisplayScriptReference() {
        MonoBehaviour scriptComponent = target as MonoBehaviour;
        SerializedObject m_serializedObject = new SerializedObject(scriptComponent);
        SerializedProperty scriptProperty = m_serializedObject.FindProperty("m_Script");
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(scriptProperty, true, new GUILayoutOption[0]);
        EditorGUI.EndDisabledGroup();
    }

    private void DrawDefaultExcludingCustomFields() {
        string[] excludeFields = new string[4];
        excludeFields[0] = "m_Script";
        excludeFields[1] = nameof(m_Camera);
        excludeFields[2] = nameof(followTransform);
        excludeFields[3] = nameof(cameraPivot);
        DrawPropertiesExcluding(serializedObject, excludeFields);
    }

    private void Button_AllignRotationWithTarget() {
        if (GUILayout.Button(buttonName)) {
            Transform pivotTransform = (Transform)cameraPivot.objectReferenceValue;
            Transform targetTransform = (Transform)followTransform.objectReferenceValue;
            if (pivotTransform != null && targetTransform != null) {
                pivotTransform.eulerAngles = targetTransform.eulerAngles;
            }
            else {
                Debug.LogError("FollowTransform and/or CameraPivot reference(s) have not been assigned.");
            }
        }
    }
}
