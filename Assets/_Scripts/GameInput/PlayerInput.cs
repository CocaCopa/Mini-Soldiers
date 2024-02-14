using UnityEngine;

public class PlayerInput : MonoBehaviour {

    private InputActions inputActions;

    private void Awake() {
        inputActions = new InputActions();
        inputActions.Player.Enable();
    }

    public Vector2 MovementInput() => inputActions.Player.Movement.ReadValue<Vector2>();
}
