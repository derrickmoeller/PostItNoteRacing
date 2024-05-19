using System;
using System.Windows.Markup;

namespace PostItNoteRacing.Common.Extensions
{
    /// <summary>
    /// Markup extension for binding enums.
    /// </summary>
    /// <see href="https://brianlagunas.com/a-better-way-to-data-bind-enums-in-wpf/" />
    public class EnumBindingSourceExtension : MarkupExtension
    {
        private Type _enumType;

        public EnumBindingSourceExtension(Type enumType = null)
        {
            EnumType = enumType;
        }

        public Type EnumType
        {
            get => _enumType;
            set
            {
                if (_enumType != value)
                {
                    if (value != null)
                    {
                        Type enumType = Nullable.GetUnderlyingType(value) ?? value;
                        if (!enumType.IsEnum)
                        {
                            throw new ArgumentException("Type must be for an Enum.");
                        }
                    }

                    _enumType = value;
                }
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_enumType == null)
            {
                throw new InvalidOperationException("The EnumType must be specified.");
            }

            Type actualEnumType = Nullable.GetUnderlyingType(_enumType) ?? _enumType;
            Array enumValues = Enum.GetValues(actualEnumType);

            if (actualEnumType == _enumType)
            {
                return enumValues;
            }

            Array tempArray = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
            enumValues.CopyTo(tempArray, 1);
            return tempArray;
        }
    }
}
