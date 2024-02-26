using CocaCopa.Utilities;
using UnityEngine;

public class AIGunner : AIController {

    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolTime;
    private float patrolTimer;
    private Vector3 moveDirection;
    private Transform targetTransform;
    private Vector3 targetMovePosition;
    private int patrolPointIndex = 0;

    protected override void Awake() {
        base.Awake();
        patrolTimer = patrolTime;
    }

    protected override void Start() {
        base.Start();
        State = AI_State.Patrol;
        SetNewDestination(patrolPoints[0].position);
        patrolPointIndex++;
    }

    protected override void Update() {
        base.Update();
        IsRunning = false;
        StateManager();
    }

    private void StateManager() {
        switch (State) {
            case AI_State.Patrol:
            StatePatrol();
            break;
            case AI_State.Investigate:
            StateInvestigate();
            break;
            case AI_State.PlayerSpotted:
            StatePlayerSpotted();
            break;
            case AI_State.Combat:
            StateCombat();
            break;
        }
    }

    private void StatePatrol() {
        if (ReachedPathDestination && Common.TickTimer(ref patrolTimer, patrolTime)) {
            if (patrolPointIndex >= patrolPoints.Length) {
                patrolPointIndex = 0;
            }

            SetNewDestination(patrolPoints[patrolPointIndex].position);
            patrolPointIndex++;
        }
        if (DirectionalInput != Vector2.zero)
        moveDirection = new Vector3(DirectionalInput.x, 0f, DirectionalInput.y) * 2f;
        Vector3 eyesLevel = Vector3.up;
        SetLookAtObjectPosition(transform.position + moveDirection + eyesLevel);

        if (Stimulus.CanSeeTarget(PlayerTransform)) {
            State = AI_State.PlayerSpotted;
        }
    }

    private void StateInvestigate() {

    }

    private void StatePlayerSpotted() {
        Vector3 hideSpot = FindHideSpot(PlayerTransform);
        SetNewDestination(hideSpot);
        SetLookAtObjectPosition(PlayerTransform.position);
        State = AI_State.Combat;
    }

    private void StateCombat() {
        if (Stimulus.CanSeeTarget(PlayerTransform)) {
            combatManager.ReleaseGunTrigger();
            combatManager.PullGunTrigger();
        }
        SetLookAtObjectPosition(PlayerTransform.position + Vector3.up);
    }
}
