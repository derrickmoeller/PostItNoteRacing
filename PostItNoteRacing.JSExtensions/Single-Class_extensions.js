"use strict";

function sc_GetPropertyFromAheadBehind(aheadBehind, propertyName) {
    let livePosition = null;    

    if (aheadBehind > 0) {
        livePosition = $prop('PostItNoteRacing.Behind_' + aheadBehind.toString().padStart(2, '0') + '_LivePosition');
    } else if (aheadBehind < 0) {
        livePosition = $prop('PostItNoteRacing.Ahead_' + (-aheadBehind).toString().padStart(2, '0') + '_LivePosition');
    }

    return sc_GetPropertyFromLivePosition(livePosition, propertyName);
}

function sc_GetPlayerLivePosition() {
    let livePosition = $prop('PostItNoteRacing.Player_LivePosition');

    if (livePosition === undefined) {
        return null;
    } else {
        return livePosition;
    }
}

function sc_GetPropertyFromLivePosition(livePosition, propertyName) {
    let propertyValue = $prop('PostItNoteRacing.Drivers_' + (livePosition ?? '00').toString().padStart(2, '0') + '_' + propertyName);

    if (propertyValue === undefined) {
        return null;
    } else {
        return propertyValue;
    }
}