using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using CocaCopa.Utilities;
using System.Linq;

public abstract class AIController : Controller {

    [SerializeField] private List<Transform> hideSpots;

    private const string WALL_COLLIDER_LAYER_NAME = "AI Wall Collider";

    private NavMeshPath navMeshPath;
    private Collider m_Collider;
    private Vector3[] waypoints;
    private int currentWaypointIndex;

    [Tooltip("- Patrol\r\n- Investigate\r\n- PlayerSpotted")]
    [SerializeField] private AI_State mainState = AI_State.Unassigned;
    [Tooltip("- FindNewHideSpot\r\n- Hide\r\n- PeekCorner")]
    [SerializeField] private PlayerSpotted_SubState playerSpottedState = PlayerSpotted_SubState.Unassigned;
    [Tooltip("- Cover\r\n- Peek\r\n- Reload")]
    [SerializeField] private PeekCorner_SubState peekCornerState = PeekCorner_SubState.Unassigned;

    protected AI_State MainState { get => mainState; set => mainState = value; }
    protected PlayerSpotted_SubState PlayerSpottedState { get => playerSpottedState; set => playerSpottedState = value; }
    protected PeekCorner_SubState PeekCornerState { get => peekCornerState; set => peekCornerState = value; }
    protected AIStimulus Stimulus { get; private set; }
    protected Collider MyCollider => m_Collider;
    protected float StoppingDistance { get; set; }
    protected Transform PlayerTransform { get; private set; }
    protected bool ReachedPathDestination { get; private set; }

    protected new virtual void Awake() {
        base.Awake();
        m_Collider = GetComponent<Collider>();
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
        DirectionalInput = IsAlive ? NavMeshMovementDirection(StoppingDistance) : Vector3.zero;
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
    /// Caclulates the position of the closest hide spot.
    /// </summary>
    /// <returns>The position of the closest hide spot.</returns>
    protected Vector3 ClosestHideSpot() {
        return Environment.FindClosestPosition(transform.position, hideSpots);
    }

    /// <summary>
    /// Filters the provided hide spots and picks the best position to hide from the provided target.
    /// </summary>
    /// <param name="againstTarget">Target transform to filter the environment against.</param>
    /// <returns></returns>
    protected Vector3 FindHideSpot(Transform againstTarget) {
        // Select spots 20m away from the player.
        List<Transform> filteredHideSpots = AIPositionFinder.FilterByDistance(hideSpots, againstTarget, 20f);
        // Select spots 10m away from the AI.
        filteredHideSpots = AIPositionFinder.FilterByDistance(filteredHideSpots, transform, 10f);
        // 
        filteredHideSpots = AIPositionFinder.FilterByDotProduct(filteredHideSpots, transform, againstTarget, CompareDotProduct.GreaterThan, dotProduct: 0f);
        // Select spots not seeable by the player.
        filteredHideSpots = AIPositionFinder.FilterByLineOfSight(filteredHideSpots, againstTarget);
        // Select spots that have an object towards the player's position.
        filteredHideSpots = AIPositionFinder.FilterByObjectTowardsTarget(filteredHideSpots, againstTarget, 2f);
        return Environment.FindClosestPosition(transform.position, filteredHideSpots);
    }

    /// <summary>
    /// Determines the nearest obstacle ahead of the AI and computes an optimal position for the AI to peek around the corner.
    /// </summary>
    /// <param name="peekCornerDistance">The distance the AI should extend from the corner.</param>
    /// <returns>The calculated position for peeking around the corner.</returns>
    protected Vector3 CalculatePeekPosition(float peekCornerDistance, out Vector3 peekDirection) {
        Vector3[] wallEdges = FindClosestWall(true);
        if (wallEdges.Length > 0) {
            Vector3 closestEdge = Environment.FindClosestPosition(transform.position, wallEdges.ToList());
            closestEdge.y = transform.position.y;
            Vector3 edgeToEdgeDirection = (wallEdges[0] - wallEdges[1]).normalized;
            Vector3 directionToEdge = (closestEdge - transform.position).normalized;
            peekDirection = Vector3.Dot(edgeToEdgeDirection, directionToEdge) > 0f
                    ? edgeToEdgeDirection
                    : -edgeToEdgeDirection;
            float distanceToEdge = (closestEdge - transform.position).magnitude;
            return transform.position + peekDirection * (distanceToEdge + peekCornerDistance);
        }
        else {
            peekDirection = Vector3.zero;
            return Vector3.zero;
        }
    }

    /// <summary>
    /// Calculates the movement direction for navigating on the NavMesh towards the next waypoint.
    /// </summary>
    /// <param name="distanceFromTarget">Stop within the specified distance from the target position.</param>
    /// <returns>The normalized movement direction vector towards the next waypoint.</returns>
    private Vector3 NavMeshMovementDirection(float distanceFromTarget) {
        Vector3 direction = Vector2.zero;
        if (currentWaypointIndex < waypoints.Length) {
            if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex]) <= 0.25f) {
                currentWaypointIndex++;
                if (currentWaypointIndex >= waypoints.Length) {
                    ReachedPathDestination = true;
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
    /// Finds the closest collider to the AI from its standing position.
    /// </summary>
    /// <param name="debugColliderEdges">True, will draw rays from the detected edges to the upward direction.</param>
    /// <returns>An array containing the edges of the found wall as vectors.</returns>
    protected Vector3[] FindClosestWall(bool debugColliderEdges = false) {
        int layerMask = LayerMask.GetMask(WALL_COLLIDER_LAYER_NAME);
        float overlapDistance = 2.5f;
        Collider[] colliders = Physics.OverlapSphere(m_Collider.bounds.center, overlapDistance, layerMask, QueryTriggerInteraction.Collide);
        Collider closestCollider = null;
        float closestDistance = Mathf.Infinity;
        foreach (Collider collider in colliders) {
            Vector3 closestPoint = collider.ClosestPoint(m_Collider.bounds.center);
            float distance = (closestPoint - m_Collider.bounds.center).sqrMagnitude;

            if (distance < closestDistance) {
                closestDistance = distance;
                closestCollider = collider;
            }
        }

        if (closestCollider != null) {
            List<Vector3> edges = FindBoxColliderFaceEdges(closestCollider, debugColliderEdges);
            return edges.ToArray();
        }
        else {
            Debug.LogWarning("No colliders found. Make sure the colliders representing a wall are on the '" + WALL_COLLIDER_LAYER_NAME + "' layer and/or the AI's distance from the collider is less than " + overlapDistance + " units.");
        }

        return new Vector3[0];
    }

    /// <summary>
    /// Finds the edges of a face on a collider closest to the AI's position.
    /// </summary>
    /// <param name="collider">The collider whose face edges are to be found.</param>
    /// <returns>A list of Vector3 representing the edges of the face closest to the AI's position.</returns>
    private List<Vector3> FindBoxColliderFaceEdges(Collider collider, bool debug = false) {
        Vector3 origin = m_Collider.bounds.center;
        Vector3 closestPoint = collider.ClosestPoint(origin);
        Vector3 direction = (origin - closestPoint).normalized;

        Vector3 crossDirection = Vector3.Cross(direction, Vector3.up).normalized;

        if (debug) {
            Debug.DrawRay(closestPoint, direction, Color.magenta);
            Debug.DrawRay(closestPoint, crossDirection, Color.yellow);
        }

        List<Vector3> objectEdges = Environment.GetBoxColliderEdges(collider as BoxCollider, debugCalculatedEdges: false);
        List<Vector3> faceEdges = new List<Vector3>();

        foreach (Vector3 edge in objectEdges) {
            Vector3 adustedEdge = edge;
            adustedEdge.y = closestPoint.y;
            Vector3 edgeToPointDirection = (closestPoint - adustedEdge).normalized;
            float dotProduct = Vector3.Dot(crossDirection, edgeToPointDirection);
            if (Mathf.Approximately(Mathf.Abs(dotProduct), 1f)) {
                faceEdges.Add(edge);
                if (debug) {
                    Debug.DrawRay(edge, Vector3.up * 100f, Color.green);
                }
            }
        }

        if (faceEdges.Count == 0) {
            Debug.LogWarning("Could not calculate collider edges");
        }
        return faceEdges;
    }

    public override void TakeDamage(float amount) {
        base.TakeDamage(amount);
        if (CurrentHealthPoints <= 0) {
            SetNewDestination(transform.position);
            MainState = AI_State.Dead;
            PlayerSpottedState = PlayerSpotted_SubState.Unassigned;
            PeekCornerState = PeekCorner_SubState.Unassigned;

        }
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
