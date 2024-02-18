using System.Collections;
using UnityEngine;
using CocaCopa;

public enum WeaponType {
    Knife,
    Pistol,
    Rifle
};

public enum WeaponMode {
    Automatic,
    SemiAutomatic
};

public class Weapon : MonoBehaviour {

    [Tooltip("Name of the weapon")]
    [SerializeField] private string m_name;

    [Header("--- Type ---")]
    [Tooltip("Type of the weapon.")]
    [SerializeField] private WeaponType type;
    [Tooltip("Mode of the weapon.")]
    [SerializeField] private WeaponMode mode;

    [Header("--- Stats ---")]
    [Tooltip("Rate of fire of the weapon.")]
    [SerializeField] private float rateOfFire;
    [Tooltip("Speed of the bullet.")]
    [SerializeField] private float bulletSpeed;
    [Tooltip("Maximum range at which the weapon can deal damage.")]
    [SerializeField] private float range;
    [Tooltip("Damage dealt by the weapon.")]
    [SerializeField] private float damage;
    [Tooltip("Damage drop-off curve")]
    [SerializeField] private AnimationCurve damageDropOff = AnimationCurve.Linear(0, 1, 1, 0);

    [Header("--- Bullet Spread ---")]
    [Tooltip("Spread dispersion of the weapon. Higher values will soften the intensity of the bullet spread.")]
    [SerializeField] private float spreadDispersion;
    [Tooltip("Minimum spread of the weapon.")]
    [SerializeField] private float minSpread;
    [Tooltip("Maximum spread of the weapon.")]
    [SerializeField] private float maxSpread;

    [Header("--- Visuals ---")]
    [Tooltip("Effect for bullet impact.")]
    [SerializeField] private GameObject bulletImpactEffect;
    [Tooltip("Effect for muzzle flash.")]
    [SerializeField] private GameObject muzzleEffect;
    [Tooltip("Spawn transform for muzzle flash.")]
    [SerializeField] private Transform muzzleFlashTransform;
    [Tooltip("Animation for shooting.")]
    [SerializeField] private AnimationClip shootAnimation;
    [Tooltip("Animation for drawing the weapon.")]
    [SerializeField] private AnimationClip drawAnimation;

    public AnimationClip ShootAnimationClip => shootAnimation;
    public WeaponType Type => type;

    private CharacterAnimator characterAnimator;
    private Controller controller;
    private float rpmTimer;

    private void OnEnable() {
        if (characterAnimator == null) {
            characterAnimator = transform.root.GetComponentInChildren<CharacterAnimator>();
            controller = transform.root.GetComponent<Controller>();
        }
    }

    private void OnDisable() {
        if (transform.parent == null) {
            characterAnimator = null;
        }
    }

    public IEnumerator Shoot() {
        yield return new WaitForEndOfFrame();
        while (characterAnimator.IsClipPlaying(drawAnimation, 1, 0.8f)) {
            yield return null;
        }
        if (Time.time > rpmTimer && muzzleEffect) {
            GameObject muzzleEffect = Instantiate(this.muzzleEffect);
            muzzleEffect.transform.position = muzzleFlashTransform.position;
            muzzleEffect.transform.rotation = transform.rotation;
            Destroy(muzzleEffect, 5f);
            characterAnimator.PlayWeaponFireAnimation();
            rpmTimer = Time.time + (1f / rateOfFire);
            StartCoroutine(Bullet());
        }
    }

    private IEnumerator Bullet() {
        FireBulletTowardsDirection(out RaycastHit hit);
        if (hit.transform != null) {
            yield return new WaitForSeconds(BulletTravelTime(hit.point));
            SpawnImpactEffect(hit);
            if (hit.transform.TryGetComponent<IDamageable>(out var target)) {
                DealDamageToTarget(target);
            }
        }
    }

    private void FireBulletTowardsDirection(out RaycastHit hit) {
        Vector3 origin = muzzleFlashTransform.position;
        Vector3 aimPosition = controller.ObjectToLookAt.transform.position;
        //aimPosition.y = origin.y;
        Vector3 direction = SpreadBullet(origin, aimPosition);
        Ray ray = new Ray(origin, direction);
        float rayDistance = float.MaxValue;
        Physics.Raycast(ray, out hit, rayDistance);
    }

    private Vector3 SpreadBullet(Vector3 origin, Vector3 aimPosition) {
        Vector3 aimDirection = (aimPosition - muzzleFlashTransform.position).normalized;
        Vector3 newOrigin = origin + aimDirection * spreadDispersion;
        float bulletSpread = Random.Range(minSpread, maxSpread);
        Vector3 driftetTargetPosition = Utilities.RandomVectorPointOnCircle(newOrigin, bulletSpread, aimDirection);
        return (driftetTargetPosition - muzzleFlashTransform.position).normalized;
    }

    private float BulletTravelTime(Vector3 impactPosition) {
        Vector3 startPosition = muzzleFlashTransform.position;
        float travelDistance = Vector3.Distance(startPosition, impactPosition);
        return travelDistance / bulletSpeed;
    }

    private void SpawnImpactEffect(RaycastHit hit) {
        GameObject impactEffectObject = Instantiate(bulletImpactEffect);
        impactEffectObject.transform.position = hit.point;
        impactEffectObject.transform.forward = -hit.normal;
        Destroy(impactEffectObject, 5f);
    }

    private void DealDamageToTarget(IDamageable target) {

    }
}
