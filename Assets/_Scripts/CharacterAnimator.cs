using UnityEngine;

public class CharacterAnimator : MonoBehaviour {

    private const string MOVEMENT_ANIMATION_SPEED = "MovementAnimationSpeed";
    private const string MOVEMENT_SPEED = "MovementSpeed";
    private const string DRAW_RIFLE = "DrawRifle";
    private const string DRAW_PISTOL = "DrawPistol";
    private const string DRAW_KNIFE = "DrawKnife";
    private const string RELOAD = "Reload";
    private const string PRIMARY_IDLE = "PrimaryIdle";
    private const string SECONDARY_IDLE = "SecondaryIdle";
    private const string MELEE_IDLE = "MeleeIdle";
    private const string IS_COMBAT_IDLE = "IsCombatIdle";
    private const string IS_SWITCHING_WEAPON = "IsSwitchingWeapon";

    private const string STATE_NAME_DRAW_WEAPON = "m_weapon_draw";
    private const string STATE_NAME_DRAW_PISTOL = "m_pistol_draw";

    private Controller controller;
    private CharacterOrientation orientation;
    private CombatManager combatManager;
    private Animator animator;

    private void Awake() {
        controller = transform.root.GetComponent<Controller>();
        orientation = transform.root.GetComponent<CharacterOrientation>();
        combatManager = transform.root.GetComponent<CombatManager>();
        animator = GetComponent<Animator>();
        combatManager.OnSwitchWeapons += CombatManager_OnSwitchWeapons;
        combatManager.OnInitiateWeaponReload += CombatManager_OnInitiateWeaponReload;
    }

    private void Start() {
    }

    private void CombatManager_OnInitiateWeaponReload(object sender, System.EventArgs e) {
        animator.SetTrigger(RELOAD);
    }

    private void Update() {
        float playbackSpeed = orientation.MovingBackwards ? -1 : 1;
        float movementSpeed = controller.CurrentMovementSpeed;
        bool isCombatIdle = combatManager.IsCombatIdle;
        bool isSwitchingWeapon = combatManager.IsSwitchingWeapon;

        animator.SetFloat(MOVEMENT_ANIMATION_SPEED, playbackSpeed);
        animator.SetFloat(MOVEMENT_SPEED, movementSpeed);
        animator.SetBool(IS_COMBAT_IDLE, isCombatIdle);
        animator.SetBool(IS_SWITCHING_WEAPON, isSwitchingWeapon);
    }

    private void CombatManager_OnSwitchWeapons(object sender, CombatManager.OnSwitchWeaponsEventArgs e) {
        switch (e.weaponType) {
            case WeaponType.Primary:
            animator.Play(STATE_NAME_DRAW_WEAPON, 1, 0f);
            animator.SetBool(PRIMARY_IDLE, true);
            animator.SetBool(SECONDARY_IDLE, false);
            animator.SetBool(MELEE_IDLE, false);
            break;
            case WeaponType.Secondary:
            animator.Play(STATE_NAME_DRAW_PISTOL, 1, 0f);
            animator.SetBool(PRIMARY_IDLE, false);
            animator.SetBool(SECONDARY_IDLE, true);
            animator.SetBool(MELEE_IDLE, false);
            break;
            case WeaponType.Melee:
            animator.SetBool(PRIMARY_IDLE, false);
            animator.SetBool(SECONDARY_IDLE, false);
            animator.SetBool(MELEE_IDLE, true);
            break;
        }
    }

    /// <summary>
    /// Determines if the specified animation clip is currently playing within a given percentage of its duration.
    /// </summary>
    /// <param name="clip">The animation clip to check.</param>
    /// <param name="animatorLayer">The desired percentage of the clip's duration.</param>
    /// <param name="percentage">The desired percentage of the clip's duration.</param>
    /// <returns>True, if the playhead is within the specified percentage of the animation clip; otherwise, false.</returns>
    public bool IsClipPlaying(AnimationClip clip, int animatorLayer, float percentage) {
        AnimatorClipInfo[] currentClipInfo = animator.GetCurrentAnimatorClipInfo(animatorLayer);
        foreach (var clipInfo in currentClipInfo) {
            if (clipInfo.clip == clip) {
                if (animator.GetCurrentAnimatorStateInfo(animatorLayer).normalizedTime <= percentage) {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if the Animator component is transitioning to a specific animation clip on a given layer.
    /// </summary>
    /// <param name="clip">The animation clip to check for transition.</param>
    /// <param name="animatorLayer">The layer index of the Animator to check.</param>
    /// <returns>True if the Animator is transitioning to the specified clip on the specified layer, otherwise false.</returns>
    public bool IsTransitioningToClip(AnimationClip clip, int animatorLayer) {
        AnimatorClipInfo[] nextClipInfo = animator.GetNextAnimatorClipInfo(animatorLayer);
        foreach(var clipInfo in nextClipInfo) {
            if (clipInfo.clip == clip) {
                return true;
            }
        }
        return false;
    }
}
