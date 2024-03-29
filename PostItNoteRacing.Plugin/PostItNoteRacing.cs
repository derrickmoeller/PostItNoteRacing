using GameReaderCommon;
using SimHub;
using SimHub.Plugins;
using System;

namespace PostItNoteRacing.Plugin
{
    [PluginAuthor("Derrick Moeller")]
    [PluginDescription("Properties for iRacing")]
    [PluginName("PostItNoteRacing")]
    public class PostItNoteRacing : IDataPlugin
    {
        private short _counter;
        private Session _session;

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
                    _session.Refresh(_counter);
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
    }
}