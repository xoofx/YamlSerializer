#if NET40
using System;
using System.ComponentModel;
using System.Globalization;
using YamlSerializer.Serialization;

namespace YamlSerializerTest
{
    public class LegacyTypeConverterFactory : IYamlTypeConverterFactory
    {
        public IYamlTypeConverter TryCreate(SerializerContext context, Type type)
        {

            var attrs = type.GetCustomAttributes(typeof (TypeConverterAttribute), true);
            if (attrs.Length > 0)
            {
                var converterAttr = (TypeConverterAttribute)attrs[0];

                // What is the difference between these two conditions?
                var converterType = context.ResolveType(converterAttr.ConverterTypeName);
                var typeCovnerter = Activator.CreateInstance(converterType) as TypeConverter;
                if (typeCovnerter != null)
                {
                    return new LegacyTypeConverter(typeCovnerter);
                }
            }
            return null;
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
#endif