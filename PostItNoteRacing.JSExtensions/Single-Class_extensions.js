"use strict";

function sc_GetPropertyFromAheadBehind(aheadBehind, propertyName) {
    let leaderboardPosition = null;    

    if (aheadBehind > 0) {
        leaderboardPosition = $prop('PostItNoteRacing.Behind_' + aheadBehind.toString().padStart(2, '0') + '_LeaderboardPosition');
    } else if (aheadBehind < 0) {
        leaderboardPosition = $prop('PostItNoteRacing.Ahead_' + (-aheadBehind).toString().padStart(2, '0') + '_LeaderboardPosition');
    }

    return sc_GetPropertyFromLeaderboardPosition(leaderboardPosition, propertyName);
}

function sc_GetPlayerLeaderboardPosition() {
    let leaderboardPosition = $prop('PostItNoteRacing.Player_LeaderboardPosition');

    if (leaderboardPosition === undefined) {
        return null;
    } else {
        return leaderboardPosition;
    }
}

function sc_GetPropertyFromLeaderboardPosition(leaderboardPosition, propertyName) {
    let propertyValue = $prop('PostItNoteRacing.Drivers_' + (leaderboardPosition ?? '00').toString().padStart(2, '0') + '_' + propertyName);

    if (propertyValue === undefined) {
        return null;
    } else {
        return propertyValue;
    }
}