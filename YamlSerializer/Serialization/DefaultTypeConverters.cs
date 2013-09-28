using System;
using System.Globalization;

namespace YamlSerializer.Serialization
{
    internal class Int16Converter : IYamlTypeConverter
    {
        public object ConvertFrom(CultureInfo culture, string value)
        {
            return short.Parse(value, culture);
        }

        public string ConvertTo(CultureInfo culture, object value)
        {
            return ((short)value).ToString("G", culture);
        }
    }

    internal class UInt16Converter : IYamlTypeConverter
    {
        public object ConvertFrom(CultureInfo culture, string value)
        {
            return ushort.Parse(value, culture);
        }

        public string ConvertTo(CultureInfo culture, object value)
        {
            return ((ushort)value).ToString("G", culture);
        }
    }

    internal class Int32Converter : IYamlTypeConverter
    {
        public object ConvertFrom(CultureInfo culture, string value)
        {
            return int.Parse(value, culture);
        }

        public string ConvertTo(CultureInfo culture, object value)
        {
            return ((int) value).ToString("G", culture);
        }
    }

    internal class UInt32Converter : IYamlTypeConverter
    {
        public object ConvertFrom(CultureInfo culture, string value)
        {
            return uint.Parse(value, culture);
        }

        public string ConvertTo(CultureInfo culture, object value)
        {
            return ((uint)value).ToString("G", culture);
        }
    }

    internal class Int64Converter : IYamlTypeConverter
    {
        public object ConvertFrom(CultureInfo culture, string value)
        {
            return long.Parse(value, culture);
        }

        public string ConvertTo(CultureInfo culture, object value)
        {
            return ((long)value).ToString("G", culture);
        }
    }

    internal class UInt64Converter : IYamlTypeConverter
    {
        public object ConvertFrom(CultureInfo culture, string value)
        {
            return ulong.Parse(value, culture);
        }

        public string ConvertTo(CultureInfo culture, object value)
        {
            return ((ulong)value).ToString("G", culture);
        }
    }

    internal class SingleConverter : IYamlTypeConverter
    {
        public object ConvertFrom(CultureInfo culture, string value)
        {
            return float.Parse(value, culture);
        }

        public string ConvertTo(CultureInfo culture, object value)
        {
            return ((float)value).ToString("R", culture);
        }
    }

    internal class DoubleConverter : IYamlTypeConverter
    {
        public object ConvertFrom(CultureInfo culture, string value)
        {
            return double.Parse(value, culture);
        }

        public string ConvertTo(CultureInfo culture, object value)
        {
            return ((double)value).ToString("R", culture);
        }
    }

    internal class StringConverter : IYamlTypeConverter
    {
        public object ConvertFrom(CultureInfo culture, string value)
        {
            return value;
        }

        public string ConvertTo(CultureInfo culture, object value)
        {
            return (string)value ?? string.Empty;
        }
    }

    internal class CharConverter : IYamlTypeConverter
    {
        public object ConvertFrom(CultureInfo culture, string value)
        {
            return string.IsNullOrEmpty(value) ? char.MinValue : value[0];
        }

        public string ConvertTo(CultureInfo culture, object value)
        {
            return "" + (char)value;
        }
    }

    internal class BooleanConverter : IYamlTypeConverter
    {
        public object ConvertFrom(CultureInfo culture, string value)
        {
            return bool.Parse(value);
        }

        public string ConvertTo(CultureInfo culture, object value)
        {
            return ((bool)value) ? "true" : "false";
        }
    }

    internal class DecimalConverter : IYamlTypeConverter
    {
        public object ConvertFrom(CultureInfo culture, string value)
        {
            return decimal.Parse(value, culture);
        }

        public string ConvertTo(CultureInfo culture, object value)
        {
            return ((decimal)value).ToString("G", culture);
        }
    }

    internal class ByteConverter : IYamlTypeConverter
    {
        public object ConvertFrom(CultureInfo culture, string value)
        {
            return byte.Parse(value, culture);
        }

        public string ConvertTo(CultureInfo culture, object value)
        {
            return ((byte)value).ToString("G", culture);
        }
    }

    internal class SByteConverter : IYamlTypeConverter
    {
        public object ConvertFrom(CultureInfo culture, string value)
        {
            return sbyte.Parse(value, culture);
        }

        public string ConvertTo(CultureInfo culture, object value)
        {
            return ((sbyte)value).ToString("G", culture);
        }
    }

    internal class EnumConverter : IYamlTypeConverter
    {
        private readonly Type enumType;

        public EnumConverter(Type enumType)
        {
            this.enumType = enumType;
        }


        public object ConvertFrom(CultureInfo culture, string value)
        {
            return Enum.Parse(enumType, value, true);
        }

        public string ConvertTo(CultureInfo culture, object value)
        {
            return ((Enum)Enum.ToObject(enumType, value)).ToString("G");
        }
    }

    internal class EnumConverterFactory : IYamlTypeConverterFactory
    {
        public IYamlTypeConverter TryCreate(Type type)
        {
            return type.IsEnum ? new EnumConverter(type) : null;
        }
    }

    internal class DateTimeConverter : IYamlTypeConverter
    {
        public object ConvertFrom(CultureInfo culture, string value)
        {
            return DateTime.Parse(value, culture);        
        }

        public string ConvertTo(CultureInfo culture, object value)
        {
            return ((DateTime)value).ToString(culture);
        }
    }

    internal class TimeSpanConverter : IYamlTypeConverter
    {
        public object ConvertFrom(CultureInfo culture, string value)
        {
            return TimeSpan.Parse(value);
        }

        public string ConvertTo(CultureInfo culture, object value)
        {
            return ((TimeSpan)value).ToString();
        }
    }
}