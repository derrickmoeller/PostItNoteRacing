The PostItNoteRacing plugin is a SimHub plugin that provides additional properties that are easily accessed by Ahead/Behind/Class/Leaderboard/Live/Relative position.

**Multi-Class Examples**

- return mc_GetPropertyFromClassPosition(2, 4, 'LastNLapsAverage'); // Retrieve last N laps average for 4th place in the 2nd class.

**Player-Class Examples**

- return pc_GetPropertyFromClassPosition(7, 'DeltaToBest'); // Retrieve delta to best for 7th place in players class.
- return pc_GetPropertyFromRelativePosition(-1, 'IRatingLicenseCombinedString'); // Retrieve irating license combined string for 1st car ahead in players class.

**Single-Class Examples**

- return sc_GetPropertyFromAheadBehind(1, 'RelativeGapToPlayerString'); // Retrieve relative gap to player for 1st car behind on track.
- return sc_GetPropertyFromLeaderboardPosition(1, 'LastLapTime'); // Retrieve last lap time of overall leader (per SimHub leaderboard).
- return sc_GetPropertyFromLivePosition(10, 'TeamName'); // Retrieve team name of 10th place (per live position).
