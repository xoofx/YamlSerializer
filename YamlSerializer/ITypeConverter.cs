using System;
using System.Globalization;

namespace YamlSerializer
{
    public interface ITypeConverter
    {
        object ConvertFromString(object context, CultureInfo culture, string value);

        string ConvertToString(object context, CultureInfo culture, object obj);
    }


    public interface ITypeConverterFactory
    {
        ITypeConverter TryCreate(Type type);
    }

}