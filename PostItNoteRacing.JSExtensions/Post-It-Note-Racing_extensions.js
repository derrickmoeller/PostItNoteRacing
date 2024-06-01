"use strict";

function mc_GetLeaderboardPosition(classIndex, classPosition) {
    return $prop('PostItNoteRacing.Class_' + (classIndex ?? '00').toString().padStart(2, '0') + '_' + (classPosition ?? '00').toString().padStart(2, '0') + '_LeaderboardPosition');
}

function mc_GetPropertyFromClassPosition(classIndex, classPosition, propertyName) {
    let leaderboardPosition = mc_GetLeaderboardPosition(classIndex, classPosition);

    return sc_GetPropertyFromLeaderboardPosition(leaderboardPosition, propertyName);
}

function pc_GetLeaderboardPosition(classPosition) {
    let classIndex = sc_GetPropertyFromLeaderboardPosition(getplayerleaderboardposition(), 'ClassIndex');

    return mc_GetLeaderboardPosition(classIndex, classPosition);
}

function pc_GetPropertyFromClassPosition(classPosition, propertyName) {
    let leaderboardPosition = pc_GetLeaderboardPosition(classPosition);

    return sc_GetPropertyFromLeaderboardPosition(leaderboardPosition, propertyName);
}

function pc_GetPropertyFromRelativePosition(relativePosition, propertyName) {
    let positionInClass = sc_GetPropertyFromLeaderboardPosition(getplayerleaderboardposition(), 'LivePositionInClass') + relativePosition;

    return pc_GetPropertyFromClassPosition(positionInClass, propertyName);
}

function sc_GetPropertyFromAheadBehind(aheadBehind, propertyName) {
    let leaderboardPosition = getopponentleaderboardposition_aheadbehind(aheadBehind);

    return sc_GetPropertyFromLeaderboardPosition(leaderboardPosition, propertyName);
}

function sc_GetPropertyFromCarNumber(carNumber, propertyName) {
    let leaderboardPosition = $prop('PostItNoteRacing.Drivers_Car_' + (carNumber ?? '000').toString().padStart(3, '0') + '_LeaderboardPosition');

    return sc_GetPropertyFromLeaderboardPosition(leaderboardPosition, propertyName);
}

function sc_GetPropertyFromLeaderboardPosition(leaderboardPosition, propertyName) {
    return $prop('PostItNoteRacing.Drivers_' + (leaderboardPosition ?? '00').toString().padStart(2, '0') + '_' + propertyName);
}

function sc_GetPropertyFromLivePosition(livePosition, propertyName) {
    let leaderboardPosition = $prop('PostItNoteRacing.Drivers_Live_' + (livePosition ?? '00').toString().padStart(2, '0') + '_LeaderboardPosition');

    return sc_GetPropertyFromLeaderboardPosition(leaderboardPosition, propertyName);
}

function sc_GetPropertyFromRelativePosition(relativePosition, propertyName) {
    let leaderboardPosition = getplayerleaderboardposition() + relativePosition;

    return sc_GetPropertyFromLeaderboardPosition(leaderboardPosition, propertyName);
}

function sh_GetPropertyFromOverriddenFunction(leaderboardPosition, propertyName, originalFunction) {
    if ($prop('PostItNoteRacing.Settings_OverrideJavaScriptFunctions') === true) {
        let value = sc_GetPropertyFromLeaderboardPosition(leaderboardPosition, propertyName);

        if (value != null) {
            return value;
        }
    }

    return originalFunction(leaderboardPosition);
}

driverclassposition = (function (originalFunction) {
    return function (leaderboardPosition) {
        return sh_GetPropertyFromOverriddenFunction(leaderboardPosition, 'LivePositionInClass', originalFunction);
    };
})(driverclassposition);

drivergaptoclassleader = (function (originalFunction) {
    return function (leaderboardPosition) {
        return sh_GetPropertyFromOverriddenFunction(leaderboardPosition, 'GapToClassLeader', originalFunction);
    };
})(drivergaptoclassleader);

drivergaptoclassleadercombined = (function (originalFunction) {
    return function (leaderboardPosition) {
        return sh_GetPropertyFromOverriddenFunction(leaderboardPosition, 'GapToClassLeaderString', originalFunction);
    };
})(drivergaptoclassleadercombined);

drivergaptoleader = (function (originalFunction) {
    return function (leaderboardPosition) {
        return sh_GetPropertyFromOverriddenFunction(leaderboardPosition, 'GapToLeader', originalFunction);
    };
})(drivergaptoleader);

drivergaptoleadercombined = (function (originalFunction) {
    return function (leaderboardPosition) {
        return sh_GetPropertyFromOverriddenFunction(leaderboardPosition, 'GapToLeaderString', originalFunction);
    };
})(drivergaptoleadercombined);

drivergaptoplayer = (function (originalFunction) {
    return function (leaderboardPosition) {
        return sh_GetPropertyFromOverriddenFunction(leaderboardPosition, 'GapToPlayer', originalFunction);
    };
})(drivergaptoplayer);

driverpositiongain = (function (originalFunction) {
    return function (leaderboardPosition) {
        return sh_GetPropertyFromOverriddenFunction(leaderboardPosition, 'PositionsGained', originalFunction);
    };
})(driverpositiongain);

driverpositiongainclass = (function (originalFunction) {
    return function (leaderboardPosition) {
        return sh_GetPropertyFromOverriddenFunction(leaderboardPosition, 'PositionsGainedInClass', originalFunction);
    };
})(driverpositiongainclass);

driverrelativegaptoplayer = (function (originalFunction) {
    return function (leaderboardPosition) {
        return sh_GetPropertyFromOverriddenFunction(leaderboardPosition, 'RelativeGapToPlayer', originalFunction);
    };
})(driverrelativegaptoplayer);

driverrelativegaptoplayercombined = (function (originalFunction) {
    return function (leaderboardPosition) {
        return sh_GetPropertyFromOverriddenFunction(leaderboardPosition, 'GapToPlayerString', originalFunction);
    };
})(driverrelativegaptoplayercombined);

driverstartposition = (function (originalFunction) {
    return function (leaderboardPosition) {
        return sh_GetPropertyFromOverriddenFunction(leaderboardPosition, 'GridPosition', originalFunction);
    };
})(driverstartposition);

driverstartpositionclass = (function (originalFunction) {
    return function (leaderboardPosition) {
        return sh_GetPropertyFromOverriddenFunction(leaderboardPosition, 'GridPositionInClass', originalFunction);
    };
})(driverstartpositionclass);