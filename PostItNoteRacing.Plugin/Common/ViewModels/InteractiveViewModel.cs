using PostItNoteRacing.Common.Interfaces;
using System;

namespace PostItNoteRacing.Common.ViewModels
{
    /// <summary>
    /// This ViewModelBase subclass provides a dialog service to be used by the user interface.
    /// This class is abstract.
    /// </summary>
    public abstract class InteractiveViewModel(IDialogService dialogService, string displayName = null) : ViewModelBase(displayName)
    {
        /// <summary>
        /// Gets the dialog service to be used by the user interface.
        /// </summary>
        protected IDialogService DialogService { get; } = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
    }
}
