﻿using PostItNoteRacing.Plugin.Models;
using System.ComponentModel;

namespace PostItNoteRacing.Plugin.Interfaces
{
    internal interface IProvideSettings
    {
        bool EnableGapCalculations { get; }

        bool EnableInverseGapStrings { get; }

        int NLaps { get; }

        ReferenceLap ReferenceLap { get; }

        int XLaps { get; }
    }
}
