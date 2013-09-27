using System;
using System.Globalization;

namespace YamlSerializer
{
    internal class Int16Converter : ITypeConverter
    {
        public object ConvertFromString(object context, CultureInfo culture, string value)
        {
            return short.Parse(value, culture);
        }

        public string ConvertToString(object context, CultureInfo cultureInfo, object value)
        {
            return ((short)value).ToString("G", cultureInfo);
        }
    }

    internal class UInt16Converter : ITypeConverter
    {
        public object ConvertFromString(object context, CultureInfo culture, string value)
        {
            return ushort.Parse(value, culture);
        }

        public string ConvertToString(object context, CultureInfo cultureInfo, object value)
        {
            return ((ushort)value).ToString("G", cultureInfo);
        }
    }

    internal class Int32Converter : ITypeConverter
    {
        public object ConvertFromString(object context, CultureInfo culture, string value)
        {
            return int.Parse(value, culture);
        }

        public string ConvertToString(object context, CultureInfo cultureInfo, object value)
        {
            return ((int) value).ToString("G", cultureInfo);
        }
    }

    internal class UInt32Converter : ITypeConverter
    {
        public object ConvertFromString(object context, CultureInfo culture, string value)
        {
            return uint.Parse(value, culture);
        }

        public string ConvertToString(object context, CultureInfo cultureInfo, object value)
        {
            return ((uint)value).ToString("G", cultureInfo);
        }
    }

    internal class Int64Converter : ITypeConverter
    {
        public object ConvertFromString(object context, CultureInfo culture, string value)
        {
            return long.Parse(value, culture);
        }

        public string ConvertToString(object context, CultureInfo cultureInfo, object value)
        {
            return ((long)value).ToString("G", cultureInfo);
        }
    }

    internal class UInt64Converter : ITypeConverter
    {
        public object ConvertFromString(object context, CultureInfo culture, string value)
        {
            return ulong.Parse(value, culture);
        }

        public string ConvertToString(object context, CultureInfo cultureInfo, object value)
        {
            return ((ulong)value).ToString("G", cultureInfo);
        }
    }

    internal class SingleConverter : ITypeConverter
    {
        public object ConvertFromString(object context, CultureInfo culture, string value)
        {
            return float.Parse(value, culture);
        }

        public string ConvertToString(object context, CultureInfo cultureInfo, object value)
        {
            return ((float)value).ToString("R", cultureInfo);
        }
    }

    internal class DoubleConverter : ITypeConverter
    {
        public object ConvertFromString(object context, CultureInfo culture, string value)
        {
            return double.Parse(value, culture);
        }

        public string ConvertToString(object context, CultureInfo cultureInfo, object value)
        {
            return ((double)value).ToString("R", cultureInfo);
        }
    }

    internal class StringConverter : ITypeConverter
    {
        public object ConvertFromString(object context, CultureInfo culture, string value)
        {
            return value;
        }

        public string ConvertToString(object context, CultureInfo cultureInfo, object value)
        {
            return (string)value ?? string.Empty;
        }
    }

    internal class CharConverter : ITypeConverter
    {
        public object ConvertFromString(object context, CultureInfo culture, string value)
        {
            return string.IsNullOrEmpty(value) ? char.MinValue : value[0];
        }

        public string ConvertToString(object context, CultureInfo cultureInfo, object value)
        {
            return "" + (char)value;
        }
    }

    internal class BooleanConverter : ITypeConverter
    {
        public object ConvertFromString(object context, CultureInfo culture, string value)
        {
            return bool.Parse(value);
        }

        public string ConvertToString(object context, CultureInfo cultureInfo, object value)
        {
            return ((bool)value) ? "true" : "false";
        }
    }

    internal class DecimalConverter : ITypeConverter
    {
        public object ConvertFromString(object context, CultureInfo culture, string value)
        {
            return decimal.Parse(value, culture);
        }

        public string ConvertToString(object context, CultureInfo cultureInfo, object value)
        {
            return ((decimal)value).ToString("G", cultureInfo);
        }
    }

    internal class ByteConverter : ITypeConverter
    {
        public object ConvertFromString(object context, CultureInfo culture, string value)
        {
            return byte.Parse(value, culture);
        }

        public string ConvertToString(object context, CultureInfo cultureInfo, object value)
        {
            return ((byte)value).ToString("G", cultureInfo);
        }
    }

    internal class SByteConverter : ITypeConverter
    {
        public object ConvertFromString(object context, CultureInfo culture, string value)
        {
            return sbyte.Parse(value, culture);
        }

        public string ConvertToString(object context, CultureInfo cultureInfo, object value)
        {
            return ((sbyte)value).ToString("G", cultureInfo);
        }
    }

    internal class EnumConverter : ITypeConverter
    {
        private readonly Type enumType;

        public EnumConverter(Type enumType)
        {
            this.enumType = enumType;
        }


        public object ConvertFromString(object context, CultureInfo culture, string value)
        {
            return Enum.Parse(enumType, value, true);
        }

        public string ConvertToString(object context, CultureInfo culture, object value)
        {
            return ((Enum)Enum.ToObject(enumType, value)).ToString("G");
        }
    }

    internal class EnumConverterFactory : ITypeConverterFactory
    {
        public ITypeConverter TryCreate(Type type)
        {
            return type.IsEnum ? new EnumConverter(type) : null;
        }
    }
}