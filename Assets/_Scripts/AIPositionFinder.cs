using System.Collections.Generic;
using UnityEngine;

public abstract class AIPositionFinder : MonoBehaviour {

    public static readonly List<Vector3> validPoints = new();
    public static readonly List<Vector3> invalidPoints = new();
    public static readonly float sphereRadius = 0.5f;

    /// <summary>
    /// Filters a list of transforms based on distance from the target Transform.
    /// </summary>
    /// <param name="transforms">The list of transforms to filter.</param>
    /// <param name="target">The target Transform to calculate distance from.</param>
    /// <param name="distance">The maximum distance allowed between each transform and the target.</param>
    /// <returns>The filtered list of transforms.</returns>
    public static List<Transform> FilterByDistance(List<Transform> transforms, Transform target, float distance) {
        List<Transform> filteredTransforms = new List<Transform>();
        foreach (Transform m_Transform in transforms) {
            MarkAsInvalid(m_Transform.position);
            if (Vector3.Distance(m_Transform.position, target.position) > distance) {
                MarkAsValid(m_Transform.position);
                filteredTransforms.Add(m_Transform);
            }
        }
        if (filteredTransforms == null || filteredTransforms.Count == 0) {
            Debug.LogWarning(nameof(FilterByDistance) + ": All positions were marked as invalid.");
        }
        return filteredTransforms;
    }

    /// <summary>
    /// Filters a list of hide spots based on line of sight to the target GameObject.
    /// </summary>
    /// <param name="transforms">The list of transforms to filter.</param>
    /// <param name="target">The target GameObject to check line of sight to.</param>
    /// <returns>The filtered list of hide spots.</returns>
    public static List<Transform> FilterByLineOfSight(List<Transform> transforms, Transform target) {
        List<Transform> filteredTransforms = new List<Transform>();
        foreach (Transform m_Transform in transforms) {
            MarkAsInvalid(m_Transform.position);
            if (Physics.Linecast(m_Transform.position, target.position, out RaycastHit hitInfo)) {
                if (hitInfo.transform != target) {
                    MarkAsValid(m_Transform.position);
                    filteredTransforms.Add(m_Transform);
                }
            }
        }
        if (filteredTransforms == null || filteredTransforms.Count == 0) {
            Debug.LogWarning(nameof(FilterByLineOfSight) + ": All positions were marked as invalid.");
        }
        return filteredTransforms;
    }

    /// <summary>
    /// Filters a list of transforms based on obstacles between each transform and the target within a specified distance.
    /// </summary>
    /// <param name="transforms">The list of transforms to filter.</param>
    /// <param name="target">The target Transform to check for obstacles towards.</param>
    /// <param name="distance">The maximum distance to check for obstacles.</param>
    /// <returns>The filtered list of transforms.</returns>
    public static List<Transform> FilterByObjectTowardsTarget(List<Transform> transforms, Transform target, float distance) {
        List<Transform> filteredTransforms = new List<Transform>();
        foreach (Transform m_Transform in transforms) {
            MarkAsInvalid(m_Transform.position);
            Vector3 origin = m_Transform.position + Vector3.up * 0.1f;
            Vector3 spotToTarget = (target.position - m_Transform.position).normalized;
            if (Physics.Raycast(origin, spotToTarget, distance)) {
                MarkAsValid(m_Transform.position);
                filteredTransforms.Add(m_Transform);
            }
        }
        if (filteredTransforms == null ||  filteredTransforms.Count == 0) {
            Debug.LogWarning(nameof(FilterByObjectTowardsTarget) + ": All positions were marked as invalid.");
        }
        return filteredTransforms;
    }

    /// <summary>
    /// Filters a list of transforms based on the dot product of their direction vectors with the direction vector towards the target.
    /// </summary>
    /// <param name="transforms">The list of transforms to filter.</param>
    /// <param name="target">The target Transform to calculate dot product with.</param>
    /// <returns>The filtered list of transforms.</returns>
    public static List<Transform> FilterByDotProduct(List<Transform> transforms, Transform transform_1, Transform transform_2) {
        List<Transform> filteredTransforms = new List<Transform>();
        foreach (Transform m_Transform in transforms) {
            MarkAsInvalid(m_Transform.position);
            Vector3 spotToTransform_1 = (transform_1.position - m_Transform.position).normalized;
            Vector3 spotToTransform_2 = (transform_2.position - m_Transform.position).normalized;
            if (Vector3.Dot(spotToTransform_2, spotToTransform_1) > 0f) {
                MarkAsValid(m_Transform.position);
                filteredTransforms.Add(m_Transform);
            }
        }
        if (filteredTransforms == null || filteredTransforms.Count == 0) {
            Debug.LogWarning(nameof(FilterByDotProduct) + ": All positions were marked as invalid.");
        }
        return filteredTransforms;
    }

    private static void MarkAsInvalid(Vector3 position) {
        if (!invalidPoints.Contains(position)) {
            invalidPoints.Add(position);
        }
        if (validPoints.Contains(position)) {
            validPoints.Remove(position);
        }
    }

    private static void MarkAsValid(Vector3 position) {
        if (invalidPoints.Contains(position)) {
            invalidPoints.Remove(position);
        }
        if (!validPoints.Contains(position)) {
            validPoints.Add(position);
        }
    }
}
