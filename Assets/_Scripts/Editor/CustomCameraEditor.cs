using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomCamera)) ]
public class CustomCameraEditor : Editor {

    private const string buttonName = "Align camera with target object";
    SerializedProperty cameraPivot;
    SerializedProperty followTransform;

    private void OnEnable() {
        cameraPivot = serializedObject.FindProperty(nameof(cameraPivot));
        followTransform = serializedObject.FindProperty(nameof(followTransform));
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        GUILayout.Space(10);
        if (GUILayout.Button(buttonName)) {
            Transform pivotTransform = (Transform)cameraPivot.objectReferenceValue;
            Transform targetTransform = (Transform)followTransform.objectReferenceValue;
            if (pivotTransform != null && targetTransform != null ) {
                pivotTransform.position = targetTransform.position;
                pivotTransform.eulerAngles = targetTransform.eulerAngles;
            }
            else {
                Debug.LogError("FollowTransform and/or CameraPivot reference(s) have not been assigned.");
            }
        }
    }
}
