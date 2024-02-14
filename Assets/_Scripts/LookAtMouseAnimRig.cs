using UnityEngine;
using UnityEngine.Animations.Rigging;

public class LookAtMouseAnimRig : MonoBehaviour {

    [SerializeField] private RigBuilder rigBuilder;
    [SerializeField] private MultiAimConstraint multiAimConstraint;
    [Space(10)]
    [Tooltip("Speed at which the assigned bone should rotate towards the mouse position.")]
    [SerializeField] private float rotationSpeed = 10f;

    private Rig animationRig;
    private float rigWeightTargetValue;

    private GameObject mousePositionObject;

    public GameObject MousePositionObject => mousePositionObject;

    private void Awake() {
        animationRig = GetComponent<Rig>();
        mousePositionObject = new GameObject(name = "MousePositionObject");
    }

    private void OnEnable() {
        AssignObjectToLookAt(mousePositionObject);
    }

    private void Update() {
        mousePositionObject.transform.position = MouseWorldPosition();
        animationRig.weight = Mathf.Lerp(animationRig.weight, rigWeightTargetValue, rotationSpeed * Time.deltaTime);
    }

    private void AssignObjectToLookAt(GameObject lookAtObject) {
        rigWeightTargetValue = 1;
        var data = multiAimConstraint.data.sourceObjects;
        data.SetTransform(0, lookAtObject.transform);
        multiAimConstraint.data.sourceObjects = data;
        rigBuilder.Build();
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
