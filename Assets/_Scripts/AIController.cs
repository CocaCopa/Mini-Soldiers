using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using CocaCopa.Utilities;
using System.Linq;

public abstract class AIController : Controller {

    [SerializeField] private List<Transform> hideSpots;

    private const string ENEMY_LAYER_NAME = "Enemy";
    private const string PLAYER_LAYER_NAME = "Player";
    private const string GROUND_LAYER_NAME = "Ground";

    private NavMeshPath navMeshPath;
    private Vector3[] waypoints;
    private int currentWaypointIndex;

    protected AI_State State { get; set; }
    protected PlayerSpotted_SubState PlayerSpottedState { get; set; }
    protected AIStimulus Stimulus { get; private set; }
    protected float StoppingDistance { get; set; }
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

    /// <summary>
    /// Determines the nearest obstacle ahead of the AI and computes an optimal position for the AI to peek around the corner.
    /// </summary>
    /// <param name="peekCornerDistance">The distance the AI should extend from the corner.</param>
    /// <returns>The calculated position for peeking around the corner.</returns>
    protected Vector3 CalculatePeekPosition(float peekCornerDistance) {
        Vector3[] wallEdges = FindClosestWall();
        Vector3 closestEdge = Environment.FindClosestPosition(transform.position, wallEdges.ToList());
        closestEdge.y = transform.position.y;
        Vector3 edgeToEdgeDirection = (wallEdges[0] - wallEdges[1]).normalized;
        Vector3 directionToEdge = (closestEdge - transform.position).normalized;
        Vector3 peekDirection = Vector3.Dot(edgeToEdgeDirection, directionToEdge) > 0f
                ? edgeToEdgeDirection
                : -edgeToEdgeDirection;
        float distanceToEdge = (closestEdge - transform.position).magnitude;
        return transform.position + peekDirection * (distanceToEdge + peekCornerDistance);
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
    /// Finds the closest wall to the AI from its standing position.
    /// </summary>
    /// <param name="debugWallEdges">True, will draw rays from the detected edges to the upward direction.</param>
    /// <returns>The an array containing the edges of the found wall as vectors.</returns>
    private Vector3[] FindClosestWall(bool debugWallEdges = false) {
        int excludeLayers = 1 << LayerMask.NameToLayer(ENEMY_LAYER_NAME) | 1 << LayerMask.NameToLayer(GROUND_LAYER_NAME) | 1 << LayerMask.NameToLayer(PLAYER_LAYER_NAME);
        int checkLayers = ~excludeLayers;
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f, checkLayers);
        List<Vector3> objectEdges = new List<Vector3>();
        Vector3 closestEdge = Vector3.zero;
        foreach (var collider in colliders) {
            objectEdges = Environment.GetObjectEdges(collider.transform);
            closestEdge = Environment.FindClosestPosition(transform.position, objectEdges);
        }
        Vector3 sideEdge = FindWallSideEdge(objectEdges, closestEdge);
        if (debugWallEdges) {
            Debug.DrawRay(closestEdge, Vector3.up * 100f, Color.magenta, float.MaxValue);
            Debug.DrawRay(sideEdge, Vector3.up * 100f, Color.green, float.MaxValue);
        }
        return new Vector3[] {
            closestEdge, sideEdge
        };
    }

    /// <summary>
    /// Locates the opposite edge of the wall facing the AI.
    /// </summary>
    /// <param name="objectEdges">Edges of the object in front of the AI.</param>
    /// <param name="closestEdge">The closest edge detected.</param>
    /// <returns>The opposite edge of the wall.</returns>
    private Vector3 FindWallSideEdge(List<Vector3> objectEdges, Vector3 closestEdge) {
        List<Vector3> oppositeEdgesPositions = new List<Vector3>();
        List<Vector3> remainingEdges = new List<Vector3>();
        foreach (var edge in objectEdges) {
            if (edge != closestEdge) {
                remainingEdges.Add(edge);
            }
        }
        Vector3 closestEdgeToQuerierDirection = (transform.position - closestEdge).normalized;
        closestEdgeToQuerierDirection.y = closestEdge.y;
        foreach (var edge in remainingEdges) {
            Vector3 edgeToClosestEdgeDirection = (transform.position - edge).normalized;
            edgeToClosestEdgeDirection.y = closestEdge.y;
            if (Vector3.Dot(closestEdgeToQuerierDirection, edgeToClosestEdgeDirection) <= 0f) {
                oppositeEdgesPositions.Add(edge);
            }

            if (Vector3.Dot(closestEdgeToQuerierDirection, edgeToClosestEdgeDirection) >= 0f && Vector3.Dot(closestEdgeToQuerierDirection, edgeToClosestEdgeDirection) <= 0.90f) {
                oppositeEdgesPositions.Add(edge);
            }
        }
        return Environment.FindClosestPosition(transform.position, oppositeEdgesPositions);
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
