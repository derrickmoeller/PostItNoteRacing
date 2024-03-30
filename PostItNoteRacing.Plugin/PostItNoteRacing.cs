using GameReaderCommon;
using SimHub;
using SimHub.Plugins;
using System;

namespace PostItNoteRacing.Plugin
{
    [PluginAuthor("Derrick Moeller")]
    [PluginDescription("Properties for iRacing")]
    [PluginName("PostItNoteRacing")]
    public class PostItNoteRacing : IDataPlugin, IDisposable
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
        public PluginManager PluginManager { get; set; }

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

                    if (_counter % 6 == 0) // 0, 6, 12, 18, 24, 30, 36, 42, 48, 54
                    {
                        _session.GetGameData();
                    }

                    if (_counter % 60 == 0) // 0
                    {
                        _session.CalculateLivePositions();
                    }

                    if (_counter % 6 == 1) // 1, 7, 13, 19, 25, 31, 37, 43, 49, 55
                    {
                        _session.CalculateGaps();
                    }

                    if (_counter % 6 == 2) // 2, 8, 14, 20, 26, 32, 38, 44, 50, 56
                    {
                        _session.WriteSimHubData();
                    }

                    if (_counter % 60 == 3 && data.GameName == "IRacing") // 3
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

        public void End(PluginManager _)
        {
            Logging.Current.Info($"Stopping plugin : {nameof(PostItNoteRacing)}");
        }

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
    }
}