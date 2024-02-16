using System;
using UnityEngine;

public class CombatManager : MonoBehaviour {

    public class OnSwitchWeaponsEventArgs { 
        public GameObject equipedWeapon;
        public WeaponType weaponType;
    }

    public event EventHandler<OnSwitchWeaponsEventArgs> OnSwitchWeapons;

    [SerializeField] private Transform rightHandTransform;
    [SerializeField] private GameObject[] loadoutWeapons;
    [SerializeField] private AnimationClip defaultShootClip;

    private CharacterAnimator characterAnimator;
    private AnimationClip activeShootAnimation;
    private GameObject equipedWeapon;

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
        if (weaponObject != equipedWeapon) {
            if (equipedWeapon != null) {
                equipedWeapon.SetActive(false);
            }
            equipedWeapon = weaponObject;
            equipedWeapon.SetActive(true);
            Weapon weapon = equipedWeapon.GetComponent<Weapon>();
            AnimationClip weaponShootAnimation = weapon.ShootAnimationClip;
            characterAnimator.SetShootAnimationClip(activeShootAnimation, weaponShootAnimation);
            activeShootAnimation = weaponShootAnimation;

            OnSwitchWeapons?.Invoke(this, new OnSwitchWeaponsEventArgs {
                equipedWeapon = weaponObject,
                weaponType = weapon.Type
            });
        }
    }
}
