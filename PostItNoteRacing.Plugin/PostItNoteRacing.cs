using GameReaderCommon;
using PostItNoteRacing.Plugin.Models;
using PostItNoteRacing.Plugin.Views;
using SimHub;
using SimHub.Plugins;
using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace PostItNoteRacing.Plugin
{
    [PluginAuthor("Derrick Moeller")]
    [PluginDescription("Properties for iRacing")]
    [PluginName("PostItNoteRacing")]
    public class PostItNoteRacing : IDataPlugin, IDisposable, IWPFSettingsV2
    {
        private short _counter;
        private Session _session;

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                _session?.Dispose();
            }
        }

        #region Interface: IDataPlugin
        /// <summary>
        /// Instance of the current plugin manager
        /// </summary>
        public PluginManager PluginManager { get; set; }

        /// <summary>
        /// Called one time per game data update, contains all normalized game data,
        /// raw data are intentionnally "hidden" under a generic object type (A plugin SHOULD NOT USE IT)
        ///
        /// This method is on the critical path, it must execute as fast as possible and avoid throwing any error
        ///
        /// </summary>
        /// <param name="pluginManager"></param>
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

                    if (_counter % 4 == 0) // 0, 4, 8, 12, 16...
                    {
                        _session.GetGameData();
                    }

                    if (_counter % 60 == 0) // 0
                    {
                        _session.CalculateLivePositions();
                    }

                    if (_counter % 2 == 1) // 1, 3, 5, 7, 9...
                    {
                        _session.WriteSimHubData();
                    }

                    if (_counter % 4 == 2) // 2, 6, 10, 14, 18...
                    {
                        _session.CalculateGaps();
                    }

                    if (_counter % 60 == 30 && data.GameName == "IRacing") // 30
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
        /// Called at plugin manager stop, close/dispose anything needed here !
        /// Plugins are rebuilt at game change
        /// </summary>
        /// <param name="pluginManager"></param>
        public void End(PluginManager _)
        {
            Logging.Current.Info($"Stopping plugin : {nameof(PostItNoteRacing)}");

            _session?.Dispose();
        }

        /// <summary>
        /// Called once after plugins startup
        /// Plugins are rebuilt at game change
        /// </summary>
        /// <param name="pluginManager"></param>
        public void Init(PluginManager pluginManager)
        {
            Logging.Current.Info($"Starting plugin : {nameof(PostItNoteRacing)}");

            _session = new Session(pluginManager, typeof(PostItNoteRacing));
        }
        #endregion

        #region Interface: IDispose
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
        /// Returns the settings control, return null if no settings control is required
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <returns></returns>
        public Control GetWPFSettingsControl(PluginManager _)
        {
            return new SettingsView(this);
        }
        #endregion
    }
}