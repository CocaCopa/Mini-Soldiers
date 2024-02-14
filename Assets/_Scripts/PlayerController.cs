using UnityEngine;
using CocaCopa;
using TMPro;

public class PlayerController : MonoBehaviour {

    [SerializeField] private float movementSpeed;
    [SerializeField] private AnimationCurve accelerationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private AnimationCurve descelerationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private float evaluationSpeed;
    [SerializeField] private float rotationSpeed;

    public float MovementSpeed => characterSpeed;

    private PlayerInput input;
    private AnimationCurve movementCurve;
    private LookAtMouseAnimRig mouseAnimRig;
    private float horizontalInput;
    private float verticalInput;
    private float lastHorizontalInput;
    private float lastVerticalInput;
    private float accelerationPoints;
    private float characterSpeed;
    private bool accelerate;
    private Vector3 lookDirection;

    private void Awake() {
        input = FindObjectOfType<PlayerInput>();
        mouseAnimRig = GetComponentInChildren<LookAtMouseAnimRig>();
    }

    private void Update() {
        Movement();
        Rotation();
    }

    private void Movement() {
        horizontalInput = input.MovementInput().x;
        verticalInput = input.MovementInput().y;

        if (verticalInput != 0 || horizontalInput != 0) {
            lastHorizontalInput = horizontalInput;
            lastVerticalInput = verticalInput;
            movementCurve = accelerationCurve;
            accelerate = true;
        }
        else {
            movementCurve = descelerationCurve;
            accelerate = false;
        }

        float interpolationTime = Utilities.EvaluateAnimationCurve(movementCurve, ref accelerationPoints, evaluationSpeed, accelerate);
        characterSpeed = Mathf.Lerp(0, movementSpeed, interpolationTime);

        Vector3 inputDirection = Vector3.forward * lastVerticalInput + Vector3.right * lastHorizontalInput;
        inputDirection.Normalize();

        Vector3 additivePosition = characterSpeed * Time.deltaTime * inputDirection;
        transform.position += additivePosition;
    }

    private void Rotation() {
        Vector3 inputDirection = lastVerticalInput == 0 && lastHorizontalInput == 0
            ? Vector3.forward
            : Vector3.forward * lastVerticalInput + Vector3.right * lastHorizontalInput;
        inputDirection.Normalize();
        
        Vector3 characterToMouseDirection = (mouseAnimRig.MousePositionObject.transform.position - transform.position).normalized;

        if (Vector3.Dot(inputDirection, characterToMouseDirection) > 0) {
            Vector3 targetDirection = Vector3.Dot(lookDirection, inputDirection) <= 0f ? characterToMouseDirection : inputDirection;
            lookDirection = Vector3.RotateTowards(lookDirection, targetDirection, Time.deltaTime * rotationSpeed, 1f);
        }
        else {
            Vector3 targetDirection = Vector3.Dot(lookDirection, -inputDirection) <= 0f ? characterToMouseDirection : -inputDirection;
            lookDirection = Vector3.RotateTowards(lookDirection, targetDirection, Time.deltaTime * rotationSpeed, 1f);
        }

        transform.forward = lookDirection;
    }
}
