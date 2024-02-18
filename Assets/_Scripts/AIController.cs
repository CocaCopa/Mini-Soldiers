using UnityEngine;
using UnityEngine.AI;

public class AIController : Controller {

    private Transform playerTransform;
    private NavMeshAgent agent;

    protected override void Awake() {
        base.Awake();
        playerTransform = FindObjectOfType<PlayerController>().transform;
        objectToLookAt = new GameObject();
        objectToLookAt.name = "AI Look At";
        agent = GetComponent<NavMeshAgent>();
    }

    protected override void Start() {
        base.Start();
    }

    private void Update() {
        objectToLookAt.transform.position = playerTransform.position + Vector3.up * 1.3f;
        agent.SetDestination(-playerTransform.position);
        agent.updateRotation = false;
        Vector3 destination = -playerTransform.position;
        Vector3 inputDirection = destination - transform.position;
        Debug.DrawRay(transform.position, inputDirection * 10f, Color.magenta);
        inputDirection.Normalize();
        orientation.CharacterRotation(new(inputDirection.x, inputDirection.z), objectToLookAt.transform);
    }
}
