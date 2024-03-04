using CocaCopa.Utilities;
using UnityEngine;

public class AIGunner : AIController {

    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolTime;
    [SerializeField] private float timeToSpotPlayer;
    [SerializeField] private float timeToPeekMin;
    [SerializeField] private float timeToPeekMax;
    [SerializeField] private float keepPeekingTimeMin;
    [SerializeField] private float keepPeekingTimeMax;
    [SerializeField] private float peekCornerDistance;
    [SerializeField] private float delayBeforeShootingMin;
    [SerializeField] private float delayBeforeShootingMax;

    private readonly Vector3 RAISE_POSITION_TO_EYES_LEVEL = Vector3.up;

    private bool isOnHideSpot;
    private bool isPeekingCorner;

    private float patrolTimer;
    private float timeToSpotPlayerTimer;
    private float delayBeforeShootingTimer;
    private float timeToPeekTimer;
    private float keepPeekingTimer;

    [HideInInspector] private float delayBeforeShootingTime;
    [HideInInspector] private float timeToPeek;
    [HideInInspector] private float keepPeekingTime;
    
    private int patrolPointIndex = 0;
    private Vector3[] wallEdges;
    private Vector3 moveDirection;
    private Vector3 targetLookAtPosition;
    private Vector3 hideSpotPosition;
    private Vector3 nextMovePosition;
    private Vector3 peekPosition;
    private Vector3 peekDirection;


    protected override void Awake() {
        base.Awake();
        InitializeTimers();
    }

    protected override void Start() {
        base.Start();
        State = AI_State.Patrol;
        SetNewDestination(patrolPoints[0].position);
        patrolPointIndex++;
    }

    protected override void Update() {
        base.Update();
        StateManager();
        SetLookAtObjectPosition(targetLookAtPosition);
        moveDirection = new Vector3(DirectionalInput.x, 0f, DirectionalInput.y);
    }

    private void InitializeTimers() {
        patrolTimer = patrolTime;
        timeToSpotPlayerTimer = timeToSpotPlayer;
        keepPeekingTime = Random.Range(keepPeekingTimeMin, keepPeekingTimeMax);
        keepPeekingTimer = keepPeekingTime;
        timeToPeek = Random.Range(timeToPeekMin, timeToPeekMax);
        timeToPeekTimer = timeToPeek;
        delayBeforeShootingTime = Random.Range(delayBeforeShootingMin, delayBeforeShootingMax);
        delayBeforeShootingTimer = delayBeforeShootingTime;
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
        }
    }

    private void StatePatrol() {
        IsRunning = false;
        if (ReachedPathDestination && Common.TickTimer(ref patrolTimer, patrolTime)) {
            if (patrolPointIndex >= patrolPoints.Length) {
                patrolPointIndex = 0;
            }

            SetNewDestination(patrolPoints[patrolPointIndex].position);
            patrolPointIndex++;
        }
        if (DirectionalInput != Vector2.zero) {
            targetLookAtPosition = transform.position + moveDirection * 2f + RAISE_POSITION_TO_EYES_LEVEL;
        }

        if (Stimulus.CanSeeTarget(PlayerTransform)) {
            targetLookAtPosition = PlayerTransform.position;
            if (Common.TickTimer(ref timeToSpotPlayerTimer, timeToSpotPlayer)) {
                State = AI_State.PlayerSpotted;
                PlayerSpottedState = PlayerSpotted_SubState.FindNewHideSpot;
            }
        }
        else if (timeToSpotPlayerTimer != timeToSpotPlayer) {
            timeToSpotPlayerTimer = timeToSpotPlayer;
        }
    }

    private void StateInvestigate() {

    }

    private void StatePlayerSpotted() {
        if (Stimulus.CanSeeTarget(PlayerTransform)) {
            targetLookAtPosition = PlayerTransform.position + RAISE_POSITION_TO_EYES_LEVEL;
            if (Common.TickTimer(ref delayBeforeShootingTimer, delayBeforeShootingTime, false)) {
                combatManager.PullGunTrigger();
            }
        }
        else if (!Stimulus.CanSeeTarget(PlayerTransform) && delayBeforeShootingTimer == 0) {
            delayBeforeShootingTime = Random.Range(delayBeforeShootingMin, delayBeforeShootingMax);
            delayBeforeShootingTimer = delayBeforeShootingTime;
        }

        switch (PlayerSpottedState) {
            case PlayerSpotted_SubState.FindNewHideSpot:
            PlayerSpotted_FindNewHideSpot();
            break;
            case PlayerSpotted_SubState.Hide:
            PlayerSpotted_Hide();
            break;
            case PlayerSpotted_SubState.PeekCorner:
            PlayerSpotted_PeekCorner();
            break;
        }
    }

    private void PlayerSpotted_FindNewHideSpot() {
        hideSpotPosition = FindHideSpot(PlayerTransform);
        SetNewDestination(hideSpotPosition);
        IsRunning = true;
        PlayerSpottedState = PlayerSpotted_SubState.Hide;
    }

    private void PlayerSpotted_Hide() {
        if (ReachedPathDestination) {
            peekPosition = CalculatePeekPosition(peekCornerDistance, out peekDirection);
            nextMovePosition = peekPosition;
            PlayerSpottedState = PlayerSpotted_SubState.PeekCorner;
        }
    }

    private void PlayerSpotted_PeekCorner() {
        IsRunning = false;
        // Is hiding and will peek the corner.
        if (ReachedPathDestination && nextMovePosition == peekPosition && !combatManager.IsReloading) {
            isOnHideSpot = true;
            isPeekingCorner = false;
            if (Common.TickTimer(ref timeToPeekTimer, timeToPeek, autoReset: false)) {
                timeToPeek = Random.Range(timeToPeekMin, timeToPeekMax);
                timeToPeekTimer = timeToPeek;
                nextMovePosition = hideSpotPosition;
                SetNewDestination(peekPosition);
            }
        }
        // Is peeking the corner and will hide.
        if (ReachedPathDestination && nextMovePosition == hideSpotPosition) {
            isPeekingCorner = true;
            isOnHideSpot = false;
            if (combatManager.EquippedWeapon.RemainingBullets == 0 && isPeekingCorner) {
                keepPeekingTimer = keepPeekingTime = 0f;
            }
            if (Common.TickTimer(ref keepPeekingTimer, keepPeekingTime, autoReset: false)) {
                keepPeekingTime = Random.Range(keepPeekingTimeMin, keepPeekingTimeMax);
                keepPeekingTimer = keepPeekingTime;
                nextMovePosition = peekPosition;
                SetNewDestination(hideSpotPosition);
            }
        }
    }
}
