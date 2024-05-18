"use strict";

function driverclassposition(raceposition) {
    return sc_GetPropertyFromLeaderboardPosition(raceposition, 'LivePositionInClass');
}

function drivergaptoclassleader(raceposition) {
    return sc_GetPropertyFromLeaderboardPosition(raceposition, 'GapToClassLeader');
}

function drivergaptoclassleadercombined(raceposition) {
    return sc_GetPropertyFromLeaderboardPosition(raceposition, 'GapToClassLeaderString');
}

function drivergaptoleader(raceposition) {
    return sc_GetPropertyFromLeaderboardPosition(raceposition, 'GapToLeader');
}

function drivergaptoleadercombined(raceposition) {
    return sc_GetPropertyFromLeaderboardPosition(raceposition, 'GapToLeaderString');
}

function drivergaptoplayer(raceposition) {
    return sc_GetPropertyFromLeaderboardPosition(raceposition, 'GapToPlayer');
}

function driverpositiongain(raceposition) {
    return sc_GetPropertyFromLeaderboardPosition(raceposition, 'PositionsGained');
}

function driverpositiongainclass(raceposition) {
    return sc_GetPropertyFromLeaderboardPosition(raceposition, 'PositionsGainedInClass');
}

function driverrelativegaptoplayer(raceposition) {
    return sc_GetPropertyFromLeaderboardPosition(raceposition, 'RelativeGapToPlayer');
}

function driverrelativegaptoplayercombined(raceposition) {
    return sc_GetPropertyFromLeaderboardPosition(raceposition, 'GapToPlayerString');
}

function driverstartposition(raceposition) {
    return sc_GetPropertyFromLeaderboardPosition(raceposition, 'GridPosition');
}

function driverstartpositionclass(raceposition) {
    return sc_GetPropertyFromLeaderboardPosition(raceposition, 'GridPositionInClass');
}