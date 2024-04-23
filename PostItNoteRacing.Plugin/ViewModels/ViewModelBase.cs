using PostItNoteRacing.Plugin.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PostItNoteRacing.Plugin.ViewModels
{
    internal abstract class ViewModelBase : INotifyPropertyChanged
    {
        public ViewModelBase(IModifySimHub modifySimHub)
        {
            ModifySimHub = modifySimHub;
        }

        protected IModifySimHub ModifySimHub { get; }

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Interface: INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
