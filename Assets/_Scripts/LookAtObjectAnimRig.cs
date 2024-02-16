using UnityEngine;
using UnityEngine.Animations.Rigging;

public class LookAtObjectAnimRig : MonoBehaviour {

    [SerializeField] private RigBuilder rigBuilder;
    [SerializeField] private MultiAimConstraint multiAimConstraint;
    [Space(10)]
    [Tooltip("Speed at which the assigned bone should rotate towards the mouse position.")]
    [SerializeField] private float rotationSpeed = 10f;

    private Rig animationRig;
    private float rigWeightTargetValue;

    private void Awake() {
        animationRig = GetComponent<Rig>();
    }

    private void Update() {
        animationRig.weight = Mathf.Lerp(animationRig.weight, rigWeightTargetValue, rotationSpeed * Time.deltaTime);
    }

    public void AssignObjectToLookAt(GameObject lookAtObject) {
        rigWeightTargetValue = 1;
        var data = multiAimConstraint.data.sourceObjects;
        data.SetTransform(0, lookAtObject.transform);
        multiAimConstraint.data.sourceObjects = data;
        rigBuilder.Build();
    }
}
