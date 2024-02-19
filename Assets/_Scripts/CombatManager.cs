using CocaCopa;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class CombatManager : MonoBehaviour {

    public class OnSwitchWeaponsEventArgs { 
        public GameObject equipedWeapon;
        public WeaponType weaponType;
    }

    public event EventHandler<OnSwitchWeaponsEventArgs> OnSwitchWeapons;
    public event EventHandler OnInitiateWeaponReload;

    [Tooltip("Primary / Secondary / Melee weapons of the character.")]
    [SerializeField] private GameObject[] loadoutWeapons;
    [Tooltip("The bone transform that will hold the character's weapons.")]
    [SerializeField] private Transform rightHandTransform;
    [SerializeField] private MultiAimConstraint handAimConstraint;
    [Tooltip("The animation clip of the 'Attack' animation state referenced in the animator controller.")]
    [SerializeField] private AnimationClip[] drawAnimations;
    [SerializeField] private AnimationClip reloadAnimation;

    private CharacterAnimator characterAnimator;
    private GameObject equippedWeaponObject;
    private Weapon equippedWeapon;
    private bool isSwitchingWeapon;
    private bool isCombatIdle;
    private float relaxTime = 5f;
    private float relaxTimer;
    private float constraintAnimationPoints;
    private AnimationCurve constraintCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    private bool isReloading;

    public Weapon EquippedWeapon => equippedWeapon;
    public bool IsCombatIdle => isCombatIdle;
    public bool IsSwitchingWeapon => isSwitchingWeapon;
    public bool IsReloading => isReloading;

    private void Awake() {
        characterAnimator = GetComponentInChildren<CharacterAnimator>();
        InitializeEquippedWeapons();
    }

    private void Update() {
        ManageIdleState();
    }

    public void SwitchToPrimary() {
        SwitchWeapon(loadoutWeapons[0]);
    }

    public void SwitchToSecondary() {
        SwitchWeapon(loadoutWeapons[1]);
    }

    public void SwitchToMelee() {
        SwitchWeapon(loadoutWeapons[2]);
    }

    private void SwitchWeapon(GameObject weaponObject) {
        if (weaponObject != equippedWeaponObject) {
            if (equippedWeaponObject != null) {
                equippedWeaponObject.SetActive(false);
            }
            equippedWeaponObject = weaponObject;
            equippedWeaponObject.SetActive(true);
            equippedWeapon = equippedWeaponObject.GetComponent<Weapon>();
            isCombatIdle = true;
            relaxTimer = relaxTime;
            StartCoroutine(CheckForDrawAnimation(0.55f));

            OnSwitchWeapons?.Invoke(this, new OnSwitchWeaponsEventArgs {
                equipedWeapon = weaponObject,
                weaponType = equippedWeapon.Type
            });

            isSwitchingWeapon = true;
        }
    }

    public void FireEquippedWeapon() {
        if (equippedWeapon && !isReloading) {
            isCombatIdle = true;
            relaxTimer = relaxTime;
            StartCoroutine(CheckForDrawAnimation(1f));
            if (!isSwitchingWeapon) {
                equippedWeapon.Shoot();
            }
        }
    }
    
    public void ReloadEquippedWeapon() {
        if (isReloading == false) {
            isReloading = true;
            OnInitiateWeaponReload?.Invoke(this, EventArgs.Empty);
            StartCoroutine(ReloadRoutine());
        }
    }

    private IEnumerator ReloadRoutine() {
        yield return new WaitForEndOfFrame();
        while (characterAnimator.IsClipPlaying(reloadAnimation, 1, 1)) {
            yield return null;
        }
        isReloading = false;
        equippedWeapon.Reload();
    }

    private void ManageIdleState() {
        if (isCombatIdle) {
            if (Utilities.TickTimer(ref relaxTimer, relaxTime, false)) {
                isCombatIdle = false;
                handAimConstraint.weight = 0f;
            }
        }

        if (isSwitchingWeapon) {
            constraintAnimationPoints = 0f;
            handAimConstraint.weight = 0f;
        }
        else if (isCombatIdle && handAimConstraint.weight != 1f) {
            float lerpTime = Utilities.EvaluateAnimationCurve(constraintCurve, ref constraintAnimationPoints, 6.75f);
            handAimConstraint.weight = Mathf.Lerp(0f, 1f, lerpTime);
        }
    }

    private IEnumerator CheckForDrawAnimation(float animationPercentage) {
        yield return new WaitForEndOfFrame();

        foreach (var animation in drawAnimations) {
            while (characterAnimator.IsClipPlaying(animation, 1, animationPercentage)) {
                isSwitchingWeapon = true;
                yield return null;
            }
        }
        isSwitchingWeapon = false;
    }

    private void InitializeEquippedWeapons() {
        for (int i = 0; i < loadoutWeapons.Length; i++) {
            loadoutWeapons[i] = Instantiate(loadoutWeapons[i]);
            loadoutWeapons[i].transform.parent = rightHandTransform;
            loadoutWeapons[i].transform.localPosition = Vector3.zero;
            loadoutWeapons[i].transform.localEulerAngles = Vector3.zero;
            loadoutWeapons[i].SetActive(false);
        }
    }
}
