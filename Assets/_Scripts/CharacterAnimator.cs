using UnityEngine;

public class CharacterAnimator : MonoBehaviour {

    private const string MOVEMENT_SPEED = "MovementSpeed";
    private const string MOVEMENT_ANIMATION_SPEED = "MovementAnimationSpeed";

    private CharacterMovement movement;
    private CharacterOrientation orientation;
    private Animator animator;

    private void Awake() {
        movement = transform.root.GetComponent<CharacterMovement>();
        orientation = transform.root.GetComponent<CharacterOrientation>();
        animator = GetComponent<Animator>();
    }

    private void Update() {
        float movementSpeed = movement.CurrentSpeed;
        animator.SetFloat(MOVEMENT_SPEED, movementSpeed);

        float playbackSpeed = orientation.MovingBackwards ? -1 : 1;
        animator.SetFloat(MOVEMENT_ANIMATION_SPEED, playbackSpeed);
    }

    /// <summary>
    /// Replaces an animation clip within the runtime animator's controller.
    /// </summary>
    /// <param name="currentClip">The animation clip to be replaced.</param>
    /// <param name="newClip">The animation clip to replace the current one.</param>
    public void SetShootAnimationClip(AnimationClip currentClip, AnimationClip newClip) {
        // Create an AnimatorOverrideController based on the existing runtime controller
        AnimatorOverrideController aoc = new AnimatorOverrideController(animator.runtimeAnimatorController);

        // Replace the animation clip of the "Attack" animation state with the provided clip.
        aoc[currentClip] = newClip;

        // Apply the overrides to the animator
        animator.runtimeAnimatorController = aoc;
    }
}
