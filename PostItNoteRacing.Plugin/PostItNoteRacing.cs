using GameReaderCommon;
using PostItNoteRacing.Plugin.Models;
using PostItNoteRacing.Plugin.ViewModels;
using PostItNoteRacing.Plugin.Views;
using SimHub;
using SimHub.Plugins;
using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace PostItNoteRacing.Plugin
{
    [PluginAuthor("Derrick Moeller")]
    [PluginDescription("Additional Properties")]
    [PluginName("PostItNoteRacing")]
    public class PostItNoteRacing : IDataPlugin, IDisposable, IWPFSettingsV2
    {
        private short _counter;
        private Session _session;
        private Settings _settings;

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                _session?.Dispose();
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
                _counter++;

                if (_counter > 59)
                {
                    _counter = 0;
                }

                if (data.GameRunning && data.NewData != null)
                {
                    _session.StatusDatabase = data.NewData;

                    // 0, 4, 8, 12, 16...
                    if (_counter % 4 == 0)
                    {
                        _session.GetGameData();
                    }

                    // 0
                    if (_counter % 60 == 0)
                    {
                        _session.CalculateLivePositions();
                    }

                    // 1, 3, 5, 7, 9...
                    if (_counter % 2 == 1)
                    {
                        _session.WriteSimHubData();
                    }

                    // 2, 8, 14, 20, 26...
                    if (_counter % 6 == 2)
                    {
                        _session.CalculateGaps();
                    }

                    // 4, 10, 16, 22, 28...
                    if (_counter % 6 == 4)
                    {
                        _session.CalculateEstimatedLaps();
                    }

                    // 30
                    if (_counter % 60 == 30 && data.GameName == "IRacing")
                    {
                        _session.CalculateIRating();
                    }
                }
                else
                {
                    _session.Reset();
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

            this.SaveCommonSettings("GeneralSettings", _settings);

            _session?.Dispose();
        }

        /// <summary>
        /// Called once after plugins startup
        /// Plugins are rebuilt at game change.
        /// </summary>
        /// <param name="pluginManager">Plugin manager.</param>
        public void Init(PluginManager pluginManager)
        {
            Logging.Current.Info($"Starting plugin : {nameof(PostItNoteRacing)}");

            _settings = this.ReadCommonSettings("GeneralSettings", () => new Settings());

            _session = new Session(pluginManager, typeof(PostItNoteRacing), _settings);
        }
        #endregion

        #region Interface: IDisposable
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
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
                DataContext = new SettingsViewModel(_settings),
            };
        }
        #endregion
    }
}