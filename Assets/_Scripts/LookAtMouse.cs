using UnityEngine;
using UnityEngine.Animations.Rigging;

public class LookAtMouse : MonoBehaviour {

    [SerializeField] private RigBuilder rigBuilder;

    private MultiAimConstraint multiAimConstraint;
    private Rig animationRig;
    private float rigWeightTargetValue;

    private GameObject mousePositionObject;

    private void Awake() {
        multiAimConstraint = GetComponentInChildren<MultiAimConstraint>();
        animationRig = GetComponent<Rig>();
        mousePositionObject = new GameObject(name = "MousePositionObject");
    }

    private void OnEnable() {
        AssignObjectToLookAt(mousePositionObject);
    }

    private void Update() {
        mousePositionObject.transform.position = MouseWorldPosition();
        animationRig.weight = Mathf.Lerp(animationRig.weight, rigWeightTargetValue, 10f * Time.deltaTime);
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
