using UnityEngine;
using CocaCopa.Utilities;

public class BulletShell : MonoBehaviour {

    [SerializeField] private bool debugShellDirection;
    [Space(10)]
    [SerializeField] private float minForceAmount;
    [SerializeField] private float maxForceAmount;
    [SerializeField] private float minTorqueAmount;
    [SerializeField] private float maxTorqueAmount;
    [SerializeField] private float spreadDispersion;
    [SerializeField] private float minSpread, maxSpread;

    private Rigidbody m_rigidbody;

    private void Awake() {
        m_rigidbody = GetComponent<Rigidbody>();
    }

    public void ThrowBulletShell(Transform relativeTransform) {
        Vector3 direction = Quaternion.AngleAxis(65f, relativeTransform.forward) * relativeTransform.right;
        Vector3 origin = transform.position;
        Vector3 aimPosition = origin + direction * 50f;
        Vector3 forceDirection = SpreadBullet(origin, aimPosition, transform.right);
        if (debugShellDirection) {
            Debug.DrawRay(origin, forceDirection * 10f, Color.magenta, 2.5f);
        }
        float forceAmount = Random.Range(minForceAmount, maxForceAmount);
        Vector3 force = forceDirection * forceAmount;
        m_rigidbody.AddForce(force, ForceMode.Impulse);

        float torqueAmount = Random.Range(-minTorqueAmount, maxTorqueAmount);
        Vector3 torque = forceDirection * torqueAmount;
        m_rigidbody.AddTorque(torque, ForceMode.Impulse);
        Destroy(gameObject, 4f);
    }

    private Vector3 SpreadBullet(Vector3 origin, Vector3 aimPosition, Vector3 spreadAroundDirection) {
        Vector3 aimDirection = (aimPosition - origin).normalized;
        Vector3 newOrigin = origin + aimDirection * spreadDispersion;
        float bulletSpread = Random.Range(minSpread, maxSpread);
        Vector3 driftetTargetPosition = Environment.RandomVectorPointOnCircle(newOrigin, bulletSpread, spreadAroundDirection);
        return (driftetTargetPosition - origin).normalized;
    }
}
