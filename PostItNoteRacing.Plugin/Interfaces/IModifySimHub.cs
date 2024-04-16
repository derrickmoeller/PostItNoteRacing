using SimHub.Plugins;
using System;

namespace PostItNoteRacing.Plugin.Interfaces
{
    internal interface IModifySimHub
    {
        void AddAction(string actionName, Action<PluginManager, string> action);

        void AddProperty(string propertyName, dynamic defaultValue);

        dynamic GetProperty(string propertyName);

        void SetProperty(string propertyName, dynamic value);
    }
}
