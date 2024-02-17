using UnityEngine;

public class CharacterAnimator : MonoBehaviour {

    private const string MOVEMENT_SPEED = "MovementSpeed";
    private const string DRAW_RIFLE = "DrawRifle";
    private const string DRAW_PISTOL = "DrawPistol";
    private const string DRAW_KNIFE = "DrawKnife";
    private const string MOVEMENT_ANIMATION_SPEED = "MovementAnimationSpeed";

    private const string FIRE_WEAPON_STATE_NAME = "Attack";

    private CharacterMovement movement;
    private CharacterOrientation orientation;
    private CombatManager combatManager;
    private Animator animator;

    private void Awake() {
        movement = transform.root.GetComponent<CharacterMovement>();
        orientation = transform.root.GetComponent<CharacterOrientation>();
        combatManager = transform.root.GetComponent<CombatManager>();
        animator = GetComponent<Animator>();
    }

    private void Start() {
        combatManager.OnSwitchWeapons += CombatManager_OnSwitchWeapons;
    }

    private void Update() {
        float movementSpeed = movement.CurrentSpeed;
        animator.SetFloat(MOVEMENT_SPEED, movementSpeed);

        float playbackSpeed = orientation.MovingBackwards ? -1 : 1;
        animator.SetFloat(MOVEMENT_ANIMATION_SPEED, playbackSpeed);
    }

    public void PlayWeaponFireAnimation() {
        animator.Play(FIRE_WEAPON_STATE_NAME, 1, 0f);
    }

    private void CombatManager_OnSwitchWeapons(object sender, CombatManager.OnSwitchWeaponsEventArgs e) {
        string drawTrigger = "";
        switch (e.weaponType) {
            case WeaponType.Knife:
            drawTrigger = DRAW_KNIFE;
            break;
            case WeaponType.Pistol:
            drawTrigger = DRAW_PISTOL;
            break;
            case WeaponType.Rifle:
            drawTrigger = DRAW_RIFLE;
            break;
        }
        animator.SetTrigger(drawTrigger);
    }

    /// <summary>
    /// Replaces an animation clip within the runtime animator's controller.
    /// </summary>
    /// <param name="currentClip">The animation clip to be replaced.</param>
    /// <param name="newClip">The animation clip to replace the current one.</param>
    public void SetShootAnimationClip(AnimationClip currentClip, AnimationClip newClip) {
        AnimatorOverrideController overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        overrideController[currentClip] = newClip;
        animator.runtimeAnimatorController = overrideController;
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
                if (animator.GetCurrentAnimatorStateInfo(animatorLayer).normalizedTime <= percentage)
                    return true;
            }
        }
        return false;
    }
}
