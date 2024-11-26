using UnityEngine;

public class AIStimulus : MonoBehaviour {

    [Header("--- Sight Sense ---")]
    [Tooltip("Specify the Transform of the eyes. \n\nIf the AI's head rotates independently from its body, provide the Transform of the head or the eyes for accurate calculations.\n\nIf the head and body rotate together, the eyes can be the same GameObject as the AI itself.")]
    [SerializeField] private Transform eyesTransform;
    [Tooltip("Distance at which the AI is able to detect a target.")]
    [SerializeField] float sightRadius = 5.0f;
    [Tooltip("Angle at which the AI can spot a target.")]
    [SerializeField,Range(0f, 360f)] float fieldOfView = 90.0f;

    public Transform EyesTransform => eyesTransform;
    public float SightRadius { get => sightRadius; set => sightRadius = value; }
    public float FieldOfView { get => fieldOfView; set => fieldOfView = value; }

    private float eyesHeightOffset;

    private void Awake() {
        float eyesWorldPositionY = eyesTransform.position.y;
        float myTransformY = transform.position.y;
        eyesHeightOffset = eyesWorldPositionY - myTransformY;
    }

    /// <summary>
    /// Determines whether the AI has a clear line of sight to the specified target.
    /// Assumes that both the AI and the target object are humanoids and their pivot point is located at their feet.
    /// </summary>
    /// <param name="targetTransform">The transform of the target.</param>
    /// <returns>True if the AI has a clear line of sight to the target; otherwise, false.</returns>
    public bool CanSeeTarget(Transform targetTransform) {
        Vector3 myPosition = transform.position;
        Vector3 targetPosition = targetTransform.position;
        Vector3 toTargetVector = targetPosition - myPosition;
        if (toTargetVector.magnitude <= sightRadius) {
            float dotProduct = Vector3.Dot(eyesTransform.forward, toTargetVector.normalized);
            float FOV = Mathf.Cos(fieldOfView * 0.5f * Mathf.Deg2Rad);
            if (dotProduct > FOV) {
                return ClearLineOfSight(targetTransform);
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if there is a clear line of sight from the current transform's position to the specified target's transform position.
    /// </summary>
    /// <param name="targetTransform">The transform of the target to check for line of sight.</param>
    /// <returns>True if there is a clear line of sight to the target, otherwise false.</returns>
    public bool ClearLineOfSight(Transform targetTransform) {
        Vector3 origin = transform.position;
        // The character animations affect the position/height of the eyes. Changes to the Y positions help stabilize the height of the direction.
        origin.y += eyesHeightOffset;
        Vector3 targetPosition = targetTransform.position;
        targetPosition.y += eyesHeightOffset;
        Vector3 direction = (targetPosition - origin).normalized;
        if (Physics.Raycast(origin, direction, out RaycastHit hitInfo, sightRadius)) {
            return hitInfo.transform.gameObject == targetTransform.gameObject;
        }
        return false;
    }
}
