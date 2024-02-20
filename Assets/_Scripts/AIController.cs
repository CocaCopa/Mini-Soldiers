using UnityEngine;
using UnityEngine.AI;
using CocaCopa;

public class AIController : Controller {

    private Transform playerTransform;
    private Vector2 inputDirection;

    private NavMeshPath navMeshPath;
    private Vector3[] waypoints;
    private int currentWaypointIndex;

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
            //SetNewPath(playerTransform.position);
            combatManager.FireEquippedWeapon();
            if (combatManager.EquippedWeapon.RemainingBullets == 0) {
                combatManager.SwitchToSecondary();
            }
        }
    }

    private void FixedUpdate() {
        if (FollowNavMeshPath(playerTransform.position, out inputDirection)) {
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

    private bool FollowNavMeshPath(Vector3 targetPosition, out Vector2 direction) {
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

                if (Vector3.Distance(transform.position, targetPosition) <= 5) {
                    direction = Vector3.zero;
                }
                else {
                    direction = new Vector2(moveDirection.x, moveDirection.z).normalized;
                }
            }
        }
        return true;
    }
}
