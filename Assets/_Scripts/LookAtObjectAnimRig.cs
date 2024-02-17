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

    private void Test() {
        Controller controller = transform.root.GetComponent<Controller>();
        Vector3 mousePosition = controller.ObjectToLookAt.transform.position;
        Vector3 origin = transform.position;
        Vector3 characterToMouse = (mousePosition - transform.position).normalized;
        Debug.DrawRay(origin, characterToMouse * 10f, Color.magenta);

        if (Vector3.Dot(characterToMouse, transform.forward) < 0f) {
            Debug.DrawRay(transform.position, -Vector3.Reflect(characterToMouse, transform.right) * 10f, Color.green);
        }
    }
}
