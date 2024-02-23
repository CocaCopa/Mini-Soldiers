using UnityEngine;

public class CustomCamera : MonoBehaviour {

    [SerializeField] private Transform followTransform;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float sensitivity;

    private float angle;
    private Vector2 initialMousePosition;
    private float initialY;

    public Transform CameraPivot => cameraPivot;

    public void SetCameraFollowTarget(Transform target) {

    }

    private void Update() {
        cameraPivot.position = followTransform.position;

        if (Input.GetMouseButtonDown(1)) {
            initialMousePosition = Input.mousePosition;
            initialY = cameraPivot.eulerAngles.y;
        }
        if (Input.GetMouseButton(1)) {
            float mouseDrag = (Input.mousePosition.x - initialMousePosition.x) / Screen.width - (Input.mousePosition.y - initialMousePosition.y) / Screen.height;
            angle = mouseDrag * sensitivity;
            Vector3 euler = cameraPivot.eulerAngles;
            euler.y = initialY + angle;
            cameraPivot.eulerAngles = euler;
        }
    }
}
