using System;
using UnityEngine;

public class CombatManager : MonoBehaviour {

    public class OnSwitchWeaponsEventArgs { 
        public GameObject equipedWeapon;
        public WeaponType weaponType;
    }

    public event EventHandler<OnSwitchWeaponsEventArgs> OnSwitchWeapons;

    [Tooltip("Primary / Secondary / Melee weapons of the character.")]
    [SerializeField] private GameObject[] loadoutWeapons;
    [Tooltip("The bone transform that will hold the character's weapons.")]
    [SerializeField] private Transform rightHandTransform;
    [Tooltip("The animation clip of the 'Attack' animation state referenced in the animator controller.")]
    [SerializeField] private AnimationClip defaultShootClip;

    private CharacterAnimator characterAnimator;
    private AnimationClip activeShootAnimation;
    private GameObject equipedWeaponObject;
    private Weapon equipedWeapon;

    public GameObject EquipedWeapon => equipedWeaponObject;

    private void Awake() {
        characterAnimator = GetComponentInChildren<CharacterAnimator>();
        activeShootAnimation = defaultShootClip;

        for (int i = 0; i < loadoutWeapons.Length; i++) {
            loadoutWeapons[i] = Instantiate(loadoutWeapons[i]);
            loadoutWeapons[i].transform.parent = rightHandTransform;
            loadoutWeapons[i].transform.localPosition = Vector3.zero;
            loadoutWeapons[i].transform.localEulerAngles = Vector3.zero;
            loadoutWeapons[i].SetActive(false);
        }
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
        if (weaponObject != equipedWeaponObject) {
            if (equipedWeaponObject != null) {
                equipedWeaponObject.SetActive(false);
            }
            equipedWeaponObject = weaponObject;
            equipedWeaponObject.SetActive(true);
            equipedWeapon = equipedWeaponObject.GetComponent<Weapon>();
            AnimationClip weaponShootAnimation = equipedWeapon.ShootAnimationClip;
            characterAnimator.SetShootAnimationClip(activeShootAnimation, weaponShootAnimation);
            activeShootAnimation = weaponShootAnimation;

            OnSwitchWeapons?.Invoke(this, new OnSwitchWeaponsEventArgs {
                equipedWeapon = weaponObject,
                weaponType = equipedWeapon.Type
            });
        }
    }

    public void FireEquipedWeapon() {
        if (equipedWeapon != null) {
            StartCoroutine(equipedWeapon.Shoot());
        }
    }
}
