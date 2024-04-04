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

    [Tooltip("The bone transform that will hold the character's weapons.")]
    [SerializeField] private Transform rightHandTransform;
    [Tooltip("Delay before the character will auto reload once the magazine of the weapon is empty.")]
    [SerializeField] private float autoReloadDelay = 0.2f;
    [Space(10)]
    [Tooltip("Primary / Secondary / Melee weapons of the character.")]
    [SerializeField] private GameObject[] loadoutWeapons;
    [Tooltip("The animation clip of the 'Attack' animation state referenced in the animator controller.")]
    [SerializeField] private AnimationClip[] drawAnimations;
    [SerializeField] private AnimationClip reloadAnimation;

    private Controller controller;
    private CharacterAnimator characterAnimator;
    private GameObject equippedWeaponObject;
    private Weapon equippedWeapon;
    private Coroutine reloadRoutine;
    private float relaxTime = 5f;
    private float relaxTimer;
    private bool allowAutoReload = true;
    private bool isCombatIdle;
    private bool isSwitchingWeapon;
    private bool isPullingGunTrigger;
    private bool isReloading;

    public Transform RightHandTransform => rightHandTransform;
    public Weapon EquippedWeapon => equippedWeapon;
    public bool IsCombatIdle => isCombatIdle;
    public bool IsSwitchingWeapon => isSwitchingWeapon;
    public bool IsReloading => isReloading;
    public bool IsPullingGunTrigger => isPullingGunTrigger;

    private void Awake() {
        controller = GetComponent<Controller>();
        characterAnimator = GetComponentInChildren<CharacterAnimator>();
        InitializeEquippedWeapons();
    }

    private void Start() {
        controller.OnCharacterDeath += Controller_OnCharacterDeath;
    }

    private void Controller_OnCharacterDeath(object sender, EventArgs e) {
        equippedWeaponObject.SetActive(false);
    }

    private void Update() {
        ManageIdleState();
    }

    public void SwitchWeapon(WeaponSwitch weaponSwitch) {
        SwitchWeapon(loadoutWeapons[(int)weaponSwitch]);
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

            StartCoroutine(CheckForDrawAnimation(animationPercentage: 1f));
        }
    }

    public void PullGunTrigger(bool autoReload = true) {
        if (equippedWeapon && !isReloading) {
            isCombatIdle = true;
            relaxTimer = relaxTime;
            StartCoroutine(CheckForDrawAnimation(1f));
            if (!isSwitchingWeapon) {
                isPullingGunTrigger = true;
                equippedWeapon.PullGunTrigger();
                if (autoReload && CanAutoReload()) {
                    ReloadEquippedWeapon(autoReloadDelay);
                }
            }
        }
        else {
            isPullingGunTrigger = false;
        }
    }

    public void ReleaseGunTrigger() {
        equippedWeapon.ReleaseGunTrigger();
        isPullingGunTrigger = false;
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
        if (isReloading == false && equippedWeapon.RemainingBullets < equippedWeapon.MagazineSize) {
            if (reloadRoutine != null) {
                StopCoroutine(reloadRoutine);
            }
            reloadRoutine = StartCoroutine(ReloadRoutine(delayReload));
        }
    }

    private IEnumerator ReloadRoutine(float delayReload) {
        yield return new WaitForSeconds(delayReload);
        while (isSwitchingWeapon) {
            yield return null;
        }
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
            }
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
