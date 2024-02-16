using UnityEngine;
using CocaCopa;

public enum MoveState { 
    Walking,
    Running
};

public class CharacterMovement : MonoBehaviour {

    [Header("--- Movement Speed ---")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;

    [Header("--- Movement Curves ---")]
    [SerializeField] private AnimationCurve accelerationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private AnimationCurve descelerationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private float evaluateAcceleration;
    [SerializeField] private float evaluateDesceleration;

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

    public float CurrentSpeed => characterSpeed;

    /// <summary>
    /// Moves the character towards the given direction.
    /// </summary>
    /// <param name="direction">Direction to move towards.</param>
    /// <param name="run">True, will sprint, otherwise walk.</param>
    public void MoveTowardsDirection(Vector3 direction, bool run) {
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
        

        float interpolationTime = Utilities.EvaluateAnimationCurve(movementCurve, ref accelerationPoints, moveCurveEvaluationSpeed, accelerateCurve);
        characterSpeed = Mathf.Lerp(minMoveSpeed, maxMoveSpeed, interpolationTime);

        Vector3 inputDirection = Vector3.forward * lastDirectionalInput.y + Vector3.right * lastDirectionalInput.x;
        inputDirection.Normalize();

        Vector3 additivePosition = characterSpeed * Time.deltaTime * inputDirection;
        transform.position += additivePosition;
    }

    private void UpdateCharacterSpeed() {

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
