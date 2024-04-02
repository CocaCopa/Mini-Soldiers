using UnityEditor;

[CustomEditor(typeof(Weapon))]
public class WeaponEditor : Editor {

    SerializedProperty m_name;

    private void OnEnable() {
        m_name = serializedObject.FindProperty(nameof(m_name));
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        serializedObject.Update();
        SetDefaultWeaponName();
        serializedObject.ApplyModifiedProperties();
    }

    private void SetDefaultWeaponName() {
        if (string.IsNullOrEmpty(m_name.stringValue) || string.IsNullOrWhiteSpace(m_name.stringValue)) {
            Weapon weapon = target as Weapon;
            m_name.stringValue = weapon.transform.name;
        }
    }
}
