using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(AIController))]
public class AIControllerEditor : Editor {

    private const string buttonName = "Assign Hide Spots";

    SerializedProperty hideSpots;
    private bool hideSpotsHelper = false;
    private Transform hideSpotsHolderTransform;

    private void OnEnable() {
        hideSpots = serializedObject.FindProperty(nameof(hideSpots));
        LoadInspectorValues();
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        GUILayout.Space(10);
        EditorGUI.BeginChangeCheck();
        hideSpotsHelper = EditorGUILayout.Toggle("Hide Spots Helper", hideSpotsHelper);
        if (hideSpotsHelper) {
            hideSpotsHolderTransform = EditorGUILayout.ObjectField("Hide Spots Holder", hideSpotsHolderTransform, typeof(Transform), true) as Transform;
            if (GUILayout.Button(buttonName)) {
                AssignHideSpots();
            }
        }
        if (EditorGUI.EndChangeCheck()) {
            SaveInspectorValues();
        }
    }

    private void AssignHideSpots() {
        // Clear the existing hide spots
        hideSpots.ClearArray();

        // Iterate through each hide spot in the holder and assign them to the serialized property
        for (int i = 0; i < hideSpotsHolderTransform.childCount; i++) {
            Transform hideSpot = hideSpotsHolderTransform.GetChild(i);
            hideSpots.InsertArrayElementAtIndex(i);
            hideSpots.GetArrayElementAtIndex(i).objectReferenceValue = hideSpot;
        }

        // Apply modifications
        serializedObject.ApplyModifiedProperties();
    }

    private string GetTransformPath(Transform transform) {
        if (transform == null) {
            return "";
        }
        return GetTransformPath(transform.parent) + "/" + transform.name;
    }

    private void SaveInspectorValues() {
        EditorPrefs.SetBool(nameof(hideSpotsHelper) + "Key", hideSpotsHelper);
        if (hideSpotsHolderTransform != null) {
            EditorPrefs.SetString(nameof(hideSpotsHolderTransform) + "Key", GetTransformPath(hideSpotsHolderTransform));
        }
        else {
            EditorPrefs.DeleteKey(nameof(hideSpotsHolderTransform) + "Key");
        }
    }

    private void LoadInspectorValues() {
        hideSpotsHelper = EditorPrefs.GetBool(nameof(hideSpotsHolderTransform) + "Key", false);
        string holderPath = EditorPrefs.GetString(nameof(hideSpotsHolderTransform) + "Key");
        if (!string.IsNullOrEmpty(holderPath)) {
            hideSpotsHolderTransform = GameObject.Find(holderPath)?.transform;
        }
    }
}
