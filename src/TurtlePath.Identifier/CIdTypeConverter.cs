namespace TurtlePath.Identifier
{
    using System.ComponentModel;

    /// <summary>
    /// Provides a type converter to convert between <see cref="CId"/> and string representations.
    /// </summary>
    public class CIdTypeConverter : TypeConverter
    {
        /// <summary>
        /// Determines whether this converter can convert an object of the given type to the type of this converter.
        /// </summary>
        /// <param name="context">An optional format context.</param>
        /// <param name="sourceType">The type you want to convert from.</param>
        /// <returns>True if conversion is possible; otherwise, false.</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        /// <summary>
        /// Converts the given value to the type of this converter.
        /// </summary>
        /// <param name="context">An optional format context.</param>
        /// <param name="culture">The culture info.</param>
        /// <param name="value">The value to convert.</param>
        /// <returns>An object that represents the converted value.</returns>
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            => value is string str ? CId.Parse(str) : base.ConvertFrom(context, culture, value);
    }
}
