using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace PostItNoteRacing.Common.ViewModels
{
    /// <summary>
    /// Base class for all ViewModel classes.
    /// It provides support for property change notifications and has a DisplayName property.
    /// This class is abstract.
    /// </summary>
    public abstract class ViewModelBase(string displayName = null) : Disposable, INotifyPropertyChanged
    {
        private readonly SynchronizationContext _dispatcher = SynchronizationContext.Current;

        /// <summary>
        /// Gets or sets the user-friendly name of this object.
        /// Child classes can set this property to a new value,
        /// or override it to determine the value on-demand.
        /// </summary>
        public virtual string DisplayName { get; protected set; } = displayName;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (_dispatcher == SynchronizationContext.Current)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            else
            {
                _dispatcher.Post((object state) => { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }, null);
            }
        }

        #region Interface: INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
