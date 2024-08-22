using GameReaderCommon;
using PostItNoteRacing.Common;
using PostItNoteRacing.Plugin.EventArgs;
using PostItNoteRacing.Plugin.Interfaces;
using PostItNoteRacing.Plugin.ViewModels;
using SimHub;
using SimHub.Plugins;
using System;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;

namespace PostItNoteRacing.Plugin
{
    [PluginAuthor("Derrick Moeller")]
    [PluginDescription("Additional Properties")]
    [PluginName("Post-It Note Racing")]
    public class PostItNoteRacing : Disposable, IDataPlugin, IModifySimHub, IWPFSettingsV2
    {
        private MainPageViewModel _mainPage;

        private EventHandler<NotifyDataUpdatedEventArgs> _dataUpdated;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _mainPage?.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Interface: IDataPlugin

        /// <summary>
        /// Gets or sets instance of the current plugin manager.
        /// </summary>
        public PluginManager PluginManager { get; set; }

        /// <summary>
        /// Called one time per game data update, contains all normalized game data,
        /// raw data are intentionnally "hidden" under a generic object type (A plugin SHOULD NOT USE IT)
        ///
        /// This method is on the critical path, it must execute as fast as possible and avoid throwing any error.
        /// </summary>
        /// <param name="_">Discarded parameter.</param>
        /// <param name="data">Current game data, including current and previous data frame.</param>
        public void DataUpdate(PluginManager _, ref GameData data)
        {
            try
            {
                _dataUpdated?.Invoke(this, new NotifyDataUpdatedEventArgs(data));
            }
            catch (Exception ex)
            {
                Logging.Current.Info($"Exception in plugin ({nameof(PostItNoteRacing)}) : {ex}");
            }
        }

        /// <summary>
        /// Called at plugin manager stop, close/dispose anything needed here!
        /// Plugins are rebuilt at game change.
        /// </summary>
        /// <param name="_">Discarded parameter.</param>
        public void End(PluginManager _)
        {
            _mainPage?.Dispose();
        }

        /// <summary>
        /// Called once after plugins startup
        /// Plugins are rebuilt at game change.
        /// </summary>
        /// <param name="pluginManager">Plugin manager.</param>
        public void Init(PluginManager pluginManager)
        {
            _mainPage ??= new MainPageViewModel(this);

            (this as IModifySimHub)?.AddProperty("Version", Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }
        #endregion

        #region Interface: IModifySimHub
        event EventHandler<NotifyDataUpdatedEventArgs> IModifySimHub.DataUpdated
        {
            add
            {
                _dataUpdated += value;
            }

            remove
            {
                _dataUpdated -= value;
            }
        }

        void IModifySimHub.AddAction(string actionName, Action<PluginManager, string> action) => PluginManager.AddAction<PostItNoteRacing>(actionName, action);

        void IModifySimHub.AddProperty(string propertyName, dynamic defaultValue) => PluginManager.AddProperty(propertyName, typeof(PostItNoteRacing), defaultValue);

        void IModifySimHub.AttachDelegate<T>(string propertyName, Func<T> valueProvider) => PluginManager.AttachDelegate(propertyName, typeof(PostItNoteRacing), valueProvider);

        void IModifySimHub.DetachDelegate(string propertyName) => PluginManager.DetachDelegate(propertyName, typeof(PostItNoteRacing));

        dynamic IModifySimHub.GetProperty(string propertyName) => PluginManager.GetPropertyValue<PostItNoteRacing>(propertyName);

        T IModifySimHub.ReadSettings<T>(string settingsName, Func<T> valueProvider) => this.ReadCommonSettings(settingsName, valueProvider);

        void IModifySimHub.SaveSettings<T>(string settingsName, T settings) => this.SaveCommonSettings(settingsName, settings);

        void IModifySimHub.SetProperty(string propertyName, dynamic value) => PluginManager.SetPropertyValue<PostItNoteRacing>(propertyName, value);
        #endregion

        #region Interface: IWPFSettingsV2

        /// <summary>
        /// Gets a short plugin title to show in left menu. Return null if you want to use the title as defined in PluginName attribute.
        /// </summary>
        public string LeftMenuTitle => "Post-It Note Racing";

        /// <summary>
        /// Gets the left menu icon. Icon must be 24x24 and compatible with black and white display.
        /// </summary>
        public ImageSource PictureIcon => this.ToIcon(Properties.Resources.MenuIcon);

        /// <summary>
        /// Returns the settings control, return null if no settings control is required.
        /// </summary>
        /// <param name="_">Discarded parameter.</param>
        /// <returns>Settings control.</returns>
        public Control GetWPFSettingsControl(PluginManager _)
        {
            return new MainPage
            {
                DataContext = _mainPage,
            };
        }
        #endregion
    }
}