using System;
using System.Collections;
using UnityEngine;
using CocaCopa.Utilities;
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
    [Tooltip("")]
    [SerializeField] private float autoReloadDelay = 0.2f;
    [Tooltip("The bone transform that will hold the character's weapons.")]
    [SerializeField] private Transform rightHandTransform;
    [SerializeField] private MultiAimConstraint handAimConstraint;
    [Tooltip("The animation clip of the 'Attack' animation state referenced in the animator controller.")]
    [SerializeField] private AnimationClip[] drawAnimations;
    [SerializeField] private AnimationClip reloadAnimation;

    private CharacterAnimator characterAnimator;
    private GameObject equippedWeaponObject;
    private Weapon equippedWeapon;
    private Coroutine reloadRoutine;
    private bool isSwitchingWeapon;
    private bool isCombatIdle;
    private float relaxTime = 5f;
    private float relaxTimer;
    private float constraintAnimationPoints;
    private AnimationCurve constraintCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    private bool allowAutoReload = true;
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
            if (reloadRoutine != null) {
                StopCoroutine(reloadRoutine);
                isReloading = false;
                reloadRoutine = null;
            }
            equippedWeaponObject = weaponObject;
            equippedWeaponObject.SetActive(true);
            equippedWeapon = equippedWeaponObject.GetComponent<Weapon>();
            isCombatIdle = true;
            relaxTimer = relaxTime;
            isSwitchingWeapon = true;
            allowAutoReload = true;

            OnSwitchWeapons?.Invoke(this, new OnSwitchWeaponsEventArgs {
                equipedWeapon = weaponObject,
                weaponType = equippedWeapon.Type
            });

            StartCoroutine(CheckForDrawAnimation(1));
        }
    }

    public void FireEquippedWeapon(bool autoReload = true) {
        if (equippedWeapon && !isReloading) {
            isCombatIdle = true;
            relaxTimer = relaxTime;
            StartCoroutine(CheckForDrawAnimation(1f));
            if (!isSwitchingWeapon) {
                equippedWeapon.Shoot();
                if (autoReload && CanAutoReload()) {
                    ReloadEquippedWeapon(autoReloadDelay);
                }
            }
        }
    }

    private bool CanAutoReload() {
        if (equippedWeapon.RemainingBullets == 0 && allowAutoReload) {
            allowAutoReload = false;
            return true;
        }
        else if (equippedWeapon.RemainingBullets > 0 && !allowAutoReload) {
            allowAutoReload = true;
            return false;
        }
        return false;
    }
    
    public void ReloadEquippedWeapon(float delayReload = 0f) {
        if (isReloading == false) {
            if (reloadRoutine != null) {
                StopCoroutine(reloadRoutine);
            }
            reloadRoutine = StartCoroutine(ReloadRoutine(delayReload));
        }
    }

    private IEnumerator ReloadRoutine(float delayReload) {
        yield return new WaitForSeconds(delayReload);
        isReloading = true;
        isCombatIdle = true;
        relaxTimer = relaxTime;
        OnInitiateWeaponReload?.Invoke(this, EventArgs.Empty);

        yield return new WaitForEndOfFrame();
        while (characterAnimator.IsTransitioningToClip(reloadAnimation, 1) || characterAnimator.IsClipPlaying(reloadAnimation, 1, 1)) {
            yield return null;
        }
        isReloading = false;
        equippedWeapon.Reload();
    }

    private void ManageIdleState() {
        if (isCombatIdle) {
            if (Common.TickTimer(ref relaxTimer, relaxTime)) {
                isCombatIdle = false;
                handAimConstraint.weight = 0f;
            }
        }

        if (isSwitchingWeapon) {
            constraintAnimationPoints = 0f;
            handAimConstraint.weight = 0f;
        }
        else if (isCombatIdle && handAimConstraint.weight != 1f) {
            float lerpTime = Common.EvaluateAnimationCurve(constraintCurve, ref constraintAnimationPoints, 6.75f);
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
