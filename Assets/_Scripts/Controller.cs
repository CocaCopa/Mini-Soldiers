using System;
using UnityEngine;
using UnityEngine.Windows;

public abstract class Controller : MonoBehaviour, IDamageable {

    public event EventHandler OnCharacterTakeDamage;
    public event EventHandler OnCharacterRespawn;
    public event EventHandler OnCharacterDeath;

    private enum OnGameStart { SwitchToPrimary, SwitchToSecondary, SwitchToMelee }

    [Tooltip("When the game starts, the selected weapon will be automatically equipped.")]
    [SerializeField] private OnGameStart onGameStart;
    [Tooltip("How fast should the character turn their head to look at the 'ObjectToLookAt' target.")]
    [SerializeField] private float lookAtTargetObjectSpeed = 10f;

    private LookAtObjectAnimRig objectAnimRig;
    private GameObject objectToLookAt;
    protected CharacterMovement movement;
    protected CharacterOrientation orientation;
    protected CombatManager combatManager;

    protected Vector2 DirectionalInput { get; set; }
    protected bool IsRunning { get; set; }
    protected Vector3 RelativeForwardDir { get; set; }
    protected Vector3 RelativeRightDir { get; set; }

    public GameObject ObjectToLookAt => objectToLookAt;
    public float CurrentMovementSpeed => movement.CurrentSpeed;

    protected virtual void Awake() {
        objectAnimRig = GetComponentInChildren<LookAtObjectAnimRig>();
        movement = GetComponent<CharacterMovement>();
        orientation = GetComponent<CharacterOrientation>();
        combatManager = GetComponent<CombatManager>();
    }

    protected virtual void Start() {
        objectToLookAt = new GameObject(gameObject.name + " LookAtObject");
        objectAnimRig.AssignObjectToLookAt(objectToLookAt);
        EquipWeaponOnGameStart();
    }

    protected virtual void Update() {
        orientation.CharacterRotation(DirectionalInput, objectToLookAt.transform, RelativeForwardDir, RelativeRightDir);
    }

    protected virtual void FixedUpdate() {
        movement.MoveTowardsDirection(DirectionalInput, IsRunning, handleCollisions: false, RelativeForwardDir, RelativeRightDir);
    }

    private void EquipWeaponOnGameStart() {
        switch (onGameStart) {
            case OnGameStart.SwitchToPrimary:
            combatManager.SwitchToPrimary();
            break;
            case OnGameStart.SwitchToSecondary:
            combatManager.SwitchToSecondary();
            break;
            case OnGameStart.SwitchToMelee:
            combatManager.SwitchToMelee();
            break;
        }
    }

    /// <summary>
    /// Sets the position of the object that the character is set to look at.
    /// </summary>
    protected void SetLookAtObjectPosition(Vector3 position) {
        Vector3 currentPosition = objectToLookAt.transform.position;
        Vector3 targetPosition = position;
        Vector3 newPosition = Vector3.Lerp(currentPosition, targetPosition, 10f * Time.deltaTime);
        objectToLookAt.transform.position = newPosition;
    }

    public virtual void TakeDamage(float amount) {
        OnCharacterTakeDamage?.Invoke(this, EventArgs.Empty);
    }

    public virtual void Respawn() {
        OnCharacterRespawn?.Invoke(this, EventArgs.Empty);
    }
}
