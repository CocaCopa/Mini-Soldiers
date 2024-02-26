using System.Collections;
using UnityEngine;

public class Test : MonoBehaviour {
    Mesh mesh;

    private void Start() {
        ReadMesh();
    }

    private void ReadMesh() {
        mesh = GetComponent<MeshFilter>().mesh;

        // Get the bounds of the mesh
        Bounds bounds = mesh.bounds;

        // Calculate the corners of the bounding box
        Vector3 frontBottomLeft = transform.TransformPoint(bounds.min);
        Vector3 frontBottomRight = transform.TransformPoint(new Vector3(bounds.max.x, bounds.min.y, bounds.min.z));
        Vector3 backBottomLeft = transform.TransformPoint(new Vector3(bounds.min.x, bounds.min.y, bounds.max.z));
        Vector3 backBottomRight = transform.TransformPoint(new Vector3(bounds.max.x, bounds.min.y, bounds.max.z));

        // Draw rays at the calculated positions with an upward direction
        Debug.DrawRay(frontBottomLeft, Vector3.up * 5f, Color.magenta, float.MaxValue);
        Debug.DrawRay(frontBottomRight, Vector3.up * 5f, Color.magenta, float.MaxValue);
        Debug.DrawRay(backBottomLeft, Vector3.up * 5f, Color.magenta, float.MaxValue);
        Debug.DrawRay(backBottomRight, Vector3.up * 5f, Color.magenta, float.MaxValue);
    }
}
