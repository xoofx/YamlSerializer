using System;
using System.ComponentModel;
using System.Globalization;
using YamlSerializer.Serialization;

namespace YamlSerializerTest
{
    public class LegacyTypeConverterFactory : IYamlTypeConverterFactory
    {
        public IYamlTypeConverter TryCreate(Type type)
        {
            var converter = TypeDescriptor.GetConverter(type);
            return new LegacyTypeConverter(converter);
        }

        /// <summary>
        /// Allow 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class LegacyTypeConverter : IYamlTypeConverter
        {
            private readonly TypeConverter converter;

            public LegacyTypeConverter(TypeConverter converter)
            {
                this.converter = converter;
            }

            public object ConvertFrom(CultureInfo culture, string value)
            {
                return converter.ConvertFrom(null, culture, value);
            }

            public string ConvertTo(CultureInfo culture, object obj)
            {
                return (string)converter.ConvertTo(null, culture, obj, typeof(string));
            }
        }
    }
}