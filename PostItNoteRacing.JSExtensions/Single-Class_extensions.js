"use strict";

function sc_GetPropertyFromAheadBehind(aheadBehind, propertyName) {
    let leaderboardPosition = getopponentleaderboardposition_aheadbehind(aheadBehind);

    return sc_GetPropertyFromLeaderboardPosition(leaderboardPosition, propertyName);
}

function sc_GetPropertyFromLeaderboardPosition(leaderboardPosition, propertyName) {
    return $prop('PostItNoteRacing.Drivers_' + (leaderboardPosition ?? '00').toString().padStart(2, '0') + '_' + propertyName);
}

function sc_GetPropertyFromLivePosition(livePosition, propertyName) {
    let leaderboardPosition = $prop('PostItNoteRacing.Drivers_Live_' + (livePosition ?? '00').toString().padStart(2, '0') + '_LeaderboardPosition');

    return sc_GetPropertyFromLeaderboardPosition(leaderboardPosition, propertyName);
}