using System;

namespace PostItNoteRacing.Common
{
    public abstract class DisposableObject : IDisposable
    {
        protected virtual void Dispose(bool disposing)
        {
        }

        #region Interface: IDisposable

        /// <summary>
        /// Invoked when this object is being removed from the application
        /// and will be subject to garbage collection.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
