using System.Collections.Generic;
using UnityEngine;

public class QuerierTest : MonoBehaviour {

    public Transform building;
    private List<Vector3> objectEdges = new List<Vector3>();

    private void Awake() {
        ReadMesh();
    }

    private void Update() {
        FindWall();
    }

    private void FindWall() {
        List<Vector3> oppositeEdgesPositions = new List<Vector3>();
        Vector3 closestEdge = CocaCopa.Utilities.Environment.FindClosestPosition(transform.position, objectEdges);

        List<Vector3> remainingEdges = new List<Vector3>();
        foreach (var edge in objectEdges) {
            if (edge != closestEdge) {
                remainingEdges.Add(edge);
            }
        }
        Vector3 closestEdgeToQuerierDirection = (transform.position - closestEdge).normalized;
        Debug.DrawRay(closestEdge, closestEdgeToQuerierDirection * 10f, Color.yellow);
        foreach (var edge in remainingEdges) {
            Vector3 edgeToQuerierDirection = (transform.position - edge).normalized;
            Debug.DrawRay(edge, edgeToQuerierDirection * 10f, Color.yellow);
            if (Vector3.Dot(closestEdgeToQuerierDirection, edgeToQuerierDirection) < 0) {
                oppositeEdgesPositions.Add(edge);
            }
        }
        Debug.Log(oppositeEdgesPositions.Count);
        Vector3 oppositeClosestEdge = CocaCopa.Utilities.Environment.FindClosestPosition(transform.position, oppositeEdgesPositions);

        Debug.DrawRay(closestEdge, Vector3.up * 50f, Color.red);
        Debug.DrawRay(oppositeClosestEdge, Vector3.up * 50f, Color.red);
    }

    private void ReadMesh() {
        Mesh mesh = building.GetComponent<MeshFilter>().mesh;

        // Get the bounds of the mesh
        Bounds bounds = mesh.bounds;

        // Calculate the corners of the bounding box
        Vector3 frontBottomLeft = building.TransformPoint(bounds.min);
        Vector3 frontBottomRight = building.TransformPoint(new Vector3(bounds.max.x, bounds.min.y, bounds.min.z));
        Vector3 backBottomLeft = building.TransformPoint(new Vector3(bounds.min.x, bounds.min.y, bounds.max.z));
        Vector3 backBottomRight = building.TransformPoint(new Vector3(bounds.max.x, bounds.min.y, bounds.max.z));

        objectEdges.Add(frontBottomLeft);
        objectEdges.Add(frontBottomRight);
        objectEdges.Add(backBottomLeft);
        objectEdges.Add(backBottomRight);

        // Draw rays at the calculated positions with an upward direction
        /*Debug.DrawRay(frontBottomLeft, Vector3.up * 5f, Color.red, float.MaxValue);
        Debug.DrawRay(frontBottomRight, Vector3.up * 5f, Color.red, float.MaxValue);
        Debug.DrawRay(backBottomLeft, Vector3.up * 5f, Color.red, float.MaxValue);
        Debug.DrawRay(backBottomRight, Vector3.up * 5f, Color.red, float.MaxValue);*/
    }
}
