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

        public string ConvertTo(CultureInfo cultureInfo, object value)
        {
            return ((short)value).ToString("G", cultureInfo);
        }
    }

    internal class UInt16Converter : IYamlTypeConverter
    {
        public object ConvertFrom(CultureInfo culture, string value)
        {
            return ushort.Parse(value, culture);
        }

        public string ConvertTo(CultureInfo cultureInfo, object value)
        {
            return ((ushort)value).ToString("G", cultureInfo);
        }
    }

    internal class Int32Converter : IYamlTypeConverter
    {
        public object ConvertFrom(CultureInfo culture, string value)
        {
            return int.Parse(value, culture);
        }

        public string ConvertTo(CultureInfo cultureInfo, object value)
        {
            return ((int) value).ToString("G", cultureInfo);
        }
    }

    internal class UInt32Converter : IYamlTypeConverter
    {
        public object ConvertFrom(CultureInfo culture, string value)
        {
            return uint.Parse(value, culture);
        }

        public string ConvertTo(CultureInfo cultureInfo, object value)
        {
            return ((uint)value).ToString("G", cultureInfo);
        }
    }

    internal class Int64Converter : IYamlTypeConverter
    {
        public object ConvertFrom(CultureInfo culture, string value)
        {
            return long.Parse(value, culture);
        }

        public string ConvertTo(CultureInfo cultureInfo, object value)
        {
            return ((long)value).ToString("G", cultureInfo);
        }
    }

    internal class UInt64Converter : IYamlTypeConverter
    {
        public object ConvertFrom(CultureInfo culture, string value)
        {
            return ulong.Parse(value, culture);
        }

        public string ConvertTo(CultureInfo cultureInfo, object value)
        {
            return ((ulong)value).ToString("G", cultureInfo);
        }
    }

    internal class SingleConverter : IYamlTypeConverter
    {
        public object ConvertFrom(CultureInfo culture, string value)
        {
            return float.Parse(value, culture);
        }

        public string ConvertTo(CultureInfo cultureInfo, object value)
        {
            return ((float)value).ToString("R", cultureInfo);
        }
    }

    internal class DoubleConverter : IYamlTypeConverter
    {
        public object ConvertFrom(CultureInfo culture, string value)
        {
            return double.Parse(value, culture);
        }

        public string ConvertTo(CultureInfo cultureInfo, object value)
        {
            return ((double)value).ToString("R", cultureInfo);
        }
    }

    internal class StringConverter : IYamlTypeConverter
    {
        public object ConvertFrom(CultureInfo culture, string value)
        {
            return value;
        }

        public string ConvertTo(CultureInfo cultureInfo, object value)
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

        public string ConvertTo(CultureInfo cultureInfo, object value)
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

        public string ConvertTo(CultureInfo cultureInfo, object value)
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

        public string ConvertTo(CultureInfo cultureInfo, object value)
        {
            return ((decimal)value).ToString("G", cultureInfo);
        }
    }

    internal class ByteConverter : IYamlTypeConverter
    {
        public object ConvertFrom(CultureInfo culture, string value)
        {
            return byte.Parse(value, culture);
        }

        public string ConvertTo(CultureInfo cultureInfo, object value)
        {
            return ((byte)value).ToString("G", cultureInfo);
        }
    }

    internal class SByteConverter : IYamlTypeConverter
    {
        public object ConvertFrom(CultureInfo culture, string value)
        {
            return sbyte.Parse(value, culture);
        }

        public string ConvertTo(CultureInfo cultureInfo, object value)
        {
            return ((sbyte)value).ToString("G", cultureInfo);
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
}