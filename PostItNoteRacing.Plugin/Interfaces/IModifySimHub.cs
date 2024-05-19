using PostItNoteRacing.Plugin.EventArgs;
using SimHub.Plugins;
using System;

namespace PostItNoteRacing.Plugin.Interfaces
{
    internal interface IModifySimHub
    {
        event EventHandler<NotifyDataUpdatedEventArgs> DataUpdated;

        void AddAction(string actionName, Action<PluginManager, string> action);

        void AddProperty(string propertyName, dynamic defaultValue);

        void AttachDelegate<T>(string propertyName, Func<T> valueProvider);

        dynamic GetProperty(string propertyName);

        T ReadSettings<T>(string settingsName, Func<T> valueProvider);

        void SaveSettings<T>(string settingsName, T settings);

        void SetProperty(string propertyName, dynamic value);
    }
}
