public enum AI_State {
    Unassigned,
    Dead,
    Patrol,
    Investigate,
    PlayerSpotted,
};

public enum PlayerSpotted_SubState {
    Unassigned,
    FindNewHideSpot,
    Hide,
    PeekCorner,
}

public enum PeekCorner_SubState {
    Unassigned,
    Cover,
    Peek,
    Reload
}
