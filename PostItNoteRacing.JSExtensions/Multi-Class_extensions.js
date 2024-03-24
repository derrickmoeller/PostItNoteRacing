"use strict";

function mc_GetLivePosition(classPosition) {
    let classIndex = sc_GetPropertyFromLivePosition(sc_GetPlayerLivePosition(), 'ClassIndex');
    
    let livePosition = $prop('PostItNoteRacing.Class_' + (classIndex ?? '00').toString().padStart(2, '0') + '_' + (classPosition ?? '00').toString().padStart(2, '0') + '_LivePosition');

    if (livePosition === undefined) {
        return null;
    } else {
        return livePosition;
    }
}

function mc_GetPropertyFromClassPosition(classPosition, propertyName) {
    let livePosition = mc_GetLivePosition(classPosition);

    return sc_GetPropertyFromLivePosition(livePosition, propertyName);
}

function mc_GetPropertyFromRelativePosition(relativePosition, propertyName) {
    let positionInClass = sc_GetPropertyFromLivePosition(sc_GetPlayerLivePosition(), 'LivePositionInClass');

    return mc_GetPropertyFromClassPosition(positionInClass + relativePosition, propertyName);
}