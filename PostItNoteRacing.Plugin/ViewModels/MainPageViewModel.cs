using PostItNoteRacing.Common.ViewModels;
using PostItNoteRacing.Plugin.Interfaces;
using System.Collections.ObjectModel;

namespace PostItNoteRacing.Plugin.ViewModels
{
    internal class MainPageViewModel : ViewModelBase
    {
        public MainPageViewModel(IModifySimHub plugin)
        {
            Workspaces.Add(new TelemetryViewModel(plugin));
            Workspaces.Add(new UtilityViewModel(plugin));
        }

        public ObservableCollection<ViewModelBase> Workspaces { get; } = new ObservableCollection<ViewModelBase>();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var workspace in Workspaces)
                {
                    workspace.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}