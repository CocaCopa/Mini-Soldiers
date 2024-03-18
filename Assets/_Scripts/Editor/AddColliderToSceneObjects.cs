using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Unity.VisualScripting;

public class AddColliderToSceneObjects : MonoBehaviour {
    [MenuItem("Objects/Add Mesh Collider")]
    public static void SelectObjects() {
        List<GameObject> objectsWithoutCollider = FindObjects();
        List<Transform> transforms = new List<Transform>();
        foreach (GameObject obj in objectsWithoutCollider) {
            transforms.Add(obj.transform);
        }
        foreach (Transform obj in transforms) {
            obj.transform.AddComponent<MeshCollider>();
        }
    }

    private static List<GameObject> FindObjects() {
        List<GameObject> objectsWithoutCollider = new List<GameObject>();
        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject rootObject in rootObjects) {
            if (rootObject.GetComponent<MeshRenderer>() && rootObject.GetComponent<MeshFilter>() && !rootObject.GetComponent<Collider>()) {
                objectsWithoutCollider.Add(rootObject);
            }
            CheckChildObjects(rootObject.transform, objectsWithoutCollider);
        }
        return objectsWithoutCollider;
    }

    private static void CheckChildObjects(Transform parent, List<GameObject> objectsWithoutCollider) {
        foreach (Transform child in parent) {
            if (child.GetComponent<MeshRenderer>() && child.GetComponent<MeshFilter>() && !child.GetComponent<Collider>()) {
                objectsWithoutCollider.Add(child.gameObject);
            }
            CheckChildObjects(child, objectsWithoutCollider);
        }
    }
}
