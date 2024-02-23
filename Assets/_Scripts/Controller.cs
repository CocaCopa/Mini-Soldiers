using System;
using UnityEngine;

public abstract class Controller : MonoBehaviour, IDamageable {

    public event EventHandler OnCharacterTakeDamage;
    public event EventHandler OnCharacterRespawn;

    private enum OnGameStart { SwitchToPrimary, SwitchToSecondary, SwitchToMelee }

    [Tooltip("When the game starts, the selected weapon will be automatically equipped.")]
    [SerializeField] private OnGameStart onGameStart;

    private LookAtObjectAnimRig objectAnimRig;
    protected CharacterMovement movement;
    protected CharacterOrientation orientation;
    protected CombatManager combatManager;
    protected GameObject objectToLookAt;

    public GameObject ObjectToLookAt => objectToLookAt;
    public float CurrentMovementSpeed => movement.CurrentSpeed;

    protected virtual void Awake() {
        objectAnimRig = GetComponentInChildren<LookAtObjectAnimRig>();
        movement = GetComponent<CharacterMovement>();
        orientation = GetComponent<CharacterOrientation>();
        combatManager = GetComponent<CombatManager>();
    }

    protected virtual void Start() {
        objectAnimRig.AssignObjectToLookAt(objectToLookAt);
        EquipWeaponOnGameStart();
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

    public virtual void TakeDamage(float amount) {
        OnCharacterTakeDamage?.Invoke(this, EventArgs.Empty);
    }

    public virtual void Respawn() {
        OnCharacterRespawn?.Invoke(this, EventArgs.Empty);
    }
}
