using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour {

    private const string MOVEMENT_SPEED = "MovementSpeed";
    private const string MOVEMENT_ANIMATION_SPEED = "MovementAnimationSpeed";

    private PlayerController playerController;
    private Animator animator;

    private void Awake() {
        playerController = transform.root.GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
    }

    private void Update() {
        float movementSpeed = playerController.CharacterSpeed;
        animator.SetFloat(MOVEMENT_SPEED, movementSpeed);

        float playbackSpeed = playerController.MovingBackwards ? -1 : 1;
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
