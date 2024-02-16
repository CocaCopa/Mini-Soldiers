using UnityEngine;

public class PlayerController : MonoBehaviour {

    private PlayerInput input;
    private LookAtObjectAnimRig mouseAnimRig;
    private CharacterMovement movement;
    private CharacterOrientation orientation;
    private CombatManager combatManager;
    private GameObject followMouseObject;


    private void Awake() {
        input = FindObjectOfType<PlayerInput>();
        mouseAnimRig = GetComponentInChildren<LookAtObjectAnimRig>();
        movement = GetComponent<CharacterMovement>();
        orientation = GetComponent<CharacterOrientation>();
        combatManager = GetComponent<CombatManager>();

        followMouseObject = new GameObject("FollowMouseObject");
    }

    private void Start() {
        mouseAnimRig.AssignObjectToLookAt(followMouseObject);
    }

    private void Update() {
        followMouseObject.transform.position = input.MouseWorldPosition();
        movement.MoveTowardsDirection(input.MovementInput(), input.RunKeyContinuous());
        orientation.CharacterRotation(input.MovementInput(), followMouseObject.transform);
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            combatManager.SwitchToPrimary();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            combatManager.SwitchToSecondary();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            combatManager.SwitchToMelee();
        }
    }
}
