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
    public float SightRadius => sightRadius;
    public float FieldOfView => fieldOfView;

    private float eyesPositionY;

    private void Awake() {
        eyesPositionY = eyesTransform.position.y;
    }

    /// <summary>
    /// Determines whether the AI has a clear line of sight to the specified target.
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
                return ClearLineOfSight(targetTransform.position);
            }
        }
        return false;
    }

    private bool ClearLineOfSight(Vector3 targetPosition) {
        Vector3 origin = transform.position;
        // The character animations affect the position/height of the eyes. Changes to the Y positions help stabilize the height of the direction.
        origin.y += eyesPositionY;
        targetPosition.y += eyesPositionY;
        Vector3 direction = (targetPosition - origin).normalized;
        float distance = Vector3.Distance(origin, targetPosition);

        Debug.DrawRay(origin, direction * distance, Color.magenta);
        return true;
    }
}
