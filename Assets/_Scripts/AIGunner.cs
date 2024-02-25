using UnityEngine;

public class AIGunner : AIController {

    private Transform targetTransform;
    private Vector3 targetMovePosition;

    protected override void Awake() {
        base.Awake();
        targetTransform = FindObjectOfType<PlayerController>().transform;
    }

    protected override void Update() {
        base.Update();
        SetLookAtObjectPosition(targetTransform.position + Vector3.up * 1.2f);
        if (Stimulus.CanSeeTarget(targetTransform)) {
            combatManager.ReleaseGunTrigger();
            combatManager.PullGunTrigger();
            if (combatManager.EquippedWeapon.RemainingBullets == 0) {
                combatManager.SwitchToSecondary();
            }

            targetMovePosition = FindHideSpot(targetTransform);
            SetNewDestination(targetMovePosition);
        }
    }

    private void FixedUpdate() {
        movement.MoveTowardsDirection(InputDirection, run: false);
    }
}
