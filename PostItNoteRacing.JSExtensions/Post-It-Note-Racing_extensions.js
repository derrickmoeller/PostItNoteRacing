"use strict";

function mc_GetPropertyFromClassPosition(classIndex, classPosition, propertyName) {
    let teamIndex = $prop('PostItNoteRacing.Class_' + (classIndex ?? '00').toString().padStart(2, '0') + '_LivePosition_' + (classPosition ?? '00').toString().padStart(2, '0') + '_Team');

    return sc_GetPropertyFromTeamIndex(teamIndex, propertyName);
}

function pc_GetPropertyFromClassPosition(classPosition, propertyName) {
    let classIndex = sc_GetPropertyFromLeaderboardPosition(getplayerleaderboardposition(), 'ClassIndex');
    
    return mc_GetPropertyFromClassPosition(classIndex, classPosition, propertyName);
}

function pc_GetPropertyFromRelativePosition(relativePosition, propertyName) {
    let classPosition = sc_GetPropertyFromLeaderboardPosition(getplayerleaderboardposition(), 'LivePositionInClass') + relativePosition;

    return pc_GetPropertyFromClassPosition(classPosition, propertyName);
}

function sc_GetPropertyFromAheadBehind(aheadBehind, propertyName) {
    let leaderboardPosition = getopponentleaderboardposition_aheadbehind(aheadBehind);

    return sc_GetPropertyFromLeaderboardPosition(leaderboardPosition, propertyName);
}

function sc_GetPropertyFromLeaderboardPosition(leaderboardPosition, propertyName) {
    let teamIndex = $prop('PostItNoteRacing.LeaderboardPosition_' + (leaderboardPosition ?? '00').toString().padStart(2, '0') + '_Team');

    return sc_GetPropertyFromTeamIndex(teamIndex, propertyName);
}

function sc_GetPropertyFromLivePosition(livePosition, propertyName) {
    let teamIndex = $prop('PostItNoteRacing.LivePosition_' + (livePosition ?? '00').toString().padStart(2, '0') + '_Team');

    return sc_GetPropertyFromTeamIndex(teamIndex, propertyName);
}

function sc_GetPropertyFromRelativePosition(relativePosition, propertyName) {
    let position = sc_GetPropertyFromLeaderboardPosition(getplayerleaderboardposition(), 'LivePosition') + relativePosition;
    
    return sc_GetPropertyFromLivePosition(position, propertyName);
}

function sc_GetPropertyFromTeamIndex(teamIndex, propertyName) {
    return $prop('PostItNoteRacing.Team_' + (teamIndex ?? '00').toString().padStart(2, '0') + '_' + propertyName);
}

function sh_GetPropertyFromOverriddenFunction(leaderboardPosition, propertyName, originalFunction) {
    if ($prop('PostItNoteRacing.Game_IsSupported') != false && $prop('PostItNoteRacing.Settings_OverrideJavaScriptFunctions') === true) {
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

getopponentleaderboardposition_playerclassonly = (function (originalFunction) {
    return function (classPosition) {
        if ($prop('PostItNoteRacing.Game_IsSupported') != false && $prop('PostItNoteRacing.Settings_OverrideJavaScriptFunctions') === true) {
            let value = pc_GetPropertyFromClassPosition(classPosition, 'LeaderboardPosition');

            if (value != null) {
                return value;
            }
        }

        return originalFunction(classPosition);
    };
})(getopponentleaderboardposition_playerclassonly);