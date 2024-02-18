using UnityEngine;
using UnityEngine.Animations.Rigging;

public class LookAtObjectAnimRig : MonoBehaviour {

    private RigBuilder rigBuilder;
    private MultiAimConstraint multiAimConstraint;

    private void Awake() {
        rigBuilder = transform.root.GetComponentInChildren<RigBuilder>();
        multiAimConstraint = GetComponentInChildren<MultiAimConstraint>();
    }

    public void AssignObjectToLookAt(GameObject lookAtObject) {
        var data = multiAimConstraint.data.sourceObjects;
        data.SetTransform(0, lookAtObject.transform);
        multiAimConstraint.data.sourceObjects = data;
        rigBuilder.Build();
    }
}
