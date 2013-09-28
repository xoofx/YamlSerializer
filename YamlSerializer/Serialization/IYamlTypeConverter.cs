using System.ComponentModel;
using System.Globalization;

namespace YamlSerializer.Serialization
{
    /// <summary>
    /// Allows to serialize an object from/to a string. See remarks.
    /// </summary>
    /// <remarks>This is a replacement for legacy <see cref="TypeConverter" /> that is not provided
    /// in several .NET frameworks.</remarks>
    public interface IYamlTypeConverter
    {
        /// <summary>
        /// Converts a string to an object.
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <param name="value">The string.</param>
        /// <returns>The value representing the string.</returns>
        object ConvertFrom(CultureInfo culture, string value);

        /// <summary>
        /// Converts the automatic.
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <param name="obj">The object.</param>
        /// <returns>System.String.</returns>
        string ConvertTo(CultureInfo culture, object obj);
    }
}