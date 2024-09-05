using PostItNoteRacing.Common.Interfaces;
using System.Windows;

namespace PostItNoteRacing.Common
{
    public class DialogService : IDialogService
    {
        public void Show(string messageText)
        {
            MessageBox.Show(messageText);
        }
    }
}
