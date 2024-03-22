"use strict";

function pinr_GetLeaderboardPosition(classIndex, classPosition) {
    let leaderboardPosition = $prop('PostItNoteRacing.Class_' + (classIndex ?? '00').toString().padStart(2, '0') + '_' + (classPosition ?? '00').toString().padStart(2, '0') + '_LeaderboardPosition');

    if (leaderboardPosition === undefined) {
        return null;
    } else {
        return leaderboardPosition;
    }
}

function pinr_GetPropertyFromLeaderboardPosition(leaderboardPosition, propertyName) {
    let propertyValue = $prop('PostItNoteRacing.Drivers_' + (leaderboardPosition ?? '00').toString().padStart(2, '0') + '_' + propertyName);

    if (propertyValue === undefined) {
        return null;
    } else {
        return propertyValue;
    }
}