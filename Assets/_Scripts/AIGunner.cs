using CocaCopa.Utilities;
using UnityEngine;

public class AIGunner : AIController {

    [Tooltip("The patrol route of the AI.")]
    [SerializeField] private Transform[] patrolPoints;
    [Tooltip("Specify the wait time before the AI moves to the next patrol point.")]
    [SerializeField] private float patrolTime;
    [Tooltip("The time in seconds before the AI can spot the player character.")]
    [SerializeField] private float timeToSpotPlayer;
    [Tooltip("The minimum time in seconds before the AI starts peeking from cover.")]
    [SerializeField] private float timeToPeekMin;
    [Tooltip("The maximum time in seconds before the AI starts peeking from cover.")]
    [SerializeField] private float timeToPeekMax;
    [Tooltip("The minimum time in seconds the AI keeps peeking from cover.")]
    [SerializeField] private float keepPeekingTimeMin;
    [Tooltip("The maximum time in seconds the AI keeps peeking from cover.")]
    [SerializeField] private float keepPeekingTimeMax;
    [Tooltip("Determine how far from the corner the AI will extend when peeking.")]
    [SerializeField] private float peekCornerDistance;
    [Tooltip("Minimum delay before the AI will start shooting when the player is in range.")]
    [SerializeField] private float delayBeforeShootingMin;
    [Tooltip("Maximum delay before the AI will start shooting when the player is in range.")]
    [SerializeField] private float delayBeforeShootingMax;
    [Tooltip("The AI will go to a new hide spot if its distance with the player character is less than the determined value.")]
    [SerializeField] private float repositionDistance;

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
        MainState = AI_State.Patrol;
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
        switch (MainState) {
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
                MainState = AI_State.PlayerSpotted;
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
            PeekCornerState = PeekCorner_SubState.Cover;
        }
    }

    private void PlayerSpotted_PeekCorner() {
        if (Stimulus.CanSeeTarget(PlayerTransform) && Vector3.Distance(PlayerTransform.position, transform.position) < repositionDistance) {
            PlayerSpottedState = PlayerSpotted_SubState.FindNewHideSpot;
        }
        switch (PeekCornerState) {
            case PeekCorner_SubState.Cover:
            PeekCorner_Cover();
            break;
            case PeekCorner_SubState.Peek:
            PeekCorner_Peek();
            break;
            case PeekCorner_SubState.Reload:
            break;
        }
    }

    private void PeekCorner_Cover() {
        // Is hiding and will peek the corner.
        if (ReachedPathDestination && nextMovePosition == peekPosition && !combatManager.IsReloading) {
            isOnHideSpot = true;
            isPeekingCorner = false;
            if (Common.TickTimer(ref timeToPeekTimer, timeToPeek, autoReset: false)) {
                timeToPeek = Random.Range(timeToPeekMin, timeToPeekMax);
                timeToPeekTimer = timeToPeek;
                nextMovePosition = hideSpotPosition;
                SetNewDestination(peekPosition);
                PeekCornerState = PeekCorner_SubState.Peek;
                IsRunning = false;
            }
        }
    }

    private void PeekCorner_Peek() {
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
                PeekCornerState = PeekCorner_SubState.Cover;
                IsRunning = true;
            }
        }
    }
}
