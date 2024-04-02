using UnityEngine;

public class PlayerController : Controller {

    [Tooltip("The maximum angle in degrees that the character can look upward.")]
    [SerializeField] private float maxLookUpAngle = 45f;

    private LineRenderer laserSight;
    private PlayerInput input;
    private CustomCamera customCamera;
    private Vector3 trackDirection;
    private float height;

    public Vector3 MoveDirection => (customCamera.CameraPivot.forward * DirectionalInput.y + customCamera.CameraPivot.right * DirectionalInput.x).normalized;

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
        combatManager.SwitchWeapon(WeaponSwitch.Primary);
    }

    private void Input_OnSecondarySwitchPressed(object sender, System.EventArgs e) {
        combatManager.SwitchWeapon(WeaponSwitch.Secondary);
    }

    private void Input_OnMeleeSwitchPressed(object sender, System.EventArgs e) {
        combatManager.SwitchWeapon(WeaponSwitch.Melee);
    }

    protected override void Update() {
        base.Update();
        
        SetLookAtObjectPosition(ClampLookAtObjectHeight());
        HandleMovementParameters();
        HandleCombatInput();
        ManageLaserSight();
    }

    private void HandleMovementParameters() {
        DirectionalInput = input.MovementInput();
        RelativeForwardDir = customCamera.CameraPivot.forward;
        RelativeRightDir = customCamera.CameraPivot.right;
        IsRunning = input.RunInputHold();
    }

    private void HandleCombatInput() {
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

    private Vector3 ClampLookAtObjectHeight() {
        Vector3 characterToObjectDirection = input.MouseWorldPosition() - transform.position;
        characterToObjectDirection.Normalize();
        Vector3 lookDirection = new Vector3(input.MouseWorldPosition().x, transform.position.y, input.MouseWorldPosition().z) - transform.position;
        lookDirection.Normalize();

        if (Vector3.Angle(characterToObjectDirection, lookDirection) < maxLookUpAngle) {
            trackDirection = characterToObjectDirection;
            height = input.MouseWorldPosition().y;
        }
        else {
            Vector3 origin = transform.position;
            Vector3 direction = trackDirection;
            Ray ray = new Ray(origin, direction);
            float distance = Mathf.Infinity;
            if (Physics.Raycast(ray, out RaycastHit hitInfo, distance, ~LayerMask.GetMask("Player"))) {
                height = hitInfo.point.y;
                height = Mathf.Clamp(height, 1f, hitInfo.point.y);
            }
            else {
                Debug.Log("Warning! Investigate this code block.");
            }
        }
        Vector3 objectPosition = input.MouseWorldPosition();
        objectPosition.y = height;

        return objectPosition;
    }
}
