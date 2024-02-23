using UnityEngine;

public class PlayerController : Controller, AITarget {
    
    private PlayerInput input;
    private CustomCamera customCamera;

    protected override void Awake() {
        base.Awake();
        input = FindObjectOfType<PlayerInput>();
        customCamera = FindObjectOfType<CustomCamera>();
        objectToLookAt = new GameObject();
        objectToLookAt.name = "FollowMouseObject";
    }

    protected override void Start() {
        base.Start();
        input.OnPrimarySwitchPressed += Input_OnPrimarySwitchPressed;
        input.OnSecondarySwitchPressed += Input_OnSecondarySwitchPressed;
        input.OnMeleeSwitchPressed += Input_OnMeleeSwitchPressed;
    }

    private void Input_OnPrimarySwitchPressed(object sender, System.EventArgs e) {
        combatManager.SwitchToPrimary();
    }

    private void Input_OnSecondarySwitchPressed(object sender, System.EventArgs e) {
        combatManager.SwitchToSecondary();
    }

    private void Input_OnMeleeSwitchPressed(object sender, System.EventArgs e) {
        combatManager.SwitchToMelee();
    }

    private void Update() {
        objectToLookAt.transform.position = input.MouseWorldPosition();
        orientation.CharacterRotation(input.MovementInput(), objectToLookAt.transform);
        if (input.FireInputHold()) {
            combatManager.FireEquippedWeapon();
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            combatManager.ReloadEquippedWeapon();
        }
    }

    private void FixedUpdate() {
        Vector3 relativeForward = customCamera.CameraPivot.forward;
        Vector3 relativeRight = customCamera.CameraPivot.right;
        movement.MoveTowardsDirection(input.MovementInput(), input.RunInputHold(), handleCollisions: false, relativeForward, relativeRight);
    }
}
