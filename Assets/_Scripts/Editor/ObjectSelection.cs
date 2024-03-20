using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjectSelection : EditorWindow {
    private GameObject selectedObject;
    private GameObject targetParent;
    private List<GameObject> selectedObjects = new List<GameObject>();

    [MenuItem("Objects/Object Selection Window")]
    public static void ShowWindow() {
        EditorWindow.GetWindow(typeof(ObjectSelection));
    }

    private void OnGUI() {
        Repaint();
        SelectedObjectLabelWithButton();
        TargetParentLabelWithButton();
        GUILayout.Space(10);
        selectedObject = Selection.activeGameObject;

        if (selectedObject != null) {
            ActionButtons();
        }
        Repaint();
    }

    private void OnInspectorUpdate() {
        if (selectedObject != null && selectedObjects.Count > 0) {
            if (selectedObject.transform.parent != targetParent.transform && selectedObjects.Contains(selectedObject)) {
                selectedObject.SetActive(true);
                selectedObjects.Remove(selectedObject);
            }
        }
    }

    private void SelectedObjectLabelWithButton() {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Selected Object: " + (selectedObject != null ? selectedObject.name : "None"));
        if (selectedObject != null && selectedObject != targetParent) {
            if (GUILayout.Button("Set Target Parent")) {
                SetTargetParent(selectedObject);
            }
        }
        else {
            GUILayout.Label("", GUILayout.Width(1));
        }
        GUILayout.EndHorizontal();
    }

    private void TargetParentLabelWithButton() {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Target Parent: " + (targetParent != null ? targetParent.name : "None"));
        if (targetParent != null) {
            if (GUILayout.Button("Find Parent")) {
                Selection.activeGameObject = targetParent;
            }
        }
        else {
            GUILayout.Label("", GUILayout.Width(1));
        }
        GUILayout.EndHorizontal();
    }

    private void ActionButtons() {
        if (targetParent != null) {
            if (GUILayout.Button("Change Parent")) {
                ChangeParent(selectedObject);
            }

            if (GUILayout.Button("Toggle Enable Objects")) {
                foreach (GameObject obj in selectedObjects) {
                    obj.SetActive(!obj.activeInHierarchy);
                }
            }
        }
    }

    private void SetTargetParent(GameObject selectedObject) {
        targetParent = selectedObject;
        selectedObjects.Clear();
        for (int i = 0; i < targetParent.transform.childCount; i++) {
            GameObject child = targetParent.transform.GetChild(i).gameObject;
            selectedObjects.Add(child);
        }
    }

    private void ChangeParent(GameObject obj) {
        obj.transform.parent = targetParent.transform;
        selectedObjects.Add(obj);
        obj.SetActive(false);
    }
}
