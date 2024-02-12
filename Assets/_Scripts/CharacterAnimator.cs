using UnityEngine;

public class CharacterAnimator : MonoBehaviour {

    private const string MOVEMENT_SPEED = "MovementSpeed";

    private PlayerController playerController;
    private Animator animator;

    private void Awake() {
        playerController = transform.root.GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
    }

    private void Update() {
        float movementSpeed = playerController.MovementSpeed;
        animator.SetFloat(MOVEMENT_SPEED, movementSpeed);
    }
}
