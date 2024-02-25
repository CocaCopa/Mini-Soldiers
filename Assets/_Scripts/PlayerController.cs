using UnityEngine;

public class PlayerController : Controller, AITarget {
    
    private PlayerInput input;
    private CustomCamera customCamera;

    protected override void Awake() {
        base.Awake();
        input = FindObjectOfType<PlayerInput>();
        customCamera = FindObjectOfType<CustomCamera>();
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
        SetLookAtObjectPosition(input.MouseWorldPosition());
        Vector3 relativeForward = customCamera.CameraPivot.forward;
        Vector3 relativeRight = customCamera.CameraPivot.right;
        orientation.CharacterRotation(input.MovementInput(), ObjectToLookAt.transform, relativeForward, relativeRight);

        if (input.FireInputHold()) {
            combatManager.PullGunTrigger();
        }
        else if (input.FireInputReleased()) {
            combatManager.ReleaseGunTrigger();
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
