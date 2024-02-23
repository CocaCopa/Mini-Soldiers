using UnityEngine;

public class CustomCamera : MonoBehaviour {

    [SerializeField] private Transform followTransform;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float sensitivity;

    public float angle;
    private Vector3 initialMousePosition;
    private Vector3 initialEuler;
    private float initialY;

    public Transform CameraPivot => cameraPivot;

    public void SetCameraFollowTarget(Transform target) {

    }

    private void Update() {
        cameraPivot.position = followTransform.position;

        if (Input.GetMouseButtonDown(1)) {
            initialMousePosition = Input.mousePosition;
            initialEuler = cameraPivot.eulerAngles;
            initialY = cameraPivot.eulerAngles.y;
        }
        if (Input.GetMouseButton(1)) {
            float value = (Input.mousePosition.x - initialMousePosition.x) / Screen.width;
            //initialEuler.y = initialY + value * sensitivity;
            angle = value * sensitivity;
            Vector3 euler = cameraPivot.eulerAngles;
            euler.y = initialY + angle;
            cameraPivot.eulerAngles = euler;
        }



    }
}
