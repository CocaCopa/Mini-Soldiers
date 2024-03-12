using UnityEngine;
using CocaCopa.Utilities;

[ExecuteInEditMode]
public class CustomCamera : MonoBehaviour {

    [Tooltip("Reference to the target camera.")]
    [SerializeField] private Camera m_Camera;
    [Tooltip("Target for the camera to follow / look at.")]
    [SerializeField] private Transform followTransform;
    [Tooltip("Reference to the camera pivot object.")]
    [SerializeField] private Transform cameraPivot;

    [Header("--- Follow ---")]
    [Tooltip("Offset the camera's follow position.")]
    [SerializeField] private Vector3 followOffset;
    [Tooltip("Offset the camera's look at position.")]
    [SerializeField] private Vector3 lookAtOffset;
    [Space(10)]
    [Tooltip("Speed at which the camera will follow its target.")]
    [SerializeField] private float followSpeed;
    [Tooltip("If enabled, the camera's look-at position will be stabilized based on the camera's right direction, ensuring that it maintains a consistent orientation relative to the target.")]
    [SerializeField] private bool stabilizeCameraLookAt = true;

    [Header("--- Collisions ---")]
    [SerializeField] private LayerMask collisionLayers;
    [SerializeField] private float cameraRadius;

    private const string followErrorMessage = "Follow Transform reference has not been assigned.";
    private const string pivotErrorMessage = "Camera Pivot reference has not been assigned.";
    private const string cameraErrorMessage = nameof(CustomCamera) + ": Missing Camera reference.";

    public Transform CameraPivot => cameraPivot;
    public Vector3 FollowOffset { get => followOffset; set => followOffset = value; }
    public Vector3 LookAtOffset { get => lookAtOffset; set => lookAtOffset = value; }

    public void SetCameraPivotEulerAngles(Vector3 eulerAngles) {
        cameraPivot.eulerAngles = eulerAngles;
    }

    public void SetCameraFollowTarget(Transform target) {
        followTransform = target;
    }

    private void LateUpdate() {
        if (Common.NullReferenceCheck(m_Camera, cameraErrorMessage)
            || Common.NullReferenceCheck(followTransform, followErrorMessage)
            || Common.NullReferenceCheck(cameraPivot, pivotErrorMessage)) {
            return;
        }

        cameraPivot.position = followTransform.position;
        m_Camera.transform.position = CameraPosition();
        m_Camera.transform.LookAt(CameraLookAt());
        HandleCollisions();
    }

    private Vector3 CameraPosition() {
        Vector3 cameraPosition = cameraPivot.position;
        cameraPosition += cameraPivot.forward * followOffset.z;
        cameraPosition += cameraPivot.right * followOffset.x;
        cameraPosition += cameraPivot.up * followOffset.y;
        return Vector3.Lerp(m_Camera.transform.position, cameraPosition, followSpeed * Time.deltaTime);
    }

    private Vector3 CameraLookAt() {
        Vector3 cameraLookAt = cameraPivot.position;
        cameraLookAt += cameraPivot.forward * lookAtOffset.z;
        if (stabilizeCameraLookAt) {
            StabilizeCameraLookAt(ref cameraLookAt);
        }
        cameraLookAt += cameraPivot.right * lookAtOffset.x;
        cameraLookAt += cameraPivot.up * lookAtOffset.y;
        return cameraLookAt;
    }

    private void StabilizeCameraLookAt(ref Vector3 lookAtPosition) {
        Vector3 directionToCamera = m_Camera.transform.position - followTransform.position;

        Vector3 floorPosition = cameraPivot.position + cameraPivot.right * lookAtOffset.x;
        Vector3 cameraPosition = floorPosition - cameraPivot.forward * followOffset.z;
        Vector3 direction = floorPosition - cameraPosition;
        Vector3 rightDirection = Vector3.Cross(direction.normalized, Vector3.up);

        float distanceInRightDirection = Vector3.Dot(directionToCamera, rightDirection);
        lookAtPosition += cameraPivot.right * distanceInRightDirection;
        lookAtPosition -= cameraPivot.right * followOffset.x;
    }

    private void HandleCollisions() {
        Vector3 origin = followTransform.position;
        Vector3 toCamera = m_Camera.transform.position - origin;
        Vector3 direction = toCamera.normalized;
        Ray ray = new Ray(origin, direction);
        float distance = toCamera.magnitude;

        if (Physics.SphereCast(ray, cameraRadius, out RaycastHit hitInfo, distance, collisionLayers)) {
            print("Camera collision detected.");
        }
    }
}
