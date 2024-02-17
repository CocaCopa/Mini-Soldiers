using UnityEngine;

public abstract class Controller : MonoBehaviour, IDamageable {

    private LookAtObjectAnimRig objectAnimRig;
    protected CharacterMovement movement;
    protected CharacterOrientation orientation;
    protected CombatManager combatManager;

    [SerializeField] protected GameObject objectToLookAt;

    public GameObject ObjectToLookAt => objectToLookAt;

    protected virtual void Awake() {
        objectAnimRig = GetComponentInChildren<LookAtObjectAnimRig>();
        movement = GetComponent<CharacterMovement>();
        orientation = GetComponent<CharacterOrientation>();
        combatManager = GetComponent<CombatManager>();
    }

    protected virtual void Start() {
        objectAnimRig.AssignObjectToLookAt(objectToLookAt);
    }

    public virtual void TakeDamage(float amount) {

    }


}
