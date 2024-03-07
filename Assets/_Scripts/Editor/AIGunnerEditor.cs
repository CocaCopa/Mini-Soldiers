using PlasticPipe.PlasticProtocol.Messages;
using UnityEditor;

[CustomEditor(typeof(AIGunner))]
public class AIGunnerEditor : AIControllerEditor {

    SerializedProperty hideSpots;
    
    SerializedProperty patrolPoints;
    SerializedProperty patrolTime;
    SerializedProperty timeToSpotPlayer;

    SerializedProperty peekCornerDistance;

    SerializedProperty delayBeforeShootingMin;
    SerializedProperty delayBeforeShootingMax;

    SerializedProperty timeToPeekMin;
    SerializedProperty timeToPeekMax;

    SerializedProperty keepPeekingTimeMin;
    SerializedProperty keepPeekingTimeMax;

    SerializedProperty repositionDistance;

    private bool foldoutPatrolSettings;
    private bool foldoutHideSpotSettings;
    private bool foldoutCombatSettings;

    public override void OnEnable() {
        base.OnEnable();
        hideSpots = serializedObject.FindProperty(nameof(hideSpots));
        
        patrolPoints = serializedObject.FindProperty(nameof(patrolPoints));
        patrolTime = serializedObject.FindProperty(nameof(patrolTime));
        timeToSpotPlayer = serializedObject.FindProperty(nameof(timeToSpotPlayer));

        peekCornerDistance = serializedObject.FindProperty(nameof(peekCornerDistance));
        delayBeforeShootingMin = serializedObject.FindProperty(nameof(delayBeforeShootingMin));
        delayBeforeShootingMax = serializedObject.FindProperty(nameof(delayBeforeShootingMax));

        timeToPeekMin = serializedObject.FindProperty(nameof(timeToPeekMin));
        timeToPeekMax = serializedObject.FindProperty(nameof(timeToPeekMax));

        keepPeekingTimeMin = serializedObject.FindProperty(nameof(keepPeekingTimeMin));
        keepPeekingTimeMax = serializedObject.FindProperty(nameof(keepPeekingTimeMax));

        repositionDistance = serializedObject.FindProperty(nameof(repositionDistance));
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
            EditorGUILayout.PropertyField(timeToSpotPlayer);
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
            EditorGUILayout.PropertyField(delayBeforeShootingMin);
            EditorGUILayout.PropertyField(delayBeforeShootingMax);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(timeToPeekMin);
            EditorGUILayout.PropertyField(timeToPeekMax);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(keepPeekingTimeMin);
            EditorGUILayout.PropertyField(keepPeekingTimeMax);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(repositionDistance);
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
