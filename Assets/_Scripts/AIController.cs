using CocaCopa;
using UnityEngine;
using UnityEngine.AI;

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
        SetNewPath(playerTransform.position);
        DebugAIWeaponShoot();
    }

    float time = 1f;
    float timer = 0f;
    int index;

    private void FixedUpdate() {
        if (FollowNavMeshPath(playerTransform.position, out inputDirection)) {
            movement.MoveTowardsDirection(inputDirection, false);
        }
    }

    private void DebugAIWeaponShoot() {
        if (Utilities.TickTimer(ref timer, time) && combatManager.EquippedWeapon.RemainingBullets != 0) {
            if (index > 1) {
                index = 0;
            }
            if (index == 0) {
                combatManager.SwitchToPrimary();
            }
            if (index == 1) {
                combatManager.SwitchToSecondary();
            }
            index++;
        }
        if (!combatManager.IsSwitchingWeapon)
            combatManager.FireEquippedWeapon(autoReload: true);
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

    /// <summary>
    /// Guides the agent along a NavMesh path, providing the direction of movement.
    /// </summary>
    /// <param name="direction">Out parameter for the direction of movement.</param>
    /// <returns>True if the entity should continue following the path, false otherwise.</returns>
    private bool FollowNavMeshPath(Vector3 targetPosition, out Vector2 direction) {
        if (waypoints.Length == 0) {
            direction = Vector2.zero;
            Debug.LogError("No path has been calculated but the AI is trying to follow one.");
            return false;
        }
        if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex]) < 0.1f) {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length) {
                
            }
        }

        Vector3 moveDirection = (waypoints[currentWaypointIndex] - transform.position).normalized;
        if (Vector3.Distance(transform.position, targetPosition) <= 5) {
            direction = Vector3.zero;
        }
        else {
            direction = new Vector2(moveDirection.x, moveDirection.z).normalized;
        }
        return true;
    }
}
