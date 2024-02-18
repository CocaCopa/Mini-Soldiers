using UnityEngine;
using CocaCopa;

public class CharacterMovement : MonoBehaviour {

    [Header("--- Movement Speed ---")]
    [Tooltip("The character's walking speed.")]
    [SerializeField] private float walkSpeed;
    [Tooltip("The character's running speed.")]
    [SerializeField] private float runSpeed;

    [Header("--- Movement Curves ---")]
    [Tooltip("Animation curve to be used when the character is accelerating.")]
    [SerializeField] private AnimationCurve accelerationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [Tooltip("Animation curve to be used when the character is descelerating.")]
    [SerializeField] private AnimationCurve descelerationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [Tooltip("The evaluation speed of the acceleration curve.")]
    [SerializeField] private float evaluateAcceleration;
    [Tooltip("The evaluation speed of the desceleration curve.")]
    [SerializeField] private float evaluateDesceleration;

    private Rigidbody characterRb;
    private CapsuleCollider characterCollider;
    private AnimationCurve movementCurve = AnimationCurve.Constant(0, 0, 0);
    private Vector3 lastDirectionalInput;

    private float moveCurveEvaluationSpeed;
    private float accelerationPoints;
    private bool accelerateCurve;
    
    private float characterSpeed;
    private float minMoveSpeed;
    private float maxMoveSpeed;

    private bool canSetStopParameters = true;
    private bool canSetRunParameters = true;
    private bool canSetWalkParameters = false;

    public float CurrentSpeed => characterRb.velocity.magnitude;

    private void Awake() {
        characterRb = GetComponent<Rigidbody>();
        characterCollider = GetComponent<CapsuleCollider>();
    }

    /// <summary>
    /// Moves the kinematic character towards the given direction.
    /// </summary>
    /// <param name="direction">Direction to move towards.</param>
    /// <param name="run">True, will sprint, otherwise walk.</param>
    /// <param name="handleCollisions"></param>
    public void MoveTowardsDirection(Vector3 direction, bool run, bool handleCollisions = true) {
        CalculateCharacterSpeed(direction, run);
        float interpolationTime = Utilities.EvaluateAnimationCurve(movementCurve, ref accelerationPoints, moveCurveEvaluationSpeed, accelerateCurve);
        characterSpeed = Mathf.Lerp(minMoveSpeed, maxMoveSpeed, interpolationTime);

        Vector3 inputDirection = Vector3.forward * lastDirectionalInput.y + Vector3.right * lastDirectionalInput.x;
        inputDirection.Normalize();
        if (handleCollisions) {
            inputDirection = AdjustMoveDirection(inputDirection);
        }
        Vector3 additivePosition = characterSpeed * Time.fixedDeltaTime * inputDirection;
        Vector3 movePosition = characterRb.position + additivePosition;

        characterRb.MovePosition(movePosition);
    }

    private Vector3 AdjustMoveDirection(Vector3 moveDirection) {
        if (!PlayerCanMove(moveDirection)) {
            Vector3 moveDirectionX = new Vector3(moveDirection.x, 0, 0).normalized;
            if (PlayerCanMove(moveDirectionX)) {
                // Can move only on the X axis
                return moveDirectionX;
            }
            else {
                // Attempt only Z movement
                Vector3 moveDirectionZ = new Vector3(0, 0, moveDirection.z).normalized;
                if (PlayerCanMove(moveDirectionZ)) {
                    // Can move only on the Z axis
                    return moveDirectionZ;
                }
            }
        }

        return moveDirection;
    }

    private bool PlayerCanMove(Vector3 moveDir) {
        float playerRadius = characterCollider.radius;
        float playerHeight = characterCollider.height;
        float moveDistance = runSpeed * Time.deltaTime;
        Vector3 feetPosition = transform.position;
        Vector3 headPosition = feetPosition + Vector3.up * playerHeight;

        return !Physics.CapsuleCast(feetPosition, headPosition, playerRadius, moveDir, moveDistance);
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
        movementCurve = descelerationCurve;
        moveCurveEvaluationSpeed = evaluateDesceleration;
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
        movementCurve = descelerationCurve;
        moveCurveEvaluationSpeed = evaluateDesceleration;
        accelerateCurve = false;
    }
}
