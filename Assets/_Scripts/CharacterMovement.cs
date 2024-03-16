using UnityEngine;
using CocaCopa.Utilities;

public class CharacterMovement : MonoBehaviour {

    [Tooltip("If enabled, the code will also handle collisions for the given character, otherwise collisions will be ignored.")]
    [SerializeField] private bool handleCollisions;

    [Header("--- Movement Speed ---")]
    [Tooltip("The character's walking speed.")]
    [SerializeField] private float walkSpeed;
    [Tooltip("The character's running speed.")]
    [SerializeField] private float runSpeed;

    [Header("--- Movement Curves ---")]
    [Tooltip("Animation curve to be used when the character is accelerating.")]
    [SerializeField] private AnimationCurve accelerationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [Tooltip("Animation curve to be used when the character is descelerating.")]
    [SerializeField] private AnimationCurve decelerationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [Tooltip("The evaluation speed of the acceleration curve.")]
    [SerializeField] private float evaluateAcceleration;
    [Tooltip("The evaluation speed of the desceleration curve.")]
    [SerializeField] private float evaluateDeceleration;

    private Rigidbody characterRb;
    private CollisionDetection collisionDetection;

    private AnimationCurve movementCurve = AnimationCurve.Constant(0, 0, 0);
    private Vector3 lastDirectionalInput;
    private Vector3 gravity;

    private float moveCurveEvaluationSpeed;
    private float accelerationPoints;
    private bool accelerateCurve;
    
    private float characterSpeed;
    private float minMoveSpeed;
    private float maxMoveSpeed;

    private bool canSetStopParameters = true;
    private bool canSetRunParameters = true;
    private bool canSetWalkParameters = false;

    public float CurrentSpeed => characterSpeed;

    private void Awake() {
        characterRb = GetComponent<Rigidbody>();
        if (!TryGetComponent(out collisionDetection) && handleCollisions) {
            handleCollisions = false;
            Debug.LogWarning(name + ":\n" + nameof(CollisionDetection) + " component is not attached to the given character. Collisions will be ignored.");
        }
    }
    
    private void FixedUpdate() {
        CalculateGravity();
    }

    /// <summary>
    /// Moves the kinematic character towards the given direction.
    /// </summary>
    /// <param name="direction">Direction to move towards.</param>
    /// <param name="run">True, will sprint, otherwise walk.</param>
    public void MoveTowardsDirection(Vector2 direction, bool run, Vector3 relativeForward = default, Vector3 relativeRight = default) {
        if (relativeForward == default) {
            relativeForward = Vector3.forward;
        }
        if (relativeRight == default) {
            relativeRight = Vector3.right;
        }

        CalculateCharacterSpeed(direction, run);
        float interpolationTime = Common.EvaluateAnimationCurve(movementCurve, ref accelerationPoints, moveCurveEvaluationSpeed, accelerateCurve);
        characterSpeed = Mathf.Lerp(minMoveSpeed, maxMoveSpeed, interpolationTime);

        Vector3 inputDirection = relativeForward * lastDirectionalInput.y + relativeRight * lastDirectionalInput.x;
        inputDirection.Normalize();
        
        Vector3 additivePosition = characterSpeed * Time.fixedDeltaTime * inputDirection;
        if (handleCollisions) {
            additivePosition = collisionDetection.CollideAndSlide(additivePosition, characterRb.position, 0, false, additivePosition);
            additivePosition += collisionDetection.CollideAndSlide(gravity, characterRb.position + additivePosition, 0, true, gravity);
        }
        Vector3 movePosition = characterRb.position + additivePosition;
        characterRb.MovePosition(movePosition);
    }

    private void CalculateCharacterSpeed(Vector3 direction, bool run) {
        if (direction != Vector3.zero) {
            lastDirectionalInput = direction;
            if (run && canSetRunParameters) {
                SetRunParameters();
                canSetWalkParameters = true;
                canSetRunParameters = false;
            }
            else if (!run) {
                if (canSetWalkParameters) {
                    DescelerateToWalkSpeed();
                    canSetWalkParameters = false;
                    canSetRunParameters = true;
                }
                else if (characterSpeed < walkSpeed) {
                    SetWalkParameters();
                }
            }
            canSetStopParameters = true;
        }
        else if (canSetStopParameters) {
            StopMoving();
            canSetRunParameters = true;
            canSetStopParameters = false;
        }
    }

    /// <summary>
    /// Change the speed of the character to 'running' speed.
    /// </summary>
    private void SetRunParameters() {
        accelerationPoints = 0f;
        minMoveSpeed = characterSpeed;
        maxMoveSpeed = runSpeed;
        movementCurve = accelerationCurve;
        moveCurveEvaluationSpeed = evaluateAcceleration;
        accelerateCurve = true;
    }

    /// <summary>
    /// Change the speed of the character to 'walking' speed.
    /// </summary>
    private void SetWalkParameters() {
        minMoveSpeed = 0f;
        maxMoveSpeed = walkSpeed;
        movementCurve = accelerationCurve;
        moveCurveEvaluationSpeed = evaluateAcceleration;
        accelerateCurve = true;
    }

    /// <summary>
    /// Descelerate the character's speed from running to walking.
    /// </summary>
    private void DescelerateToWalkSpeed() {
        minMoveSpeed = walkSpeed;
        maxMoveSpeed = characterSpeed;
        movementCurve = decelerationCurve;
        moveCurveEvaluationSpeed = evaluateDeceleration;
        accelerateCurve = false;
    }

    /// <summary>
    /// Descelerate the character's speed to 0.
    /// </summary>
    private void StopMoving() {
        if (characterSpeed > 0f && accelerationPoints == 0f) {
            accelerationPoints = 1f;
        }
        canSetStopParameters = false;
        minMoveSpeed = 0f;
        maxMoveSpeed = characterSpeed;
        movementCurve = decelerationCurve;
        moveCurveEvaluationSpeed = evaluateDeceleration;
        accelerateCurve = false;
    }

    private void CalculateGravity() {
        if (handleCollisions) {
            if (collisionDetection.IsGrounded) {
                gravity = Vector3.zero;
            }
            gravity += 0.25f * Time.fixedDeltaTime * Physics.gravity;
        }
    }
}
