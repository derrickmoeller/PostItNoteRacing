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

driverclassposition = (function (original) {
    return function (raceposition) {
        if ($prop('PostItNoteRacing.Settings_OverrideJavaScriptFunctions') === true) {
            let positionInClass = sc_GetPropertyFromLeaderboardPosition(raceposition, 'LivePositionInClass');

            if (positionInClass != null) {
                return positionInClass;
            }
        }

        return original(raceposition);
    };
})(driverclassposition);

drivergaptoclassleader = (function (original) {
    return function (raceposition) {
        if ($prop('PostItNoteRacing.Settings_OverrideJavaScriptFunctions') === true) {
            let gapToClassLeader = sc_GetPropertyFromLeaderboardPosition(raceposition, 'GapToClassLeader');

            if (gapToClassLeader != null) {
                return gapToClassLeader;
            }
        }

        return original(raceposition);
    };
})(drivergaptoclassleader);

drivergaptoclassleadercombined = (function (original) {
    return function (raceposition) {
        if ($prop('PostItNoteRacing.Settings_OverrideJavaScriptFunctions') === true) {
            let gapToClassLeader = sc_GetPropertyFromLeaderboardPosition(raceposition, 'GapToClassLeaderString');

            if (gapToClassLeader != null) {
                return gapToClassLeader;
            }
        }

        return original(raceposition);
    };
})(drivergaptoclassleadercombined);

drivergaptoleader = (function (original) {
    return function (raceposition) {
        if ($prop('PostItNoteRacing.Settings_OverrideJavaScriptFunctions') === true) {
            let gapToClassLeader = sc_GetPropertyFromLeaderboardPosition(raceposition, 'GapToLeader');

            if (gapToClassLeader != null) {
                return gapToClassLeader;
            }
        }

        return original(raceposition);
    };
})(drivergaptoleader);

drivergaptoleadercombined = (function (original) {
    return function (raceposition) {
        if ($prop('PostItNoteRacing.Settings_OverrideJavaScriptFunctions') === true) {
            let gapToClassLeader = sc_GetPropertyFromLeaderboardPosition(raceposition, 'GapToLeaderString');

            if (gapToClassLeader != null) {
                return gapToClassLeader;
            }
        }

        return original(raceposition);
    };
})(drivergaptoleadercombined);

drivergaptoplayer = (function (original) {
    return function (raceposition) {
        if ($prop('PostItNoteRacing.Settings_OverrideJavaScriptFunctions') === true) {
            let gapToClassLeader = sc_GetPropertyFromLeaderboardPosition(raceposition, 'GapToPlayer');

            if (gapToClassLeader != null) {
                return gapToClassLeader;
            }
        }

        return original(raceposition);
    };
})(drivergaptoplayer);

driverpositiongain = (function (original) {
    return function (raceposition) {
        if ($prop('PostItNoteRacing.Settings_OverrideJavaScriptFunctions') === true) {
            let gapToClassLeader = sc_GetPropertyFromLeaderboardPosition(raceposition, 'PositionsGained');

            if (gapToClassLeader != null) {
                return gapToClassLeader;
            }
        }

        return original(raceposition);
    };
})(driverpositiongain);

driverpositiongainclass = (function (original) {
    return function (raceposition) {
        if ($prop('PostItNoteRacing.Settings_OverrideJavaScriptFunctions') === true) {
            let gapToClassLeader = sc_GetPropertyFromLeaderboardPosition(raceposition, 'PositionsGainedInClass');

            if (gapToClassLeader != null) {
                return gapToClassLeader;
            }
        }

        return original(raceposition);
    };
})(driverpositiongainclass);

driverrelativegaptoplayer = (function (original) {
    return function (raceposition) {
        if ($prop('PostItNoteRacing.Settings_OverrideJavaScriptFunctions') === true) {
            let gapToClassLeader = sc_GetPropertyFromLeaderboardPosition(raceposition, 'RelativeGapToPlayer');

            if (gapToClassLeader != null) {
                return gapToClassLeader;
            }
        }

        return original(raceposition);
    };
})(driverrelativegaptoplayer);

driverrelativegaptoplayercombined = (function (original) {
    return function (raceposition) {
        if ($prop('PostItNoteRacing.Settings_OverrideJavaScriptFunctions') === true) {
            let gapToClassLeader = sc_GetPropertyFromLeaderboardPosition(raceposition, 'GapToPlayerString');

            if (gapToClassLeader != null) {
                return gapToClassLeader;
            }
        }

        return original(raceposition);
    };
})(driverrelativegaptoplayercombined);

driverstartposition = (function (original) {
    return function (raceposition) {
        if ($prop('PostItNoteRacing.Settings_OverrideJavaScriptFunctions') === true) {
            let gapToClassLeader = sc_GetPropertyFromLeaderboardPosition(raceposition, 'GridPosition');

            if (gapToClassLeader != null) {
                return gapToClassLeader;
            }
        }

        return original(raceposition);
    };
})(driverstartposition);

driverstartpositionclass = (function (original) {
    return function (raceposition) {
        if ($prop('PostItNoteRacing.Settings_OverrideJavaScriptFunctions') === true) {
            let gapToClassLeader = sc_GetPropertyFromLeaderboardPosition(raceposition, 'GridPositionInClass');

            if (gapToClassLeader != null) {
                return gapToClassLeader;
            }
        }

        return original(raceposition);
    };
})(driverstartpositionclass);