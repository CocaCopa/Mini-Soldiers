using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using CocaCopa.Utilities;

public enum AI_State {
    Patrol,
    Investigate,
    PlayerSpotted,
    Combat,
};

public abstract class AIController : Controller {

    [SerializeField] private List<Transform> hideSpots;

    private NavMeshPath navMeshPath;
    private Vector3[] waypoints;
    private int currentWaypointIndex;

    protected AIStimulus Stimulus { get; private set; }
    protected float StoppingDistance { get; set; }
    protected AI_State State { get; set; }
    protected Transform PlayerTransform { get; private set; }
    protected bool ReachedPathDestination { get; private set; }

    protected new virtual void Awake() {
        base.Awake();
        Stimulus = GetComponent<AIStimulus>();
        PlayerTransform = FindObjectOfType<PlayerController>().transform;
    }

    protected new virtual void Start() {
        base.Start();
        navMeshPath = new NavMeshPath();
        waypoints = new Vector3[0];
    }

    protected new virtual void Update() {
        base.Update();
        DirectionalInput = NavMeshMovementDirection(StoppingDistance);
        RelativeForwardDir = Vector3.forward;
        RelativeRightDir = Vector3.right;
    }

    /// <summary>
    /// Calculates the movement direction for navigating on the NavMesh towards the next waypoint.
    /// </summary>
    /// <param name="distanceFromTarget">Stop within the specified distance from the target position.</param>
    /// <returns>The normalized movement direction vector towards the next waypoint.</returns>
    private Vector3 NavMeshMovementDirection(float distanceFromTarget) {
        Vector3 direction = Vector2.zero;

        if (currentWaypointIndex < waypoints.Length) {
            if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex]) < 0.1f) {
                currentWaypointIndex++;
                if (currentWaypointIndex >= waypoints.Length) {
                    ReachedPathDestination = true;
                    print("Reached the end of the path. Calculate a new path for the AI to start moving again.");
                }
                else {
                    ReachedPathDestination = false;
                }
            }

            if (currentWaypointIndex <= waypoints.Length - 1) {
                Vector3 moveDirection = (waypoints[currentWaypointIndex] - transform.position).normalized;

                if (Vector3.Distance(transform.position, waypoints[^1]) <= distanceFromTarget) {
                    direction = Vector3.zero;
                }
                else {
                    direction = new Vector2(moveDirection.x, moveDirection.z).normalized;
                }
            }
        }
        return direction;
    }

    /// <summary>
    /// Sets a new destination for the agent to navigate towards on the NavMesh.
    /// </summary>
    /// <param name="targetPosition">The position to navigate towards.</param>
    protected void SetNewDestination(Vector3 targetPosition) {
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
    /// Filters the provided hide spots and picks the best position to hide from the provided target.
    /// </summary>
    /// <param name="againstTarget">Target transform to filter the environment against.</param>
    /// <returns></returns>
    protected Vector3 FindHideSpot(Transform againstTarget) {
        List<Transform> targetDistanceFilter = AIPositionFinder.FilterByDistance(hideSpots, againstTarget, 20f);
        List<Transform> myDistanceFilter = AIPositionFinder.FilterByDistance(targetDistanceFilter, transform, 10f);
        List<Transform> dotFilter = AIPositionFinder.FilterByDotProduct(myDistanceFilter, transform, againstTarget);
        List<Transform> losFilter = AIPositionFinder.FilterByLineOfSight(dotFilter, againstTarget);
        List<Transform> positionFilter = AIPositionFinder.FilterByObjectTowardsTarget(losFilter, againstTarget, 2f);
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
