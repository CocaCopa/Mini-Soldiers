using UnityEngine;
using CocaCopa.Utilities;

public class CameraController : MonoBehaviour {

    [Tooltip("How fast should the camera move based on the provided input.")]
    [SerializeField] private float sensitivity;
    [SerializeField] private AnimationCurve cameraShoulderCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [SerializeField] private float changeShoulderSpeed;

    private CustomCamera customCamera;
    private Vector2 initialMousePosition;
    private float initialY;
    private float angle;
    private float cameraShoulderPoints;
    private bool incrementShoulderPoints;
    private float leftSideFollow;
    private float rightSideFollow;
    private float leftSideLookAt;
    private float rightSideLookAt;

    private void Awake() {
        customCamera = GetComponent<CustomCamera>();

        rightSideFollow = customCamera.FollowOffset.x;
        leftSideFollow = -rightSideFollow;
        rightSideLookAt = customCamera.LookAtOffset.x;
        leftSideLookAt = -rightSideLookAt;
        incrementShoulderPoints = true;
        cameraShoulderPoints = 1f;
    }

    private void LateUpdate() {
        CameraRotation();
        ChangeCameraShoulder();
    }

    private void CameraRotation() {
        if (Input.GetMouseButtonDown(1)) {
            initialMousePosition = Input.mousePosition;
            initialY = customCamera.CameraPivot.eulerAngles.y;
        }
        if (Input.GetMouseButton(1)) {
            float mouseDrag = (Input.mousePosition.x - initialMousePosition.x) / Screen.width - (Input.mousePosition.y - initialMousePosition.y) / Screen.height;
            angle = mouseDrag * sensitivity;
            Vector3 eulerAngles = customCamera.CameraPivot.eulerAngles;
            eulerAngles.y = initialY + angle;
            customCamera.SetCameraPivotEulerAngles(eulerAngles);
        }
    }

    private void ChangeCameraShoulder() {
        if (Input.GetMouseButtonDown(2)) {
            incrementShoulderPoints = !incrementShoulderPoints;
        }

        float lerpTime = Common.EvaluateAnimationCurve(cameraShoulderCurve, ref cameraShoulderPoints, changeShoulderSpeed, incrementShoulderPoints);
        // Follow Offset.
        float followOffsetX = Mathf.Lerp(leftSideFollow, rightSideFollow, lerpTime);
        Vector3 followOffset = customCamera.FollowOffset;
        followOffset.x = followOffsetX;
        customCamera.FollowOffset = followOffset;

        // Look At Offset.
        float lookAtOffsetX = Mathf.Lerp(leftSideLookAt, rightSideLookAt, lerpTime);
        Vector3 lookAtOffset = customCamera.LookAtOffset;
        lookAtOffset.x = lookAtOffsetX;
        customCamera.LookAtOffset = lookAtOffset;
    }
}
