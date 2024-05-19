"use strict";

driverclassposition = (function (original) {
    return function (raceposition) {
        let positionInClass = sc_GetPropertyFromLeaderboardPosition(raceposition, 'LivePositionInClass');

        if (positionInClass != null) {
            return positionInClass;
        }

        return original(raceposition);
    };
})(driverclassposition);

drivergaptoclassleader = (function (original) {
    return function (raceposition) {
        let gapToClassLeader = sc_GetPropertyFromLeaderboardPosition(raceposition, 'GapToClassLeader');

        if (gapToClassLeader != null) {
            return gapToClassLeader;
        }

        return original(raceposition);
    };
})(drivergaptoclassleader);

drivergaptoclassleadercombined = (function (original) {
    return function (raceposition) {
        let gapToClassLeader = sc_GetPropertyFromLeaderboardPosition(raceposition, 'GapToClassLeaderString');

        if (gapToClassLeader != null) {
            return gapToClassLeader;
        }

        return original(raceposition);
    };
})(drivergaptoclassleadercombined);

drivergaptoleader = (function (original) {
    return function (raceposition) {
        let gapToClassLeader = sc_GetPropertyFromLeaderboardPosition(raceposition, 'GapToLeader');

        if (gapToClassLeader != null) {
            return gapToClassLeader;
        }

        return original(raceposition);
    };
})(drivergaptoleader);

drivergaptoleadercombined = (function (original) {
    return function (raceposition) {
        let gapToClassLeader = sc_GetPropertyFromLeaderboardPosition(raceposition, 'GapToLeaderString');

        if (gapToClassLeader != null) {
            return gapToClassLeader;
        }

        return original(raceposition);
    };
})(drivergaptoleadercombined);

drivergaptoplayer = (function (original) {
    return function (raceposition) {
        let gapToClassLeader = sc_GetPropertyFromLeaderboardPosition(raceposition, 'GapToPlayer');

        if (gapToClassLeader != null) {
            return gapToClassLeader;
        }

        return original(raceposition);
    };
})(drivergaptoplayer);

driverpositiongain = (function (original) {
    return function (raceposition) {
        let gapToClassLeader = sc_GetPropertyFromLeaderboardPosition(raceposition, 'PositionsGained');

        if (gapToClassLeader != null) {
            return gapToClassLeader;
        }

        return original(raceposition);
    };
})(driverpositiongain);

driverpositiongainclass = (function (original) {
    return function (raceposition) {
        let gapToClassLeader = sc_GetPropertyFromLeaderboardPosition(raceposition, 'PositionsGainedInClass');

        if (gapToClassLeader != null) {
            return gapToClassLeader;
        }

        return original(raceposition);
    };
})(driverpositiongainclass);

driverrelativegaptoplayer = (function (original) {
    return function (raceposition) {
        let gapToClassLeader = sc_GetPropertyFromLeaderboardPosition(raceposition, 'RelativeGapToPlayer');

        if (gapToClassLeader != null) {
            return gapToClassLeader;
        }

        return original(raceposition);
    };
})(driverrelativegaptoplayer);

driverrelativegaptoplayercombined = (function (original) {
    return function (raceposition) {
        let gapToClassLeader = sc_GetPropertyFromLeaderboardPosition(raceposition, 'GapToPlayerString');

        if (gapToClassLeader != null) {
            return gapToClassLeader;
        }

        return original(raceposition);
    };
})(driverrelativegaptoplayercombined);

driverstartposition = (function (original) {
    return function (raceposition) {
        let gapToClassLeader = sc_GetPropertyFromLeaderboardPosition(raceposition, 'GridPosition');

        if (gapToClassLeader != null) {
            return gapToClassLeader;
        }

        return original(raceposition);
    };
})(driverstartposition);

driverstartpositionclass = (function (original) {
    return function (raceposition) {
        let gapToClassLeader = sc_GetPropertyFromLeaderboardPosition(raceposition, 'GridPositionInClass');

        if (gapToClassLeader != null) {
            return gapToClassLeader;
        }

        return original(raceposition);
    };
})(driverstartpositionclass);