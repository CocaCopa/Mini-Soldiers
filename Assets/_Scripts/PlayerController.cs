using UnityEngine;
using CocaCopa;
using TMPro;
using System.Xml.Serialization;

public class PlayerController : MonoBehaviour {

    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private AnimationCurve accelerationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private AnimationCurve descelerationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private float evaluateAcceleration;
    [SerializeField] private float evaluateDesceleration;
    [SerializeField] private float rotationSpeed;
    [Space(10)]
    [SerializeField] private WeaponSO weapon;

    public float CharacterSpeed => characterSpeed;
    public bool MovingBackwards => movingBackwards;

    private PlayerInput input;
    private AnimationCurve movementCurve;
    private LookAtObjectAnimRig mouseAnimRig;
    private GameObject followMouseObject;
    private Vector3 lastDirectionInput;
    private float moveCurveEvaluationSpeed;
    private float accelerationPoints;
    private float characterSpeed;
    private float minMoveSpeed;
    private float maxMoveSpeed;
    private bool accelerate;
    private bool movingBackwards;
    private Vector3 lookDirection;

    private bool flag;

    private void Awake() {
        input = FindObjectOfType<PlayerInput>();
        mouseAnimRig = GetComponentInChildren<LookAtObjectAnimRig>();
        followMouseObject = new GameObject("FollowMouseObject");
        movementCurve = accelerationCurve;
    }

    private void Start() {
        mouseAnimRig.AssignObjectToLookAt(followMouseObject);
    }

    private void Update() {
        followMouseObject.transform.position = input.MouseWorldPosition();
        MoveTowardsDirection(input.MovementInput());
        CharacterRotation();
        Shoot();
    }

    private void MoveTowardsDirection(Vector2 direction) {
        if (direction != Vector2.zero) {
            lastDirectionInput = direction;
            if (Input.GetKeyDown(KeyCode.LeftShift)) {
                accelerationPoints = 0f;
                minMoveSpeed = characterSpeed;
                maxMoveSpeed = runSpeed;
                movementCurve = accelerationCurve;
                moveCurveEvaluationSpeed = evaluateAcceleration;
                accelerate = true;
            }
            else if (Input.GetKeyUp(KeyCode.LeftShift)) {
                minMoveSpeed = walkSpeed;
                maxMoveSpeed = characterSpeed;
                movementCurve = descelerationCurve;
                moveCurveEvaluationSpeed = evaluateDesceleration;
                accelerate = false;
            }
            if (!Input.GetKey(KeyCode.LeftShift)) {
                if (characterSpeed < walkSpeed) {
                    minMoveSpeed = 0f;
                    maxMoveSpeed = walkSpeed;
                    movementCurve = accelerationCurve;
                    moveCurveEvaluationSpeed = evaluateAcceleration;
                    accelerate = true;
                }
            }
            flag = true;
        }
        else if (flag) {
            if (characterSpeed > 0f && accelerationPoints == 0f) {
                accelerationPoints = 1f;
            }
            flag = false;
            minMoveSpeed = 0f;
            maxMoveSpeed = characterSpeed;
            movementCurve = descelerationCurve;
            moveCurveEvaluationSpeed = evaluateDesceleration;
            accelerate = false;
        }

        float interpolationTime = Utilities.EvaluateAnimationCurve(movementCurve, ref accelerationPoints, moveCurveEvaluationSpeed, accelerate);
        characterSpeed = Mathf.Lerp(minMoveSpeed, maxMoveSpeed, interpolationTime);

        Vector3 inputDirection = Vector3.forward * lastDirectionInput.y + Vector3.right * lastDirectionInput.x;
        inputDirection.Normalize();

        Vector3 additivePosition = characterSpeed * Time.deltaTime * inputDirection;
        transform.position += additivePosition;
    }

    private void CharacterRotation() {
        Vector3 inputDirection = lastDirectionInput.y == 0 && lastDirectionInput.x == 0
            ? Vector3.forward
            : Vector3.forward * lastDirectionInput.y + Vector3.right * lastDirectionInput.x;
        inputDirection.Normalize();
        
        Vector3 characterToMouseDirection = (followMouseObject.transform.position - transform.position).normalized;

        if (Vector3.Dot(inputDirection, characterToMouseDirection) > 0) {
            Vector3 targetDirection = Vector3.Dot(lookDirection, inputDirection) <= 0f ? characterToMouseDirection : inputDirection;
            lookDirection = Vector3.RotateTowards(lookDirection, targetDirection, Time.deltaTime * rotationSpeed, 1f);
            movingBackwards = false;
        }
        else {
            Vector3 targetDirection = Vector3.Dot(lookDirection, -inputDirection) <= 0f ? characterToMouseDirection : -inputDirection;
            lookDirection = Vector3.RotateTowards(lookDirection, targetDirection, Time.deltaTime * rotationSpeed, 1f);
            movingBackwards = true;
        }

        transform.forward = lookDirection;

        Vector3 eulerAngles = transform.eulerAngles;
        eulerAngles.x = eulerAngles.z = 0;
        transform.eulerAngles = eulerAngles;
    }

    public AnimationClip defaultClip;
    public AnimationClip shootclip;
    private void Shoot() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            CharacterAnimator anim = GetComponentInChildren<CharacterAnimator>();
            anim.SetShootAnimationClip(defaultClip, shootclip);
        }

        if (Input.GetMouseButtonDown(0)) {
            Animator anim = GetComponentInChildren<Animator>();
            anim.SetBool("New Bool", true);
        }
        else if (Input.GetMouseButtonUp(0)) {
            Animator anim = GetComponentInChildren<Animator>();
            anim.SetBool("New Bool", false);
        }
    }
}
