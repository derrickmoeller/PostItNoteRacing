"use strict";

function mc_GetLeaderboardPosition(classPosition) {
    let classIndex = sc_GetPropertyFromLeaderboardPosition(getplayerleaderboardposition(), 'ClassIndex');
    
    return $prop('PostItNoteRacing.Class_' + (classIndex ?? '00').toString().padStart(2, '0') + '_' + (classPosition ?? '00').toString().padStart(2, '0') + '_LeaderboardPosition');
}

function mc_GetPropertyFromClassPosition(classPosition, propertyName) {
    let leaderboardPosition = mc_GetLeaderboardPosition(classPosition);

    return sc_GetPropertyFromLeaderboardPosition(leaderboardPosition, propertyName);
}

function mc_GetPropertyFromRelativePosition(relativePosition, propertyName) {
    let positionInClass = sc_GetPropertyFromLeaderboardPosition(getplayerleaderboardposition(), 'LivePositionInClass') + relativePosition;

    return mc_GetPropertyFromClassPosition(positionInClass, propertyName);
}