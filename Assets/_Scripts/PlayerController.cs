using UnityEngine;

public class PlayerController : Controller, AITarget {

    private LineRenderer laserSight;
    private PlayerInput input;
    private CustomCamera customCamera;

    protected override void Awake() {
        base.Awake();
        input = FindObjectOfType<PlayerInput>();
        customCamera = FindObjectOfType<CustomCamera>();
        laserSight = GetComponentInChildren<LineRenderer>(true);
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

    protected override void Update() {
        base.Update();
        SetLookAtObjectPosition(input.MouseWorldPosition());
        ManageLaserSight();
        DirectionalInput = input.MovementInput();
        RelativeForwardDir = customCamera.CameraPivot.forward;
        RelativeRightDir = customCamera.CameraPivot.right;
        IsRunning = input.RunInputHold();

        if (input.FireInputHold()) {
            combatManager.PullGunTrigger();
        }
        else if (input.FireInputReleased()) {
            combatManager.ReleaseGunTrigger();
        }

        if (input.ReloadInputPerformed()) {
            combatManager.ReloadEquippedWeapon();
        }
    }

    private void ManageLaserSight() {
        laserSight.gameObject.SetActive(combatManager.IsCombatIdle && !combatManager.IsSwitchingWeapon);
        Vector3 laserOrigin = combatManager.RightHandTransform.position;
        Vector3 aimPosition = ObjectToLookAt.transform.position;
        Vector3 dirToAimPosition = (aimPosition - laserOrigin).normalized;
        laserSight.transform.SetPositionAndRotation(laserOrigin, Quaternion.LookRotation(dirToAimPosition));
        CalculateLaserLength();

        void CalculateLaserLength() {
            bool objectFound = Physics.Raycast(laserOrigin, dirToAimPosition, out RaycastHit hit, float.MaxValue);
            Vector3 endPosition = objectFound ? hit.point : aimPosition;
            Vector3 laserLength = laserSight.GetPosition(1);
            laserLength.z = (endPosition - laserOrigin).magnitude;
            laserSight.SetPosition(1, laserLength);
        }
    }
}
