﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "Reviewed.", Scope = "member", Target = "~M:PostItNoteRacing.Common.RelayCommand`1.#ctor(System.Action{`0},System.Predicate{`0})")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "Reviewed.")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:Enumeration items should be documented", Justification = "Reviewed.")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1633:File should have header", Justification = "Reviewed.")]
[assembly: SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1516:Elements should be separated by blank line", Justification = "Property getter / setter.", Scope = "member", Target = "~P:PostItNoteRacing.Plugin.ViewModels.FooterViewModel.GitHubVersion")]
[assembly: SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "How else would we name generic and non generic (https://stackoverflow.com/a/4063061/516433).", Scope = "type", Target = "~T:PostItNoteRacing.Common.RelayCommand`1")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1309:Field names should not begin with underscore", Justification = "Reviewed.")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Conflicts with discard best practices.")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1200:Using directives should be placed correctly", Justification = "Conflicts with IDE0065.")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "Interface: IDataPlugin", Scope = "member", Target = "~P:PostItNoteRacing.Plugin.PostItNoteRacing.PluginManager")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "Interface: IModifySimHub", Scope = "member", Target = "~E:PostItNoteRacing.Plugin.PostItNoteRacing.PostItNoteRacing#Plugin#Interfaces#IModifySimHub#DataUpdated")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "Interface: INotifyBestLapChanged", Scope = "member", Target = "~E:PostItNoteRacing.Plugin.Telemetry.CarClass.BestLapChanged")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "Interface: INotifyBestLapChanged", Scope = "member", Target = "~E:PostItNoteRacing.Plugin.Telemetry.Driver.BestLapChanged")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "Interface: INotifyBestLapChanged", Scope = "member", Target = "~E:PostItNoteRacing.Plugin.Telemetry.Team.BestLapChanged")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "Interface: INotifyPropertyChanged", Scope = "member", Target = "~E:PostItNoteRacing.Common.ViewModels.ViewModelBase.PropertyChanged")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "Interface: IWPFSettingsV2", Scope = "member", Target = "~P:PostItNoteRacing.Plugin.PostItNoteRacing.LeftMenuTitle")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:Elements should be ordered by access", Justification = "Interface: IDisposable", Scope = "member", Target = "~M:PostItNoteRacing.Common.DisposableObject.Dispose")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1208:System using directives should be placed before other using directives", Justification = "Reviewed.")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this", Justification = "Reviewed.")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1124:Do not use regions", Justification = "Grouping interface implementations.")]
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:Closing parenthesis should be spaced correctly", Justification = "Conflicts with primary constructor.", Scope = "type", Target = "~T:PostItNoteRacing.Common.Converters.EnumDescriptionTypeConverter")]
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:Closing parenthesis should be spaced correctly", Justification = "Conflicts with primary constructor.", Scope = "type", Target = "~T:PostItNoteRacing.Common.FixedSizeObservableCollection`1")]
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:Closing parenthesis should be spaced correctly", Justification = "Conflicts with primary constructor.", Scope = "type", Target = "~T:PostItNoteRacing.Common.ViewModels.InteractiveViewModel")]
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:Closing parenthesis should be spaced correctly", Justification = "Conflicts with primary constructor.", Scope = "type", Target = "~T:PostItNoteRacing.Common.ViewModels.NavigableViewModel")]
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:Closing parenthesis should be spaced correctly", Justification = "Conflicts with primary constructor.", Scope = "type", Target = "~T:PostItNoteRacing.Common.ViewModels.ViewModelBase")]
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:Closing parenthesis should be spaced correctly", Justification = "Conflicts with primary constructor.", Scope = "type", Target = "~T:PostItNoteRacing.Plugin.Telemetry.CarClass")]
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:Closing parenthesis should be spaced correctly", Justification = "Conflicts with primary constructor.", Scope = "type", Target = "~T:PostItNoteRacing.Plugin.Telemetry.Entity")]
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:Closing parenthesis should be spaced correctly", Justification = "Conflicts with primary constructor.", Scope = "type", Target = "~T:PostItNoteRacing.Plugin.ViewModels.FooterViewModel")]
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:Closing parenthesis should be spaced correctly", Justification = "Conflicts with primary constructor.", Scope = "type", Target = "~T:PostItNoteRacing.Plugin.ViewModels.PropertyViewModel`1")]
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:Closing parenthesis should be spaced correctly", Justification = "Conflicts with primary constructor.", Scope = "type", Target = "~T:PostItNoteRacing.Plugin.ViewModels.SimHubViewModel")]
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1010:Opening square brackets should be spaced correctly", Justification = "Conflicts with SA1003.")]
