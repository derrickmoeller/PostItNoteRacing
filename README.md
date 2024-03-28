The PostItNoteRacing plugin is a SimHub plugin that provides additional properties that are easily accessed by Ahead/Behind/Class/Leaderboard/Live/Relative position.

**Examples**

- return sc_GetPropertyFromAheadBehind(1, 'RelativeGapToPlayerString') // Retrieve relative gap to player for 1st car behind on track.
- return mc_GetPropertyFromClassPosition(7, 'DeltaToBest') // Retrieve delta to best for 7th place in players class.
- return sc_GetPropertyFromLeaderboardPosition(1, 'LastLapTime') // Retrieve last lap time of overall leader (per SimHub leaderboard).
- return sc_GetPropertyFromLivePosition(10, 'TeamName') // Retrieve team name of 10th place (per live position). 
- return mc_GetPropertyFromRelativePosition(-1, 'IRatingLicenseCombinedString') // Retrieve irating license combined string for 1st car ahead in class.