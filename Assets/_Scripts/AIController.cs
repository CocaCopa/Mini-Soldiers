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
            Vector3 position = FindHidePosition();
            targetPosition = position;
            SetNewPath(position);
        }
    }

    private void FixedUpdate() {
        if (FollowNavMeshPath(targetPosition, distanceFromTarget: 0f, out inputDirection)) {
            movement.MoveTowardsDirection(inputDirection, false);
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

    private Vector3 FindHidePosition() {
        validPoints.Clear();
        invalidPoints.Clear();
        List<Transform> filteredHideSpots = new List<Transform>();
        foreach (Transform hideSpot in hideSpots) {
            invalidPoints.Add(hideSpot.position);
            // Check for objects between the hide spot and the target object.
            if (Physics.Linecast(hideSpot.position, playerTransform.position, out RaycastHit hitInfo)) {
                // Hide spot cannot see the target and it is 15f away from the target.
                if (!hitInfo.transform.TryGetComponent<AITarget>(out _) && Vector3.Distance(hideSpot.position, playerTransform.position) > 15f) {
                    // Hide spot has a wall in front of it, in the direction towards the target.
                    if (Physics.Raycast(hideSpot.position + Vector3.up * 0.2f, (playerTransform.position - hideSpot.position).normalized, 2f)) {
                        // TODO: AItoHidespot direction and AItoTarget produce a dot product < 0.
                        validPoints.Add(hideSpot.position);
                        invalidPoints.Remove(hideSpot.position);
                        filteredHideSpots.Add(hideSpot);
                    }
                }
            }
        }
        return CocaCopa.Utilities.Environment.FindClosestPosition(transform.position, filteredHideSpots);
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
