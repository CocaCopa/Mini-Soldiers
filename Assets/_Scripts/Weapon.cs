using System.Collections;
using UnityEngine;
using CocaCopa;
using UnityEngine.Animations.Rigging;

public enum WeaponType {
    Primary,
    Secondary,
    Melee
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
    [Tooltip("Number of bullets in the weapon's magazine.")]
    [SerializeField] private int magazineSize;
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

    [Header("--- Weapon Recoil ---")]
    [SerializeField] private AnimationCurve recoilResetCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [Tooltip("The speed at which the hand of the character will return to its default position after firing their weapon.")]
    [SerializeField] private float recoilResetSpeed;
    [Tooltip("The amount of kick in the backward direction upon firing.")]
    [SerializeField] private float backwardsKickAmount;
    [Tooltip("The amount of kick in the upward direction upon firing.")]
    [SerializeField] private float upwardsKickAmount;

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

    public WeaponType Type => type;

    private OverrideTransform overrideTransform;
    private Controller controller;
    private Quaternion kickWeaponRotation = Quaternion.identity;
    private Quaternion defaultWeaponRotation = Quaternion.identity;
    private Vector3 overridePosition;
    private float rpmTimer;
    private int bulletsInMagazine;

    private float recoilPositionAnimPoints = 0f;

    private void OnEnable() {
        if (controller == null) {
            controller = transform.root.GetComponent<Controller>();
            overrideTransform = transform.root.GetComponentInChildren<OverrideTransform>();
            overridePosition = new Vector3(backwardsKickAmount, 0f, 0f);
            bulletsInMagazine = magazineSize;
        }
    }

    private void OnDisable() {
        if (transform.parent == null) {
            controller = null;
            overrideTransform = null;
            
        }
    }

    private void Update() {
        WeaponRecoilReset();
    }

    public void Reload() {
        bulletsInMagazine = magazineSize;
    }

    /// <summary>
    /// Shoots the weapon.
    /// </summary>
    /// <param name="stopAllShootRoutines">Stops all the coroutines started </param>
    public void Shoot(bool stopAllShootRoutines) {
        if (stopAllShootRoutines) {
            //StopAllCoroutines();
            return;
        }
        if (mode == WeaponMode.Automatic) {
            ShootAutomatic();
        }
        else if (mode == WeaponMode.SemiAutomatic) {

        }
    }

    private void ShootAutomatic() {
        if (Time.time > rpmTimer && bulletsInMagazine > 0) {
            GameObject muzzleEffect = Instantiate(this.muzzleEffect);
            muzzleEffect.transform.position = muzzleFlashTransform.position;
            muzzleEffect.transform.rotation = transform.rotation;
            Destroy(muzzleEffect, 5f);
            WeaponRecoil();
            bulletsInMagazine--;
            rpmTimer = Time.time + (1f / rateOfFire);
            StartCoroutine(Bullet());
        }
    }
        
    private void WeaponRecoil() {
        // Kick weapon backwards.
        overrideTransform.data.position = overridePosition;
        recoilPositionAnimPoints = 0f;

        // Kick weapon upwards.
        defaultWeaponRotation = transform.parent.localRotation;
        transform.parent.Rotate(-Vector3.up * upwardsKickAmount);
        kickWeaponRotation  = transform.parent.localRotation;
    }

    private void WeaponRecoilReset() {
        float lerpTime = Utilities.EvaluateAnimationCurve(recoilResetCurve, ref recoilPositionAnimPoints, recoilResetSpeed, increment: true);
        overrideTransform.data.position = Vector3.Lerp(overridePosition, Vector3.zero, lerpTime);

        if (kickWeaponRotation != Quaternion.identity) {
            transform.parent.localRotation = Quaternion.Lerp(kickWeaponRotation, defaultWeaponRotation, lerpTime);
        }
        
        overrideTransform.weight = recoilPositionAnimPoints == 1f ? 0f : 1f;
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
        float rayDistance = range;
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
