using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using CocaCopa.Utilities;

public class AIController : Controller {

    private enum AI_State {
        Patrol,
        Investigate,
    };

    [SerializeField] private List<Transform> hideSpots;

    private Transform playerTransform;
    private Vector2 inputDirection;

    private NavMeshPath navMeshPath;
    private Vector3[] waypoints;
    private int currentWaypointIndex;
    private Vector3 targetPosition;

    protected new virtual void Awake() {
        base.Awake();
        playerTransform = FindObjectOfType<PlayerController>().transform;
        objectToLookAt = new GameObject();
        objectToLookAt.name = "AI Look At";
    }

    protected new virtual void Start() {
        base.Start();
        navMeshPath = new NavMeshPath();
        waypoints = new Vector3[0];
    }

    private void Update() {
        objectToLookAt.transform.position = playerTransform.position + Vector3.up * 1.2f;
        orientation.CharacterRotation(inputDirection, objectToLookAt.transform);
        if (GetComponent<AIStimulus>().CanSeeTarget(playerTransform)) {
            combatManager.ReleaseGunTrigger();
            combatManager.FireEquippedWeapon();
            if (combatManager.EquippedWeapon.RemainingBullets == 0) {
                combatManager.SwitchToSecondary();
            }
            
            targetPosition = FindHideSpot();
            SetNewPath(targetPosition);
        }
    }

    private void FixedUpdate() {
        if (FollowNavMeshPath(targetPosition, distanceFromTarget: 0f, out inputDirection)) {
            movement.MoveTowardsDirection(inputDirection, run: false);
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

    private Vector3 FindHideSpot() {
        List<Transform> targetDistanceFilter = AIPositionFinder.FilterByDistance(hideSpots, playerTransform, 20f);
        List<Transform> myDistanceFilter = AIPositionFinder.FilterByDistance(targetDistanceFilter, transform, 10f);
        List<Transform> dotFilter = AIPositionFinder.FilterByDotProduct(myDistanceFilter, transform, playerTransform);
        List<Transform> losFilter = AIPositionFinder.FilterByLineOfSight(dotFilter, playerTransform);
        List<Transform> positionFilter = AIPositionFinder.FilterByObjectTowardsTarget(losFilter, playerTransform, 2f);
        return Environment.FindClosestPosition(transform.position, positionFilter);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected() {
        if (AIPositionFinder.invalidPoints.Count > 0) {
            foreach (var point in AIPositionFinder.invalidPoints) {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(point, AIPositionFinder.sphereRadius);
            }
            foreach (var point in AIPositionFinder.validPoints) {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(point, AIPositionFinder.sphereRadius);
            }
        }
        if (!UnityEditor.EditorApplication.isPlaying && (AIPositionFinder.invalidPoints.Count > 0 || AIPositionFinder.validPoints.Count > 0)) {
            AIPositionFinder.validPoints.Clear();
            AIPositionFinder.invalidPoints.Clear();
        }
    }
#endif
}
