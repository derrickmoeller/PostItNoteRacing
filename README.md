The PostItNoteRacing plugin is a SimHub plugin that provides additional properties that are easily accessed by Ahead/Behind/Class/Relative/Overall position.

**Examples**

- return sc_GetPropertyFromAheadBehind(1, 'RelativeGapToPlayerString') // Retrieve relative gap to player for next car behind on track.
- return mc_GetPropertyFromClassPosition(7, 'DeltaToBest') // Retrieve delta to best for the 7th place car in players class.
- return mc_GetPropertyFromRelativePosition(-1, 'IRatingLicenseCombinedString') // Retrieve irating license combined string for player one position ahead in class.
- return sc_GetPropertyFromLivePosition(1, 'LastLapTime') // Retrieve last lap time of overall leader. 
