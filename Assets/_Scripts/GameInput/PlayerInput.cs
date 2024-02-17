using System;
using UnityEngine;

public class PlayerInput : MonoBehaviour {

    public event EventHandler OnFirePressed;
    public event EventHandler OnPrimarySwitchPressed;
    public event EventHandler OnSecondarySwitchPressed;
    public event EventHandler OnMeleeSwitchPressed;

    private InputActions inputActions;

    private bool runInputHold = false;
    private bool fireInputHold = false;

    private void Awake() {
        inputActions = new InputActions();
        inputActions.Enable();
        inputActions.Motion.Run.performed += Run_performed;
        inputActions.Motion.Run.canceled += Run_canceled;
        inputActions.Combat.Fire.performed += Fire_performed;
        inputActions.Combat.Fire.canceled += Fire_canceled;
        inputActions.Combat.PrimarySwitch.performed += PrimarySwitch_performed;
        inputActions.Combat.SecondarySwitch.performed += SecondarySwitch_performed;
        inputActions.Combat.MeleeSwitch.performed += MeleeSwitch_performed;
    }

    private void PrimarySwitch_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnPrimarySwitchPressed?.Invoke(this, EventArgs.Empty);
    }

    private void SecondarySwitch_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnSecondarySwitchPressed?.Invoke(this, EventArgs.Empty);
    }

    private void MeleeSwitch_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnMeleeSwitchPressed?.Invoke(this, EventArgs.Empty);
    }

    public void CombatControlsEnabled(bool enabled) {
        if (enabled) {
            inputActions.Combat.Disable();
        }
        else {
            inputActions.Combat.Enable();
        }
    }

    public void MotionControlsEnabled(bool enabled) {
        if (enabled) {
            inputActions.Motion.Enable();
        }
        else {
            inputActions.Motion.Disable();
        }
    }

    private void Fire_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        fireInputHold = false;
    }

    private void Fire_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        fireInputHold = true;
        OnFirePressed?.Invoke(this, EventArgs.Empty);
    }

    private void Run_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        runInputHold = true;
    }

    private void Run_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        runInputHold = false;
    }

    public bool RunInputHold() => runInputHold;
    public bool FireInputHold() => fireInputHold;

    public Vector2 MovementInput() => inputActions.Motion.Movement.ReadValue<Vector2>();

    public Vector3 MouseWorldPosition() {
        Vector3 mouseScreenPosition = Input.mousePosition;
        Ray mouseRay = Camera.main.ScreenPointToRay(mouseScreenPosition);

        if (Physics.Raycast(mouseRay, out RaycastHit hit, float.MaxValue/*, LayerMask.GetMask("Ground")*/)) {
            Vector3 mouseHitPoint = hit.point;
            return mouseHitPoint;
        }
        return Vector3.zero;
    }
}
