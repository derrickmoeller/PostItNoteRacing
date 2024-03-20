"use strict";

function mc_GetLeaderboardPosition(classPosition) {
    let classIndex = pinr_GetPropertyFromLeaderboardPosition(getplayerleaderboardposition(), 'ClassIndex');

    return pinr_GetLeaderboardPosition(classIndex, classPosition);
}

function mc_GetPropertyFromClassPosition(classPosition, propertyName) {
    let leaderboardPosition = mc_GetLeaderboardPosition(classPosition);

    return pinr_GetPropertyFromLeaderboardPosition(leaderboardPosition, propertyName);
}

function mc_GetPropertyFromRelativePosition(relativePosition, propertyName) {
    let positionInClass = pinr_GetPropertyFromLeaderboardPosition(getplayerleaderboardposition(), 'LivePositionInClass');

    return mc_GetPropertyFromClassPosition(positionInClass + relativePosition, propertyName);
}