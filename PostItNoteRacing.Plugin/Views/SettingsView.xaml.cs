using System.Windows.Controls;

namespace PostItNoteRacing.Plugin.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml.
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        public SettingsView(PostItNoteRacing plugin)
            : this()
        {
            Plugin = plugin;
        }

        public PostItNoteRacing Plugin { get; }
    }
}
