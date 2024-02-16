using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "New Weapon")]
public class WeaponSO : ScriptableObject {

    [SerializeField] private string m_name;
    [SerializeField] private AnimationClip shootAnimation;
    [SerializeField] private float rateOfFire;
    [SerializeField] private float damage;
    [SerializeField] private AnimationCurve damageDropOff = AnimationCurve.Linear(0, 1, 1, 0);
}
