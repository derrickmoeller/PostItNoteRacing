using GameReaderCommon;
using PostItNoteRacing.Plugin.Interfaces;
using PostItNoteRacing.Plugin.Models;
using PostItNoteRacing.Plugin.ViewModels;
using PostItNoteRacing.Plugin.Views;
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
    [PluginName("PostItNoteRacing")]
    public class PostItNoteRacing : IDataPlugin, IDisposable, IModifySimHub, IWPFSettingsV2
    {
        private short _counter;
        private SettingsViewModel _settings;
        private Telemetry _telemetry;

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                _telemetry?.Dispose();
            }
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
                if (_telemetry != null)
                {
                    _counter++;

                    if (_counter > 59)
                    {
                        _counter = 0;
                    }

                    if (data.GameRunning && data.NewData != null)
                    {
                        _telemetry.StatusDatabase = data.NewData;

                        // 0, 4, 8, 12, 16...
                        if (_counter % 4 == 0)
                        {
                            _telemetry.GetGameData();
                        }

                        // 0
                        if (_counter % 60 == 0)
                        {
                            _telemetry.CalculateLivePositions();
                        }

                        // 0, 30
                        if (_counter % 30 == 0)
                        {
                            _telemetry.GenerateMiniSectors();
                        }

                        // 1, 3, 5, 7, 9...
                        if (_counter % 2 == 1)
                        {
                            _telemetry.WriteSimHubData();
                        }

                        // 0, 6, 12, 18, 24...
                        if (_counter % 6 == 0 && _settings.EnableGapCalculations)
                        {
                            _telemetry.CalculateGaps();
                        }

                        // 2, 8, 14, 20, 26...
                        if (_counter % 6 == 2)
                        {
                            _telemetry.CalculateDeltas();
                        }

                        // 4, 10, 16, 22, 28...
                        if (_counter % 6 == 4 && _settings.EnableEstimatedLapTimes)
                        {
                            _telemetry.CalculateEstimatedLapTimes();
                        }

                        // 30
                        if (_counter % 60 == 30 && data.GameName == "IRacing")
                        {
                            _telemetry.CalculateIRating();
                        }
                    }
                    else
                    {
                        _telemetry.Reset();
                    }
                }
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
            Logging.Current.Info($"Stopping plugin : {nameof(PostItNoteRacing)}");

            this.SaveCommonSettings("GeneralSettings", _settings.Entity);

            _telemetry?.Dispose();
        }

        /// <summary>
        /// Called once after plugins startup
        /// Plugins are rebuilt at game change.
        /// </summary>
        /// <param name="pluginManager">Plugin manager.</param>
        public void Init(PluginManager pluginManager)
        {
            Logging.Current.Info($"Starting plugin : {nameof(PostItNoteRacing)}");

            _settings = new SettingsViewModel(this.ReadCommonSettings("GeneralSettings", () => new Settings()));

            if (_settings.EnableTelemetry)
            {
                _telemetry = new Telemetry(this, _settings);
            }

            if (_settings.EnableUtility)
            {
                _ = new Utility(this, _settings.Entity);
            }

            (this as IModifySimHub)?.AddProperty("Version", Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }
        #endregion

        #region Interface: IDisposable
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        #endregion

        #region Interface: IModifySimHub
        void IModifySimHub.AddAction(string actionName, Action<PluginManager, string> action) => PluginManager.AddAction<PostItNoteRacing>(actionName, action);

        void IModifySimHub.AddProperty(string propertyName, dynamic defaultValue) => PluginManager.AddProperty(propertyName, typeof(PostItNoteRacing), defaultValue);

        void IModifySimHub.AttachDelegate<T>(string propertyName, Func<T> valueProvider) => PluginManager.AttachDelegate(propertyName, typeof(PostItNoteRacing), valueProvider);

        dynamic IModifySimHub.GetProperty(string propertyName) => PluginManager.GetPropertyValue<PostItNoteRacing>(propertyName);

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
            return new SettingsView
            {
                DataContext = _settings,
            };
        }
        #endregion
    }
}