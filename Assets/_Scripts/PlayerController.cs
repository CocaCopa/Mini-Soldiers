using UnityEngine;
using CocaCopa;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {

    [SerializeField] private Transform spineTransform;
    [SerializeField] private float movementSpeed;
    [SerializeField] private AnimationCurve accelerationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private AnimationCurve descelerationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private float evaluationSpeed;

    public float MovementSpeed => characterSpeed;

    private AnimationCurve movementCurve;
    private float horizontalInput;
    private float verticalInput;
    private float lastHorizontalInput;
    private float lastVerticalInput;
    private float accelerationPoints;
    private float characterSpeed;
    private bool accelerate;

    private void Update() {
        Movement();
        Spine();
    }

    private void Movement() {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

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

        Vector3 moveDirection = Vector3.forward * lastVerticalInput + Vector3.right * lastHorizontalInput;
        moveDirection.Normalize();

        Vector3 additivePosition = characterSpeed * Time.deltaTime * moveDirection;
        transform.position += additivePosition;
    }

    public GameObject temp;
    public GameObject temp_2;
    private void Spine() {
        Vector3 mousePosition = MouseWorldPosition();
        Vector3 playerToMouseDir = (mousePosition - transform.position).normalized;
        spineTransform.forward = playerToMouseDir;
    }

    private Vector3 MouseWorldPosition() {
        Vector3 mouseScreenPosition = Input.mousePosition;
        Ray mouseRay = Camera.main.ScreenPointToRay(mouseScreenPosition);

        if (Physics.Raycast(mouseRay, out RaycastHit hit)) {
            Vector3 mouseHitPoint = hit.point;
            mouseHitPoint.y = transform.position.y;
            return mouseHitPoint;
        }
        return Vector3.zero;
    }
}
