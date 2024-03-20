"use strict";

function pinr_GetLeaderboardPosition(classIndex, classPosition) {
    return $prop('PostItNoteRacing.Class_' + classIndex.toString().padStart(2, '0') + '_' + classPosition.toString().padStart(2, '0') + '_LeaderboardPosition');
}

function pinr_GetPropertyFromLeaderboardPosition(leaderboardPosition, propertyName) {
    return $prop('PostItNoteRacing.Drivers_' + leaderboardPosition.toString().padStart(2, '0') + '_' + propertyName);
}