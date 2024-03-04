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

    public bool IsGrounded => isGrounded;

    private CapsuleCollider m_Collider;
    private bool isGrounded = true;

    private void Awake() {
        m_Collider = GetComponent<CapsuleCollider>();
    }

    private void Update() {
        Bounds colliderBounds = m_Collider.bounds;
        colliderBounds.Expand(-2f * skinWidth);
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

    private Vector3 ProjectAndScale(Vector3 leftover, Vector3 normal) {
        float magnitude = leftover.magnitude;
        leftover = Vector3.ProjectOnPlane(leftover, normal);
        leftover.Normalize();
        leftover *= magnitude;
        return leftover;
    }

    private void VerticalCollision() {
        Bounds colliderBounds = m_Collider.bounds;
        Vector3 origin = colliderBounds.center - Vector3.up * (m_Collider.height * 0.5f - m_Collider.radius);
        Vector3 direction = Vector3.down;
        Ray ray = new Ray(origin, direction);
        isGrounded = Physics.SphereCast(ray, m_Collider.radius, groundCheckDistance, collisionLayers);
    }
}