using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

[CustomEditor(typeof(AIStimulus))]
public class AIStimulusEditor : Editor {

    private AIStimulus aiStimulus;
    private bool debugSense = true;
    private float colorAlpha = 0.1f;
    private Transform targetTransform;

    private const string DebugSenseKey = "DebugSense";
    private const string ColorAlphaKey = "ColorAlpha";
    private const string TargetTransformKey = "TargetTransform";

    private float height;

    private void OnEnable() {
        aiStimulus = target as AIStimulus;
        LoadInspectorValues();
        height = aiStimulus.EyesTransform.position.y;
    }

    public override void OnInspectorGUI() {
        DisplayScriptReference();
        EditorGUI.BeginChangeCheck();
        debugSense = EditorGUILayout.Toggle("Debug Sense", debugSense);
        if (debugSense) {
            GUIContent targetTransformLabel = new GUIContent("Target Transform", "The transform of the target to check visibility for.\n\nThis value is intended for debugging purposes only and does not impact the game logic.");
            targetTransform = EditorGUILayout.ObjectField(targetTransformLabel, targetTransform, typeof(Transform), true) as Transform;
            colorAlpha = EditorGUILayout.Slider("Arc Opacity", colorAlpha, 0f, 1f);
        }
        if (EditorGUI.EndChangeCheck()) {
            SceneView.RepaintAll();
            SaveInspectorValues();
        }
        DrawDefaultExcludingCustomFields();
    }

    private void OnSceneGUI() {
        DrawLineOfSightArc();
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
        string[] excludeField = new string[1];
        excludeField[0] = "m_Script";
        DrawPropertiesExcluding(serializedObject, excludeField);
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawLineOfSightArc() {
        if (debugSense && EyesTransformAssigned()) {
            Handles.color = ArcColor();
            Vector3 forwardDirection = aiStimulus.EyesTransform.forward;
            // The transform of the eyes rotate based on the animation. This line stabilizes the height of the direction.
            forwardDirection.y = aiStimulus.transform.position.y;
            Vector3 rotatedForward = Quaternion.Euler(0, -aiStimulus.FieldOfView * 0.5f, 0) * forwardDirection;
            Handles.DrawSolidArc(
                aiStimulus.transform.position,
                Vector3.up,
                rotatedForward,
                aiStimulus.FieldOfView,
                aiStimulus.SightRadius);
        }
    }

    private bool EyesTransformAssigned() {
        if (aiStimulus.EyesTransform == null) {
            Debug.LogError("Cannot debug sight sense. Please provide a Transform for the AI's eyes.");
            debugSense = false;
            return false;
        }
        return true;
    }

    private Color ArcColor() {
        Color color = Color.red;
        if (targetTransform != null) {
            if (aiStimulus.CanSeeTarget(targetTransform)) {
                color = Color.magenta;
            }
        }
        color.a = colorAlpha;
        return color;
    }

    private string GetTransformPath(Transform transform) {
        if (transform == null)
            return "";
        return GetTransformPath(transform.parent) + "/" + transform.name;
    }

    private void SaveInspectorValues() {
        EditorPrefs.SetBool(DebugSenseKey, debugSense);
        EditorPrefs.SetFloat(ColorAlphaKey, colorAlpha);
        if (targetTransform != null) {
            EditorPrefs.SetString(TargetTransformKey, GetTransformPath(targetTransform));
        }
        else {
            EditorPrefs.DeleteKey(TargetTransformKey);
        }
    }

    private void LoadInspectorValues() {
        debugSense = EditorPrefs.GetBool(DebugSenseKey, true);
        colorAlpha = EditorPrefs.GetFloat(ColorAlphaKey, 0.1f);
        string targetTransformPath = EditorPrefs.GetString(TargetTransformKey);
        if (!string.IsNullOrEmpty(targetTransformPath)) {
            targetTransform = GameObject.Find(targetTransformPath)?.transform;
        }
    }
}