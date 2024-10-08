﻿using PostItNoteRacing.Plugin.Interfaces;
using System.Runtime.CompilerServices;

namespace PostItNoteRacing.Plugin.ViewModels
{
    internal abstract class SettingsViewModel<T> : SimHubViewModel
        where T : new()
    {
        public SettingsViewModel(IModifySimHub plugin, string displayName)
            : base(plugin, displayName)
        {
            Entity = Plugin.ReadSettings(typeof(T).Name, () => new T());
        }

        protected T Entity { get; }

        protected override void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (Entity.GetType().GetProperty(propertyName) != null)
            {
                Plugin.SaveSettings(typeof(T).Name, Entity);
            }

            base.NotifyPropertyChanged(propertyName);
        }
    }
}