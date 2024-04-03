using UnityEngine;
using UnityEngine.Animations.Rigging;
using CocaCopa.Utilities;

public class HandAimConstraint : MonoBehaviour {

    private MultiAimConstraint handAimConstraint;
    private CombatManager combatManager;
    private AnimationCurve constraintCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    private float constraintAnimationPoints;
    public float handAimSpeed;
    public bool aimTowardsMouse;

    /// <summary>
    /// Smoothly aim the character's hand towards the mouse when switching weapons at a default speed.
    /// </summary>
    private const float DEFAULT_AIM_SPEED = 1.49f;
    /// <summary>
    /// Smoothly aim the character's hand towards the mouse when switching weapons at a faster speed.
    /// </summary>
    private const float FAST_AIM_SPEED = DEFAULT_AIM_SPEED * 4f;

    private void Awake() {
        handAimConstraint = GetComponent<MultiAimConstraint>();
        combatManager = transform.root.GetComponent<CombatManager>();
        combatManager.OnSwitchWeapons += CombatManager_OnSwitchWeapons;
    }

    private void CombatManager_OnSwitchWeapons(object sender, CombatManager.OnSwitchWeaponsEventArgs e) {
        constraintAnimationPoints = 0f;
        handAimConstraint.weight = 0f;
    }

    private void Update() {
        CalculateHandAimBehaviour();
        ManageHandConstraint();
    }

    private void CalculateHandAimBehaviour() {
        bool isReloading = combatManager.IsReloading;
        bool isSwitchingWeapon = combatManager.IsSwitchingWeapon;
        bool reloadCompleted = !combatManager.IsReloading && handAimConstraint.weight == 0f && combatManager.IsCombatIdle;
        bool idleTransition = combatManager.IsCombatIdle == false && handAimConstraint.weight != 0f;
        bool isShooting = combatManager.IsPullingGunTrigger;

        if (isReloading || isSwitchingWeapon) {
            handAimSpeed = DEFAULT_AIM_SPEED;
        }
        if (reloadCompleted || idleTransition) {
            handAimSpeed = FAST_AIM_SPEED;
        }

        if (isReloading || idleTransition) {
            aimTowardsMouse = false;
        }
        else if (isShooting || reloadCompleted || isSwitchingWeapon) {
            aimTowardsMouse = true;
        }
    }

    private void ManageHandConstraint() {
        if (!combatManager.IsSwitchingWeapon && combatManager.IsPullingGunTrigger) {
            constraintAnimationPoints = 1f;
            handAimConstraint.weight = 1f;
        }
        else {
            float lerpTime = Common.EvaluateAnimationCurve(constraintCurve, ref constraintAnimationPoints, handAimSpeed, aimTowardsMouse);
            handAimConstraint.weight = Mathf.Lerp(0f, 1f, lerpTime);
            
        }
    }
}
