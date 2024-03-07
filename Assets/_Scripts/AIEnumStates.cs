public enum AI_State {
    Patrol,
    Investigate,
    PlayerSpotted,
};

public enum PlayerSpotted_SubState {
    FindNewHideSpot,
    Hide,
    PeekCorner,
}

public enum PeekCorner_SubState {
    Cover,
    Peek,
    Reload
}
