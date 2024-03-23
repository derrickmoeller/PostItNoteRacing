"use strict";

function mc_GetLeaderboardPosition(classPosition) {
    let classIndex = sc_GetPropertyFromLeaderboardPosition(sc_GetPlayerLeaderboardPosition(), 'ClassIndex');
    
    let leaderboardPosition = $prop('PostItNoteRacing.Class_' + (classIndex ?? '00').toString().padStart(2, '0') + '_' + (classPosition ?? '00').toString().padStart(2, '0') + '_LeaderboardPosition');

    if (leaderboardPosition === undefined) {
        return null;
    } else {
        return leaderboardPosition;
    }
}

function mc_GetPropertyFromClassPosition(classPosition, propertyName) {
    let leaderboardPosition = mc_GetLeaderboardPosition(classPosition);

    return sc_GetPropertyFromLeaderboardPosition(leaderboardPosition, propertyName);
}

function mc_GetPropertyFromRelativePosition(relativePosition, propertyName) {
    let positionInClass = sc_GetPropertyFromLeaderboardPosition(sc_GetPlayerLeaderboardPosition(), 'LivePositionInClass');

    return mc_GetPropertyFromClassPosition(positionInClass + relativePosition, propertyName);
}