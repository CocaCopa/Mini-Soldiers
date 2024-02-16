using UnityEngine;

public enum WeaponType {
    Knife,
    Pistol,
    Rifle
};

public class Weapon : MonoBehaviour {

    [SerializeField] private string m_name;
    [SerializeField] private WeaponType type;
    [SerializeField] private GameObject muzzleEffect;
    [SerializeField] private Transform muzzleFlashTransform;
    [SerializeField] private AnimationClip shootAnimation;
    [SerializeField] private float rateOfFire;
    [SerializeField] private float damage;
    [SerializeField] private AnimationCurve damageDropOff = AnimationCurve.Linear(0, 1, 1, 0);

    public AnimationClip ShootAnimationClip => shootAnimation;
    public WeaponType Type => type;

    public void Shoot() {
        GameObject muzzleEffect = Instantiate(this.muzzleEffect);
        muzzleEffect.transform.SetPositionAndRotation(muzzleFlashTransform.position, Quaternion.identity);
        Destroy(muzzleEffect, 5f);
    }
}
