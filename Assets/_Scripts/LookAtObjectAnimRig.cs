using UnityEngine;
using UnityEngine.Animations.Rigging;

public class LookAtObjectAnimRig : MonoBehaviour {

    private RigBuilder rigBuilder;
    private MultiAimConstraint[] multiAimConstraint;

    private void Awake() {
        rigBuilder = transform.root.GetComponentInChildren<RigBuilder>();
        multiAimConstraint = GetComponentsInChildren<MultiAimConstraint>();
    }

    public void AssignObjectToLookAt(GameObject lookAtObject) {
        foreach (var constraint in multiAimConstraint) {
            var data = constraint.data.sourceObjects;
            data.SetTransform(0, lookAtObject.transform);
            constraint.data.sourceObjects = data;
            rigBuilder.Build();
        }
    }

    private void LateUpdate() {
        GameObject hand = GameObject.Find("Bip001 R Hand");
        hand.transform.Rotate(20 * Time.deltaTime, 0, 0);
    }
}
