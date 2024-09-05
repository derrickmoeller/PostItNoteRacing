using PostItNoteRacing.Common;
using PostItNoteRacing.Common.ViewModels;
using PostItNoteRacing.Plugin.Interfaces;
using System.Collections.ObjectModel;

namespace PostItNoteRacing.Plugin.ViewModels
{
    internal class MainPageViewModel : InteractiveViewModel
    {
        public MainPageViewModel(IModifySimHub plugin)
            : base(new DialogService())
        {
            Footer = new FooterViewModel(DialogService);

            Workspaces.Add(new TelemetryViewModel(plugin));
            Workspaces.Add(new UtilityViewModel(plugin));
        }

        public FooterViewModel Footer { get; }

        public ObservableCollection<ViewModelBase> Workspaces { get; } = new ObservableCollection<ViewModelBase>();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Footer?.Dispose();

                foreach (var workspace in Workspaces)
                {
                    workspace.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}