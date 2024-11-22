using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public abstract class Controller : MonoBehaviour, IDamageable {

    public event EventHandler OnCharacterTakeDamage;
    public event EventHandler OnCharacterRespawn;
    public event EventHandler OnCharacterDeath;

    private enum OnGameStart { SwitchToPrimary, SwitchToSecondary, SwitchToMelee }

    [Tooltip("When the game starts, the selected weapon will be automatically equipped.")]
    [SerializeField] private OnGameStart onGameStart;
    [Tooltip("How fast should the character turn their head to look at the 'ObjectToLookAt' target.")]
    [SerializeField] private float lookAtTargetObjectSpeed = 10f;
    [Tooltip("The maximum health points of the character.")]
    [SerializeField] private float maximumHealthPoints;

    private CapsuleCollider characterCollider;
    private LookAtObjectAnimRig objectAnimRig;
    private Rig characterRig;
    private GameObject objectToLookAt;
    private CharacterMovement movement;
    private CharacterOrientation orientation;
    protected CombatManager combatManager;

    [SerializeField] private float currentHealthPoints;
    private bool isAlive = true;

    public bool IsAlive => isAlive;
    protected float CurrentHealthPoints => currentHealthPoints;
    protected bool IsRunning { get; set; }
    protected Vector2 DirectionalInput { get; set; }
    protected Vector3 RelativeForwardDir { get; set; }
    protected Vector3 RelativeRightDir { get; set; }

    public GameObject ObjectToLookAt => objectToLookAt;
    public float CurrentMovementSpeed => movement.CurrentSpeed;

    protected virtual void Awake() {
        characterCollider = GetComponent<CapsuleCollider>();
        objectAnimRig = GetComponentInChildren<LookAtObjectAnimRig>();
        characterRig = objectAnimRig.GetComponent<Rig>();
        movement = GetComponent<CharacterMovement>();
        orientation = GetComponent<CharacterOrientation>();
        combatManager = GetComponent<CombatManager>();

        currentHealthPoints = maximumHealthPoints;
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
        if (isAlive) {
            movement.MoveTowardsDirection(DirectionalInput, IsRunning, RelativeForwardDir, RelativeRightDir);
        }
    }

    private void EquipWeaponOnGameStart() {
        switch (onGameStart) {
            case OnGameStart.SwitchToPrimary:
            combatManager.SwitchWeapon(WeaponSwitch.Primary);
            break;
            case OnGameStart.SwitchToSecondary:
            combatManager.SwitchWeapon(WeaponSwitch.Secondary);
            break;
            case OnGameStart.SwitchToMelee:
            combatManager.SwitchWeapon(WeaponSwitch.Melee);
            break;
        }
    }

    /// <summary>
    /// Sets the position of the object that the character is set to look at.
    /// </summary>
    protected void SetLookAtObjectPosition(Vector3 position) {
        Vector3 currentPosition = objectToLookAt.transform.position;
        Vector3 targetPosition = position;
        float lerpTime = lookAtTargetObjectSpeed * Time.deltaTime;
        Vector3 newPosition = Vector3.Lerp(currentPosition, targetPosition, lerpTime);
        objectToLookAt.transform.position = newPosition;
    }

    public virtual void TakeDamage(float amount) {
        currentHealthPoints -= amount;
        OnCharacterTakeDamage?.Invoke(this, EventArgs.Empty);
        if (currentHealthPoints <= 0) {
            isAlive = false;
            characterCollider.enabled = false;
            characterRig.weight = 0f;
            OnCharacterDeath?.Invoke(this, EventArgs.Empty);
        }
    }

    public virtual void Respawn() {
        isAlive = true;
        OnCharacterRespawn?.Invoke(this, EventArgs.Empty);
    }
}
