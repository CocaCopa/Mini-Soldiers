using UnityEngine;

public class PlayerController : MonoBehaviour {

    [SerializeField] private WeaponSO weapon;
    [SerializeField] private AnimationClip defaultClip;
    [SerializeField] private AnimationClip shootclip;

    private PlayerInput input;
    private LookAtObjectAnimRig mouseAnimRig;
    private CharacterMovement movement;
    private CharacterOrientation orientation;
    private GameObject followMouseObject;


    private void Awake() {
        input = FindObjectOfType<PlayerInput>();
        mouseAnimRig = GetComponentInChildren<LookAtObjectAnimRig>();
        movement = GetComponent<CharacterMovement>();
        orientation = GetComponent<CharacterOrientation>();

        followMouseObject = new GameObject("FollowMouseObject");
    }

    private void Start() {
        mouseAnimRig.AssignObjectToLookAt(followMouseObject);
    }

    private void Update() {
        followMouseObject.transform.position = input.MouseWorldPosition();
        movement.MoveTowardsDirection(input.MovementInput(), input.RunKeyContinuous());
        orientation.CharacterRotation(input.MovementInput(), followMouseObject.transform);
        Shoot();
    }

    private void Shoot() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            CharacterAnimator anim = GetComponentInChildren<CharacterAnimator>();
            anim.SetShootAnimationClip(defaultClip, shootclip);
        }

        if (Input.GetMouseButtonDown(0)) {
            Animator anim = GetComponentInChildren<Animator>();
            anim.SetBool("New Bool", true);
        }
        else if (Input.GetMouseButtonUp(0)) {
            Animator anim = GetComponentInChildren<Animator>();
            anim.SetBool("New Bool", false);
        }
    }
}
