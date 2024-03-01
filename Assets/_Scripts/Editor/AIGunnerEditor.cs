using UnityEditor;

[CustomEditor(typeof(AIGunner))]
public class AIGunnerEditor : AIControllerEditor {

    SerializedProperty hideSpots;
    
    SerializedProperty patrolPoints;
    SerializedProperty patrolTime;

    SerializedProperty timeToPeekMin;
    SerializedProperty timeToPeekMax;
    SerializedProperty keepPeekingTimeMin;
    SerializedProperty keepPeekingTimeMax;
    SerializedProperty peekCornerDistance;

    private bool foldoutPatrolSettings;
    private bool foldoutHideSpotSettings;
    private bool foldoutCombatSettings;

    public override void OnEnable() {
        base.OnEnable();
        hideSpots = serializedObject.FindProperty(nameof(hideSpots));
        
        patrolPoints = serializedObject.FindProperty(nameof(patrolPoints));
        patrolTime = serializedObject.FindProperty(nameof(patrolTime));

        timeToPeekMin = serializedObject.FindProperty(nameof(timeToPeekMin));
        timeToPeekMax = serializedObject.FindProperty(nameof(timeToPeekMax));
        keepPeekingTimeMin = serializedObject.FindProperty(nameof(keepPeekingTimeMin));
        keepPeekingTimeMax = serializedObject.FindProperty(nameof(keepPeekingTimeMax));
        peekCornerDistance = serializedObject.FindProperty(nameof(peekCornerDistance));
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.Space(10);
        DisplayPatrolSettingsFoldout();
        if (foldoutPatrolSettings) {
            EditorGUILayout.Space(10);
        }
        DisplayCombatSettingsFoldout();
        if (foldoutCombatSettings) {
            EditorGUILayout.Space(10);
        }
        DisplayHideSpotSettingsFoldout();
        EditorGUILayout.Space(10);
        serializedObject.ApplyModifiedProperties();
    }

    private void DisplayPatrolSettingsFoldout() {
        foldoutPatrolSettings = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutPatrolSettings, "Patrol Settings");
        if (foldoutPatrolSettings) {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(patrolTime);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        if (foldoutPatrolSettings) {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(patrolPoints);
            EditorGUI.indentLevel--;
        }
    }

    private void DisplayCombatSettingsFoldout() {
        foldoutCombatSettings = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutCombatSettings, "Combat Settings");
        if (foldoutCombatSettings) {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(peekCornerDistance);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(timeToPeekMin);
            EditorGUILayout.PropertyField(timeToPeekMax);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(keepPeekingTimeMin);
            EditorGUILayout.PropertyField(keepPeekingTimeMax);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DisplayHideSpotSettingsFoldout() {
        foldoutHideSpotSettings = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutHideSpotSettings, "Hide Spot Settings");
        EditorGUILayout.EndFoldoutHeaderGroup();
        if (foldoutHideSpotSettings) {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(hideSpots);
            DisplayHideSpotsHelperButton();
            EditorGUI.indentLevel--;
        }
    }
}
