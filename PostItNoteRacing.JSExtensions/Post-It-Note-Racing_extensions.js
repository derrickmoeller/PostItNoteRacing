"use strict";

function mc_GetDriverPropertyFromClassPosition(classIndex, classPosition, propertyName) {
    let teamIndex = $prop('PostItNoteRacing.Class_' + (classIndex ?? '00').toString().padStart(2, '0') + '_LivePosition_' + (classPosition ?? '00').toString().padStart(2, '0') + '_Team');
    let driverIndex = sc_GetTeamPropertyFromIndex(teamIndex, 'ActiveDriver');

    return sc_GetDriverPropertyFromIndex(driverIndex, propertyName);
}

function mc_GetTeamPropertyFromClassPosition(classIndex, classPosition, propertyName) {
    let teamIndex = $prop('PostItNoteRacing.Class_' + (classIndex ?? '00').toString().padStart(2, '0') + '_LivePosition_' + (classPosition ?? '00').toString().padStart(2, '0') + '_Team');

    return sc_GetTeamPropertyFromIndex(teamIndex, propertyName);
}

function pc_GetClassProperty(propertyName) {
    let classIndex = sc_GetTeamPropertyFromLeaderboardPosition(getplayerleaderboardposition(), 'Class');

    return sc_GetClassPropertyFromIndex(classIndex, propertyName);
}

function pc_GetDriverPropertyFromClassPosition(classPosition, propertyName) {
    let classIndex = sc_GetTeamPropertyFromLeaderboardPosition(getplayerleaderboardposition(), 'Class');

    return mc_GetDriverPropertyFromClassPosition(classIndex, classPosition, propertyName);
}

function pc_GetDriverPropertyFromRelativePosition(relativePosition, propertyName) {
    let classPosition = sc_GetTeamPropertyFromLeaderboardPosition(getplayerleaderboardposition(), 'LivePositionInClass') + relativePosition;

    return pc_GetDriverPropertyFromClassPosition(classPosition, propertyName);
}

function pc_GetTeamPropertyFromClassPosition(classPosition, propertyName) {
    let classIndex = sc_GetTeamPropertyFromLeaderboardPosition(getplayerleaderboardposition(), 'Class');
    
    return mc_GetTeamPropertyFromClassPosition(classIndex, classPosition, propertyName);
}

function pc_GetTeamPropertyFromRelativePosition(relativePosition, propertyName) {
    let classPosition = sc_GetTeamPropertyFromLeaderboardPosition(getplayerleaderboardposition(), 'LivePositionInClass') + relativePosition;

    return pc_GetTeamPropertyFromClassPosition(classPosition, propertyName);
}

function sc_GetClassPropertyFromAheadBehind(aheadBehind, propertyName) {
    let leaderboardPosition = getopponentleaderboardposition_aheadbehind(aheadBehind);

    return sc_GetClassPropertyFromLeaderboardPosition(leaderboardPosition, propertyName);
}

function sc_GetClassPropertyFromIndex(classIndex, propertyName) {
    return $prop('PostItNoteRacing.Class_' + (classIndex ?? '00').toString().padStart(2, '0') + '_' + propertyName);
}

function sc_GetClassPropertyFromLeaderboardPosition(leaderboardPosition, propertyName) {
    let teamIndex = $prop('PostItNoteRacing.LeaderboardPosition_' + (leaderboardPosition ?? '00').toString().padStart(2, '0') + '_Team');
    let classIndex = sc_GetTeamPropertyFromIndex(teamIndex, 'Class');

    return sc_GetClassPropertyFromIndex(classIndex, propertyName);
}

function sc_GetClassPropertyFromLivePosition(livePosition, propertyName) {
    let teamIndex = $prop('PostItNoteRacing.LivePosition_' + (livePosition ?? '00').toString().padStart(2, '0') + '_Team');
    let classIndex = sc_GetTeamPropertyFromIndex(teamIndex, 'Class');

    return sc_GetClassPropertyFromIndex(classIndex, propertyName);
}

function sc_GetClassPropertyFromRelativePosition(relativePosition, propertyName) {
    let position = sc_GetTeamPropertyFromLeaderboardPosition(getplayerleaderboardposition(), 'LivePosition') + relativePosition;

    return sc_GetClassPropertyFromLivePosition(position, propertyName);
}

function sc_GetDriverPropertyFromAheadBehind(aheadBehind, propertyName) {
    let leaderboardPosition = getopponentleaderboardposition_aheadbehind(aheadBehind);

    return sc_GetDriverPropertyFromLeaderboardPosition(leaderboardPosition, propertyName);
}

function sc_GetDriverPropertyFromIndex(driverIndex, propertyName) {
    return $prop('PostItNoteRacing.Driver_' + (driverIndex ?? '000').toString().padStart(3, '0') + '_' + propertyName);
}

function sc_GetDriverPropertyFromLeaderboardPosition(leaderboardPosition, propertyName) {
    let teamIndex = $prop('PostItNoteRacing.LeaderboardPosition_' + (leaderboardPosition ?? '00').toString().padStart(2, '0') + '_Team');
    let driverIndex = sc_GetTeamPropertyFromIndex(teamIndex, 'ActiveDriver');

    return sc_GetDriverPropertyFromIndex(driverIndex, propertyName);
}

function sc_GetDriverPropertyFromLivePosition(livePosition, propertyName) {
    let teamIndex = $prop('PostItNoteRacing.LivePosition_' + (livePosition ?? '00').toString().padStart(2, '0') + '_Team');
    let driverIndex = sc_GetTeamPropertyFromIndex(teamIndex, 'ActiveDriver');

    return sc_GetDriverPropertyFromIndex(driverIndex, propertyName);
}

function sc_GetDriverPropertyFromRelativePosition(relativePosition, propertyName) {
    let position = sc_GetTeamPropertyFromLeaderboardPosition(getplayerleaderboardposition(), 'LivePosition') + relativePosition;

    return sc_GetDriverPropertyFromLivePosition(position, propertyName);
}

function sc_GetPlayerProperty(propertyName) {
    return $prop('PostItNoteRacing.Player_' + propertyName);
}

function sc_GetTeamPropertyFromAheadBehind(aheadBehind, propertyName) {
    let leaderboardPosition = getopponentleaderboardposition_aheadbehind(aheadBehind);

    return sc_GetTeamPropertyFromLeaderboardPosition(leaderboardPosition, propertyName);
}

function sc_GetTeamPropertyFromIndex(teamIndex, propertyName) {
    return $prop('PostItNoteRacing.Team_' + (teamIndex ?? '00').toString().padStart(2, '0') + '_' + propertyName);
}

function sc_GetTeamPropertyFromLeaderboardPosition(leaderboardPosition, propertyName) {
    let teamIndex = $prop('PostItNoteRacing.LeaderboardPosition_' + (leaderboardPosition ?? '00').toString().padStart(2, '0') + '_Team');

    return sc_GetTeamPropertyFromIndex(teamIndex, propertyName);
}

function sc_GetTeamPropertyFromLivePosition(livePosition, propertyName) {
    let teamIndex = $prop('PostItNoteRacing.LivePosition_' + (livePosition ?? '00').toString().padStart(2, '0') + '_Team');

    return sc_GetTeamPropertyFromIndex(teamIndex, propertyName);
}

function sc_GetTeamPropertyFromRelativePosition(relativePosition, propertyName) {
    let position = sc_GetTeamPropertyFromLeaderboardPosition(getplayerleaderboardposition(), 'LivePosition') + relativePosition;
    
    return sc_GetTeamPropertyFromLivePosition(position, propertyName);
}

function sh_GetPropertyFromOverriddenFunction(leaderboardPosition, propertyName, originalFunction) {
    if ($prop('PostItNoteRacing.Game_IsSupported') != false && $prop('PostItNoteRacing.Settings_OverrideJavaScriptFunctions') === true) {
        let value = sc_GetTeamPropertyFromLeaderboardPosition(leaderboardPosition, propertyName);

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
            let value = pc_GetTeamPropertyFromClassPosition(classPosition, 'LeaderboardPosition');

            if (value != null) {
                return value;
            }
        }

        return originalFunction(classPosition);
    };
})(getopponentleaderboardposition_playerclassonly);