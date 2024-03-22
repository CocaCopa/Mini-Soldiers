using UnityEngine;

public class CollisionDetection : MonoBehaviour {

    // ---------------------------------------- Recommended use ---------------------------------------- //
    // private void Move(Vector3 moveAmount) {                                                           //
    //     //..                                                                                          //
    //     moveAmount = CollideAndSlide(moveAmount, transform.position, 0, false, moveAmount);           //
    //     moveAmount += CollideAndSlide(gravity, transform.position + moveAmount, 0, true, gravity);    //
    //     //..                                                                                          //
    // }                                                                                                 //
    // ------------------------------------------------------------------------------------------------- //

    [Tooltip("Defines the layers that the character can collide with.")]
    [SerializeField] private LayerMask collisionLayers;

    [Header("--- Movement Check ---")]
    [Tooltip("")]
    [SerializeField] private float stepHeight = 0.15f;
    [Tooltip("Standing in a slope with an angle greater than this value will cause the character to slide.")]
    [SerializeField] private float maxSlopeAngle = 55f;
    [Tooltip("Maximum number of recursive bounces allowed during collision handling.")]
    [SerializeField] private int maxBounces = 5;
    [Tooltip("Thickness of the character's collision skin. Helps prevent tunneling and improves collision detection accuracy.")]
    [SerializeField] private float skinWidth = 0.015f;
    [Tooltip("The minimum speed of the character upon colliding with a wall.")]
    [SerializeField] private float minSpeedScale = 0.15f;

    [Header("--- Ground Check ---")]
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField, Range(0f,1f)] private float timeScale = 1.0f;

    public bool IsGrounded => isGrounded;

    private CapsuleCollider m_Collider;
    private bool isGrounded = true;

    private void Awake() {
        m_Collider = GetComponent<CapsuleCollider>();
    }

    private void Update() {
        Bounds colliderBounds = m_Collider.bounds;
        colliderBounds.Expand(-2f * skinWidth);
        Time.timeScale = timeScale;
    }

    private void FixedUpdate() {
        VerticalCollision();
    }

    /// <summary>
    /// Handles collisions for characters using a capsule collider.
    /// </summary>
    /// <param name="velocity">The current velocity of the character.</param>
    /// <param name="characterPivot">The pivot point position of the character that the collider is attached to.</param>
    /// <param name="depth">The recursion depth to prevent infinite loops. (Should always be 0)</param>
    /// <param name="gravityPass">True if the function is being called to handle gravity velocity, otherwise false.</param>
    /// <param name="initialVelocity">The velocity of the character before the function call.</param>
    /// <returns>The adjusted velocity after collision handling.</returns>
    public Vector3 CollideAndSlide(Vector3 velocity, Vector3 characterPivot, int depth, bool gravityPass, Vector3 initialVelocity) {
        if (depth >= maxBounces) {
            return Vector3.zero;
        }

        if (CollisionTowardsVelocity(characterPivot, velocity, out RaycastHit hitInfo)) {
            Vector3 snapToSurface = velocity.normalized * (hitInfo.distance - skinWidth);
            Vector3 leftover = velocity - snapToSurface;
            float angle = Vector3.Angle(Vector3.up, hitInfo.normal);

            if (snapToSurface.magnitude <= skinWidth) {
                snapToSurface = Vector3.zero;
            }

            if (angle <= maxSlopeAngle) {
                if (gravityPass) {
                    return snapToSurface;
                }
                leftover = ProjectAndScale(leftover, hitInfo.normal);
            }
            else {

                float scale = 1 - Vector3.Dot(
                    new Vector3(hitInfo.normal.x, 0f, hitInfo.normal.z).normalized,
                    -new Vector3(initialVelocity.x, 0f, initialVelocity.z).normalized
                );
                scale = Mathf.Clamp(scale, minSpeedScale, 1f);

                if (isGrounded && !gravityPass) {
                    leftover = ProjectAndScale(
                        new Vector3(leftover.x, 0f, leftover.z),
                        new Vector3(hitInfo.normal.x, 0f, hitInfo.normal.z)
                    );
                    leftover *= scale;
                }
                else {
                    leftover = ProjectAndScale(leftover, hitInfo.normal);
                    leftover *= scale;
                }

                if (StepHeight(velocity.normalized, hitInfo.point, out float height)) {
                    /*Vector3 newPos = hitInfo.point + Vector3.up * 0.01f;
                    Vector3 pos = transform.position;
                    pos.y = newPos.y;
                    transform.position = pos;*/
                    velocity.y = height * 1.01f;
                    leftover.y = height * 1.01f;
                }
            }

            return snapToSurface + CollideAndSlide(leftover, characterPivot + snapToSurface, depth + 1, gravityPass, initialVelocity);
        }
        return velocity;
    }

    private bool CollisionTowardsVelocity(Vector3 characterPivot, Vector3 characterVelocity, out RaycastHit hitInfo) {
        Vector3 bottomPoint = characterPivot + Vector3.up * m_Collider.radius;
        Vector3 topPoint = m_Collider.bounds.center + Vector3.up * (m_Collider.height * 0.5f - m_Collider.radius);
        Vector3 direction = characterVelocity.normalized;
        float distance = characterVelocity.magnitude + skinWidth;
        float radius = m_Collider.radius;
        LayerMask layerMask = collisionLayers;

        return Physics.CapsuleCast(bottomPoint, topPoint, radius, direction, out hitInfo, distance, layerMask);
    }

    private void VerticalCollision() {
        Bounds colliderBounds = m_Collider.bounds;
        Vector3 origin = colliderBounds.center - Vector3.up * (m_Collider.height * 0.5f - m_Collider.radius);
        Vector3 direction = Vector3.down;
        Ray ray = new Ray(origin, direction);
        isGrounded = Physics.SphereCast(ray, m_Collider.radius, groundCheckDistance, collisionLayers);
    }

    private bool StepHeight(Vector3 moveDirection, Vector3 hitpoint, out float height) {
        Vector3 origin = hitpoint;
        origin.y = m_Collider.bounds.center.y + m_Collider.height * 0.5f;
        Debug.DrawRay(origin, Vector3.down, Color.red);
        Vector3 direction = Vector3.down;
        Ray ray = new Ray(origin, direction);
        float distance = m_Collider.height;
        height = 0f;

        if (Physics.SphereCast(ray, 0.25f, out RaycastHit hitInfo, distance, collisionLayers)) {
            Debug.DrawRay(hitInfo.point, hitInfo.normal * 5f, Color.yellow, 5f);
            Debug.DrawRay(transform.position, moveDirection * 5f, Color.magenta);
            //if (Vector3.Angle(hitInfo.normal, moveDirection) == 90f || Mathf.Approximately(Vector3.Angle(hitInfo.normal, moveDirection), 90f)) {
                if (hitInfo.transform.TryGetComponent<Collider>(out var hitCollider)) {
                    height = hitInfo.point.y - transform.position.y;
                    return hitCollider.bounds.max.y - m_Collider.bounds.min.y <= stepHeight || hitInfo.point.y - transform.position.y <= stepHeight;
                }
            //}
        }
        return false;
    }
    
    private Vector3 ProjectAndScale(Vector3 leftover, Vector3 normal) {
        float magnitude = leftover.magnitude;
        leftover = Vector3.ProjectOnPlane(leftover, normal);
        leftover.Normalize();
        leftover *= magnitude;
        return leftover;
    }

    private bool Step(RaycastHit hitInfo, Vector3 moveDirection, out float height) {
        Vector3 origin = hitInfo.point;
        origin.y = m_Collider.center.y + m_Collider.height * 0.5f;
        height = 0f;
        if (Physics.Raycast(new Ray(origin, Vector3.down), out RaycastHit hit, m_Collider.height - 0.1f, collisionLayers)) {
            Debug.DrawRay(origin, Vector3.down * hit.distance, Color.magenta);
            if (m_Collider.height - hit.distance <= stepHeight) {
                Debug.Log("step");
                height = hit.point.y;
            }
        }
        return height != 0f;

        /*Vector3 origin = moveDirection.normalized * 0.5f + m_Collider.bounds.center + Vector3.up * m_Collider.height * 0.5f;
        Vector3 direction = Vector3.down;
        Ray ray = new Ray(origin, direction);
        float distance = m_Collider.height;
        Debug.DrawRay(origin, direction * distance, Color.yellow);
        if (Physics.Raycast(ray, out hitInfo, distance, collisionLayers)) {
            if (hitInfo.transform.TryGetComponent<Collider>(out var hitCollider)) {
                if (hitCollider.bounds.max.y - m_Collider.bounds.min.y <= stepHeight || hitInfo.point.y - transform.position.y <= stepHeight)
                    Debug.Log("Step");
            }
        }*/
    }
}
