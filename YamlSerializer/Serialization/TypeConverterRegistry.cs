using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace YamlSerializer.Serialization
{
    /// <summary>
    /// Converts various types to / from string.<br/>
    /// I don't remember why this class was needed....
    /// </summary>
    /// <example>
    /// <code>
    /// object obj = GetObjectToConvert();
    /// 
    /// // Check if the type has [TypeConverter] attribute.
    /// if( TypeConverterRegistry.IsTypeConverterSpecified(type) ) {
    /// 
    ///   // Convert the object to string.
    ///   string s = TypeConverterRegistry.ConvertTo(obj);
    /// 
    ///   // Convert the string to an object of the spific type.
    ///   object restored = TypeConverterRegistry.ConvertFrom(s, type);
    ///   
    ///   Assert.AreEqual(obj, restored);
    /// 
    /// }
    /// </code>
    /// </example>
    internal class TypeConverterRegistry
    {
        internal CultureInfo Culture;
        private readonly Dictionary<Type, IYamlTypeConverter> typeConverters = new Dictionary<Type, IYamlTypeConverter>();
        private readonly List<IYamlTypeConverterFactory> typeConverterFactories = new List<IYamlTypeConverterFactory>();

        public TypeConverterRegistry()
        {
            Culture = CultureInfo.InvariantCulture;

            // TODO pre-bake this default registers in a Dictionary in order to avoid their reallocation.
            Register(typeof(bool), new BooleanConverter());

            Register(typeof(byte), new ByteConverter());
            Register(typeof(sbyte), new SByteConverter());

            Register(typeof(short), new Int16Converter());
            Register(typeof(ushort), new UInt16Converter());

            Register(typeof(int), new Int32Converter());
            Register(typeof(uint), new UInt32Converter());

            Register(typeof(long), new Int64Converter());
            Register(typeof(ulong), new UInt64Converter());

            Register(typeof(float), new SingleConverter());
            Register(typeof(double), new DoubleConverter());

            Register(typeof(string), new StringConverter());
            Register(typeof(char), new CharConverter());

            Register(typeof(decimal), new DecimalConverter());

            Register(typeof(DateTime), new DateTimeConverter());
            Register(typeof(TimeSpan), new TimeSpanConverter());

            Register(new EnumConverterFactory());
        }

        public bool IsTypeConverterSpecified(SerializerContext context, Type type)
        {
            if (!typeConverters.ContainsKey(type))
            {
                return FindConverter(context, type, false) != null;
            }
            return true;
        }

        private IYamlTypeConverter FindConverter(SerializerContext context, Type type, bool exceptionIfNotFound)
        {
            if ( typeConverters.ContainsKey(type) ) 
            {
                return typeConverters[type];
            }

#if !NETCORE
            var tagConverterAttribute = type.GetAttribute<YamlTypeConverterAttribute>();
#else
            var tagConverterAttribute = type.GetTypeInfo().GetAttribute<YamlTypeConverterAttribute>();
#endif
            if (tagConverterAttribute != null)
            {
                var converterType = Type.GetType(tagConverterAttribute.ConverterTypeName);
                var converter = Activator.CreateInstance(converterType) as IYamlTypeConverter;
                if (converter != null)
                {
                    Register(type, converter);
                    return converter;
                }
            }

            // Try to resolve it via a factory
            foreach (var typeConverterFactory in typeConverterFactories)
            {
                var converter = typeConverterFactory.TryCreate(context, type);
                if (converter != null)
                {
                    Register(type, converter);
                    return converter;
                }
            }

            if (exceptionIfNotFound)
                throw new InvalidOperationException(string.Format("No type converter registered for type [{0}]", type.FullName));

            return null;
        }

        public void Register(Type type, IYamlTypeConverter converter)
        {
            typeConverters[type] = converter;
        }

        public void Register(IYamlTypeConverterFactory factory)
        {
            typeConverterFactories.Add(factory);
        }

        public string ConvertToString(SerializerContext context, object obj)
        {
            if ( obj == null )
                return "null";

            var converter = FindConverter(context, obj.GetType(), true);
            return converter != null ? converter.ConvertTo(Culture, obj) : obj.ToString();
        }

        public object ConvertFromString(SerializerContext context, string s, Type type)
        {
            return FindConverter(context, type, true).ConvertFrom(Culture, s);
        }
    }
}
