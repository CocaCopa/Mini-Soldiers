using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AIController))]
public class AIControllerEditor : Editor {

    private const string AUTO_ASSIGN_HOLDER = "Auto Assign Holder";
    private const string ASSIGN_HIDE_SPOTS = "Assign Hide Spots";

    SerializedProperty hideSpots;
    SerializedProperty onGameStart;
    SerializedProperty lookAtTargetObjectSpeed;
    SerializedProperty mainState;
    SerializedProperty playerSpottedState;
    SerializedProperty peekCornerState;
    SerializedProperty maximumHealthPoints;
    SerializedProperty currentHealthPoints;

    private bool autoAssignFailed = false;
    private bool hideSpotsHelper = false;
    private Transform hideSpotsHolderTransform;

    public virtual void OnEnable() {
        LoadInspectorValues();
        FindTargetScriptProperties();
    }

    private void FindTargetScriptProperties() {
        onGameStart = serializedObject.FindProperty(nameof(onGameStart));
        lookAtTargetObjectSpeed = serializedObject.FindProperty(nameof(lookAtTargetObjectSpeed));
        hideSpots = serializedObject.FindProperty(nameof(hideSpots));
        mainState = serializedObject.FindProperty(nameof(mainState));
        playerSpottedState = serializedObject.FindProperty(nameof(playerSpottedState));
        peekCornerState = serializedObject.FindProperty(nameof(peekCornerState));
        maximumHealthPoints = serializedObject.FindProperty(nameof(maximumHealthPoints));
        currentHealthPoints = serializedObject.FindProperty(nameof(currentHealthPoints));
    }

    public override void OnInspectorGUI() {
        DisplayScriptReference();
        serializedObject.Update();
        EditorGUILayout.PropertyField(onGameStart);
        EditorGUILayout.PropertyField(lookAtTargetObjectSpeed);
        EditorGUILayout.PropertyField(maximumHealthPoints);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(currentHealthPoints);
        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(mainState);
        EditorGUILayout.PropertyField(playerSpottedState);
        EditorGUILayout.PropertyField(peekCornerState);
        EditorGUI.EndDisabledGroup();
        serializedObject.ApplyModifiedProperties();
    }

    protected void DisplayHideSpotsHelperButton() {
        EditorGUI.BeginChangeCheck();
        hideSpotsHelper = EditorGUILayout.Toggle("Hide Spots Helper", hideSpotsHelper);
        if (hideSpotsHelper) {
            hideSpotsHolderTransform = EditorGUILayout.ObjectField("Hide Spots Holder", hideSpotsHolderTransform, typeof(Transform), true) as Transform;
            Button_AutoAssignHolder();
            Button_AssignHideSpots();
        }
        if (EditorGUI.EndChangeCheck()) {
            SaveInspectorValues();
        }
    }

    private void DisplayScriptReference() {
        MonoBehaviour scriptComponent = target as MonoBehaviour;
        SerializedObject m_serializedObject = new SerializedObject(scriptComponent);
        SerializedProperty scriptProperty = m_serializedObject.FindProperty("m_Script");
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(scriptProperty, true, new GUILayoutOption[0]);
        EditorGUI.EndDisabledGroup();
    }

    private void Button_AutoAssignHolder() {
        if (autoAssignFailed && hideSpotsHolderTransform == null) {
            EditorGUILayout.HelpBox("Attempt to auto assign holder reference failed. Assign the 'Hide Spots Holder' reference manually.", MessageType.Info);
        }
        if (hideSpotsHolderTransform == null) {
            if (GUILayout.Button(AUTO_ASSIGN_HOLDER)) {
                if (AttemptToFindHideSpotsHolderObject(out GameObject holder)) {
                    hideSpotsHolderTransform = holder.transform;
                    autoAssignFailed = false;
                }
                if (hideSpotsHolderTransform == null) {
                    autoAssignFailed = true;
                }
            }
        }
        else {
            autoAssignFailed = false;
        }
    }

    private void Button_AssignHideSpots() {
        if (hideSpotsHolderTransform != null) {
            if (GUILayout.Button(ASSIGN_HIDE_SPOTS)) {
                hideSpots.ClearArray();

                for (int i = 0; i < hideSpotsHolderTransform.childCount; i++) {
                    Transform hideSpot = hideSpotsHolderTransform.GetChild(i);
                    hideSpots.InsertArrayElementAtIndex(i);
                    hideSpots.GetArrayElementAtIndex(i).objectReferenceValue = hideSpot;
                }
            }
        }
    }

    private bool AttemptToFindHideSpotsHolderObject(out GameObject hideSpotsHolder) {
        string[] holderPossibleNames = new string[] { "Hide", "HideSpots", "HideSpotsHolder", "Hide Spots", "Hide Spots Holder" };
        hideSpotsHolder = null;

        foreach (string name in holderPossibleNames) {
            hideSpotsHolder = GameObject.Find(name);
            if (hideSpotsHolder != null) {
                break;
            }
        }
        return hideSpotsHolder;
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
        hideSpotsHelper = EditorPrefs.GetBool(nameof(hideSpotsHelper) + "Key", false);
        string holderPath = EditorPrefs.GetString(nameof(hideSpotsHolderTransform) + "Key");
        if (!string.IsNullOrEmpty(holderPath)) {
            hideSpotsHolderTransform = GameObject.Find(holderPath)?.transform;
        }
    }
}
