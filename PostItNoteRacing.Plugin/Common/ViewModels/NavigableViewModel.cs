using PostItNoteRacing.Common.Interfaces;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace PostItNoteRacing.Common.ViewModels
{
    /// <summary>
    /// This InteractiveViewModel subclass provides a goto URL command to be used by the user interface.
    /// This class is abstract.
    /// </summary>
    public abstract class NavigableViewModel(IDialogService dialogService, string displayName = null) : InteractiveViewModel(dialogService, displayName)
    {
        private ICommand _gotoUrlCommand;

        /// <summary>
        /// Gets the command that navigates to the specified URL.
        /// </summary>
        public ICommand GotoUrlCommand
        {
            get
            {
                _gotoUrlCommand ??= new RelayCommand<string>(GotoUrl, CanGotoUrl);

                return _gotoUrlCommand;
            }
        }

        protected virtual bool CanGotoUrl(string url)
        {
            return string.IsNullOrEmpty(url) == false;
        }

        protected virtual void GotoUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch (Exception ex)
            {
                DialogService.Show(ex.Message);
            }
        }
    }
}
