using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PostItNoteRacing.Common.Converters
{
    /// <summary>
    /// Represents the converter that converts Boolean values to and from System.Windows.Visibility enumeration values.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        private Visibility _visibilityWhenFalse = Visibility.Collapsed;

        /// <summary>
        /// Gets or sets the <see cref="System.Windows.Visibility" /> value to use when the value is false. Defaults to collapsed.
        /// </summary>
        public Visibility VisibilityWhenFalse
        {
            get { return _visibilityWhenFalse; }
            set { _visibilityWhenFalse = value; }
        }

        #region Interface: IValueConverter

        /// <summary>
        /// Converts a Boolean value to a System.Windows.Visibility enumeration value.
        /// </summary>
        /// <param name="value">The Boolean value to convert. This value can be a standard Boolean value or a nullable Boolean value.</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">Boolean value indicating whether to negate the return value.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>System.Windows.Visibility.Visible if value is true; otherwise, value specified by property VisibilityWhenFalse.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool.TryParse(parameter as string, out bool negateValue);

            // Negate the value using XOR
            bool val = negateValue ^ System.Convert.ToBoolean(value);
            return val ? Visibility.Visible : _visibilityWhenFalse;
        }

        /// <summary>
        /// Converts a System.Windows.Visibility enumeration value to a Boolean value.
        /// </summary>
        /// <param name="value">A System.Windows.Visibility enumeration value.</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">Boolean value indicating whether to negate the return value.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>true if value is System.Windows.Visibility.Visible; otherwise, false.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool.TryParse(parameter as string, out bool negateValue);

            if ((Visibility)value == Visibility.Visible)
            {
                return true ^ negateValue;
            }
            else
            {
                return false ^ negateValue;
            }
        }
        #endregion
    }
}