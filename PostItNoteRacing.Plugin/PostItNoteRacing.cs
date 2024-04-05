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