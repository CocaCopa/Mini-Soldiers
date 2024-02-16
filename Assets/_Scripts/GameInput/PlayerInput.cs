using System.Linq;
using UnityEngine;

public class PlayerInput : MonoBehaviour {

    private InputActions inputActions;

    private bool runKeyPressed = false;

    private void Awake() {
        inputActions = new InputActions();
        inputActions.Player.Enable();
        inputActions.Player.Run.performed += Run_performed;
        inputActions.Player.Run.canceled += Run_canceled;
    }

    private void Run_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        runKeyPressed = true;
    }

    private void Run_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        runKeyPressed = false;
    }

    public Vector2 MovementInput() => inputActions.Player.Movement.ReadValue<Vector2>();

    public Vector3 MouseWorldPosition() {
        Vector3 mouseScreenPosition = Input.mousePosition;
        Ray mouseRay = Camera.main.ScreenPointToRay(mouseScreenPosition);

        if (Physics.Raycast(mouseRay, out RaycastHit hit)) {
            Vector3 mouseHitPoint = hit.point;
            mouseHitPoint.y = transform.position.y;
            return mouseHitPoint;
        }
        return Vector3.zero;
    }
}
