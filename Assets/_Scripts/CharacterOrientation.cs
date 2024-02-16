using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterOrientation : MonoBehaviour {

    [SerializeField] private float rotationSpeed;

    private Vector2 lastDirectionalInput;
    private Vector3 lookDirection;
    private bool movingBackwards;

    public bool MovingBackwards => movingBackwards;

    public void CharacterRotation(Vector2 directionalInput, Transform relativeTransform) {
        if (directionalInput != Vector2.zero) {
            lastDirectionalInput = directionalInput;
        }

        Vector3 inputDirection = lastDirectionalInput == Vector2.zero
            ? Vector3.forward
            : Vector3.forward * lastDirectionalInput.y + Vector3.right * lastDirectionalInput.x;
        inputDirection.Normalize();

        Vector3 characterToMouseDirection = (relativeTransform.position - transform.position).normalized;

        if (Vector3.Dot(inputDirection, characterToMouseDirection) > 0) {
            Vector3 targetDirection = Vector3.Dot(lookDirection, inputDirection) <= 0f ? characterToMouseDirection : inputDirection;
            lookDirection = Vector3.RotateTowards(lookDirection, targetDirection, Time.deltaTime * rotationSpeed, 1f);
            movingBackwards = false;
        }
        else {
            Vector3 targetDirection = Vector3.Dot(lookDirection, -inputDirection) <= 0f ? characterToMouseDirection : -inputDirection;
            lookDirection = Vector3.RotateTowards(lookDirection, targetDirection, Time.deltaTime * rotationSpeed, 1f);
            movingBackwards = true;
        }

        transform.forward = lookDirection;

        Vector3 eulerAngles = transform.eulerAngles;
        eulerAngles.x = eulerAngles.z = 0;
        transform.eulerAngles = eulerAngles;
    }
}
