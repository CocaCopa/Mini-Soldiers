using UnityEngine;

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

    [SerializeField] private string m_name;
    [SerializeField] private WeaponType type;
    [SerializeField] private WeaponMode mode;
    [SerializeField] private Vector3 offset;
    [SerializeField] private GameObject bulletImpactEffect;
    [SerializeField] private GameObject muzzleEffect;
    [SerializeField] private Transform muzzleFlashTransform;
    [SerializeField] private AnimationClip shootAnimation;
    [SerializeField] private AnimationClip drawAnimation;
    [SerializeField] private float rateOfFire;
    [SerializeField] private float damage;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private AnimationCurve damageDropOff = AnimationCurve.Linear(0, 1, 1, 0);

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

    public System.Collections.IEnumerator Shoot() {
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

    private System.Collections.IEnumerator Bullet() {
        FireBulletTowardsDirection(out RaycastHit hit);
        if (hit.transform != null) {
            yield return new WaitForSeconds(BulletTravelTime(hit.point));
            SpawnImpactEffect(hit);
        }
    }

    private void FireBulletTowardsDirection(out RaycastHit hit) {
        Vector3 origin = muzzleFlashTransform.position;
        Vector3 targetPosition = controller.ObjectToLookAt.transform.position;
        targetPosition.y = origin.y;
        Vector3 direction = (targetPosition - muzzleFlashTransform.position).normalized;
        Ray ray = new Ray(origin, direction);
        float rayDistance = float.MaxValue;
        Physics.Raycast(ray, out hit, rayDistance);
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
}
