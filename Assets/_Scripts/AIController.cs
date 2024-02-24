using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using CocaCopa;
using System.Runtime.CompilerServices;

public class AIController : Controller {

    [SerializeField] private List<Transform> hideSpots;

    private Transform playerTransform;
    private Vector2 inputDirection;

    private NavMeshPath navMeshPath;
    private Vector3[] waypoints;
    private int currentWaypointIndex;
    private Vector3 targetPosition;

    protected override void Awake() {
        base.Awake();
        playerTransform = FindObjectOfType<PlayerController>().transform;
        objectToLookAt = new GameObject();
        objectToLookAt.name = "AI Look At";
    }

    protected override void Start() {
        base.Start();
        navMeshPath = new NavMeshPath();
        waypoints = new Vector3[0];
    }

    private void Update() {
        objectToLookAt.transform.position = playerTransform.position + Vector3.up * 1.2f;
        orientation.CharacterRotation(inputDirection, objectToLookAt.transform);
        if (GetComponent<AIStimulus>().CanSeeTarget(playerTransform)) {
            combatManager.ReleaseGunTrigger();
            //SetNewPath(playerTransform.position);
            combatManager.FireEquippedWeapon();
            if (combatManager.EquippedWeapon.RemainingBullets == 0) {
                combatManager.SwitchToSecondary();
            }
            //Vector3 position = FindHidePosition();
            List<Transform> losFilter = FilterByLineOfSight(hideSpots, playerTransform.gameObject);
            List<Transform> distanceFilter = FilterByDistance(losFilter, playerTransform, 15f);
            List<Transform> postitionFilter = FilterByObjectTowardsTarget(distanceFilter, playerTransform, 2f);
            List<Transform> dotFilter = FilterByDotProduct(postitionFilter, playerTransform);

            Vector3 position = CocaCopa.Utilities.Environment.FindClosestPosition(transform.position, dotFilter);
            targetPosition = position;
            SetNewPath(position);
        }
    }

    private void FixedUpdate() {
        if (FollowNavMeshPath(targetPosition, distanceFromTarget: 0f, out inputDirection)) {
            movement.MoveTowardsDirection(inputDirection, run: true);
        }
    }

    private void SetNewPath(Vector3 targetPosition) {
        if (Vector3.Distance(transform.position, targetPosition) > 0.1f) {
            if (NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, navMeshPath)) {
                waypoints = navMeshPath.corners;
                currentWaypointIndex = 0;
            }
            else {
                Debug.LogError("Error calculating path for agent.");
            }
        }
    }

    private bool FollowNavMeshPath(Vector3 targetPosition, float distanceFromTarget, out Vector2 direction) {
        direction = Vector2.zero;

        if (waypoints.Length == 0) {
            Debug.LogError("No path has been calculated but the AI is trying to follow one.");
            return false;
        }

        if (currentWaypointIndex < waypoints.Length) {
            if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex]) < 0.1f) {
                currentWaypointIndex++;
                if (currentWaypointIndex >= waypoints.Length) {
                    print("Reached the end of the path. Calculate a new path for the AI to start moving again.");
                }
            }

            if (currentWaypointIndex <= waypoints.Length - 1) {
                Vector3 moveDirection = (waypoints[currentWaypointIndex] - transform.position).normalized;

                if (Vector3.Distance(transform.position, targetPosition) <= distanceFromTarget) {
                    direction = Vector3.zero;
                }
                else {
                    direction = new Vector2(moveDirection.x, moveDirection.z).normalized;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Finds a suitable hide position based on various criteria such as line of sight, distance, and obstacles between the hide spots and the target object.
    /// </summary>
    /// <returns>The position of the found hide spot.</returns>
    private Vector3 FindHidePosition() {
        validPoints.Clear();
        invalidPoints.Clear();
        List<Transform> filteredHideSpots = new List<Transform>();
        foreach (Transform hideSpot in hideSpots) {
            // Check for objects between the hide spot and the target object.
            if (Physics.Linecast(hideSpot.position + Vector3.up * 0.5f, playerTransform.position + Vector3.up * 0.5f, out RaycastHit hitInfo)) {
                // Hide spot cannot see the target and it is 15f away from the target.
                if (!hitInfo.transform.TryGetComponent<AITarget>(out _) && Vector3.Distance(hideSpot.position, playerTransform.position) > 15f) {
                    // Hide spot has a wall in front of it, in the direction towards the target.
                    if (Physics.Raycast(hideSpot.position + Vector3.up * 0.2f, (playerTransform.position - hideSpot.position).normalized, 2f)) {
                        // TODO: AItoHidespot direction and AItoTarget produce a dot product < 0.
                        validPoints.Add(hideSpot.position);
                        filteredHideSpots.Add(hideSpot);
                    }
                }
            }
            if (!validPoints.Contains(hideSpot.position)) {
                invalidPoints.Add(hideSpot.position);
            }
        }
        return CocaCopa.Utilities.Environment.FindClosestPosition(transform.position, filteredHideSpots);
    }

    /// <summary>
    /// Filters a list of hide spots based on line of sight to the target GameObject.
    /// </summary>
    /// <param name="hideSpots">The list of hide spots to filter.</param>
    /// <param name="target">The target GameObject to check line of sight to.</param>
    /// <returns>The filtered list of hide spots.</returns>
    private List<Transform> FilterByLineOfSight(List<Transform> hideSpots, GameObject target) {
        List<Transform> filteredSpots = new List<Transform>();
        foreach (Transform hideSpot in hideSpots) {
            MarkAsInvalid(hideSpot.position);
            if (Physics.Linecast(hideSpot.position, target.transform.position, out RaycastHit hitInfo)) {
                if (hitInfo.transform.gameObject != target) {
                    MarkAsValid(hideSpot.position);
                    filteredSpots.Add(hideSpot);
                }
            }
        }
        return filteredSpots;
    }

    /// <summary>
    /// Filters a list of hide spots based on distance from the target Transform.
    /// </summary>
    /// <param name="hideSpots">The list of hide spots to filter.</param>
    /// <param name="target">The target Transform to calculate distance from.</param>
    /// <param name="distance">The maximum distance allowed between hide spots and the target.</param>
    /// <returns>The filtered list of hide spots.</returns>
    private List<Transform> FilterByDistance(List<Transform> hideSpots, Transform target, float distance) {
        List<Transform> filteredSpots = new List<Transform>();
        foreach (Transform hideSpot in hideSpots) {
            MarkAsInvalid(hideSpot.position);
            if (Vector3.Distance(hideSpot.position, target.position) > distance) {
                MarkAsValid(hideSpot.position);
                filteredSpots.Add(hideSpot);
            }
        }
        return filteredSpots;
    }

    /// <summary>
    /// Filters a list of hide spots based on obstacles between the hide spots and the target within a specified distance.
    /// </summary>
    /// <param name="hideSpots">The list of hide spots to filter.</param>
    /// <param name="target">The target Transform to check for obstacles towards.</param>
    /// <param name="distance">The maximum distance to check for obstacles.</param>
    /// <returns>The filtered list of hide spots.</returns>
    private List<Transform> FilterByObjectTowardsTarget(List<Transform> hideSpots, Transform target, float distance) {
        List<Transform> filteredSpots = new List<Transform>();
        foreach (Transform hideSpot in hideSpots) {
            MarkAsInvalid(hideSpot.position);
            Vector3 origin = hideSpot.position + Vector3.up * 0.1f;
            Vector3 spotToTarget = (target.position - hideSpot.position).normalized;
            if (Physics.Raycast(origin, spotToTarget, distance)) {
                MarkAsValid(hideSpot.position);
                filteredSpots.Add(hideSpot);
            }
        }
        return filteredSpots;
    }

    /// <summary>
    /// Filters a list of hide spots based on the dot product of their direction vectors with the direction vector towards the target.
    /// </summary>
    /// <param name="hideSpots">The list of hide spots to filter.</param>
    /// <param name="target">The target Transform to calculate dot product with.</param>
    /// <returns>The filtered list of hide spots.</returns>
    private List<Transform> FilterByDotProduct(List<Transform> hideSpots, Transform target) {
        List<Transform> filteredSpots = new List<Transform>();
        foreach (Transform hideSpot in hideSpots) {
            MarkAsInvalid(hideSpot.position);
            Vector3 toHideSpot = (hideSpot.position - transform.position).normalized;
            Vector3 toTarget = (target.position - transform.position).normalized;
            if (Vector3.Dot(toHideSpot, toTarget) < 0f) {
                MarkAsValid(hideSpot.position);
                filteredSpots.Add(hideSpot);
            }
        }
        return filteredSpots;
    }

    private void MarkAsInvalid(Vector3 position) {
        if (!invalidPoints.Contains(position)) {
            invalidPoints.Add(position);
        }
        if (validPoints.Contains(position)) {
            validPoints.Remove(position);
        }
    }

    private void MarkAsValid(Vector3 position) {
        if (invalidPoints.Contains(position)) {
            invalidPoints.Remove(position);
        }
        if (!validPoints.Contains(position)) {
            validPoints.Add(position);
        }
    }

    private List<Vector3> validPoints = new();
    private float sphereRadius = 0.5f;
    private List<Vector3> invalidPoints = new();
    private void OnDrawGizmos() {
        if (invalidPoints.Count > 0) {
            foreach (var point in invalidPoints) {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(point, sphereRadius);
            }
            foreach (var point in validPoints) {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(point, sphereRadius);
            }
        }
    }
}
