using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;
using System.Globalization;

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
    ///   string s = TypeConverterRegistry.ConvertToString(obj);
    /// 
    ///   // Convert the string to an object of the spific type.
    ///   object restored = TypeConverterRegistry.ConvertFromString(s, type);
    ///   
    ///   Assert.AreEqual(obj, restored);
    /// 
    /// }
    /// </code>
    /// </example>
    internal class TypeConverterRegistry
    {
        internal CultureInfo Culture;
        private static Dictionary<Type, ITypeConverter> TypeConverters = new Dictionary<Type, ITypeConverter>();
        private static List<ITypeConverterFactory> TypeConverterFactories = new List<ITypeConverterFactory>();

        public TypeConverterRegistry()
        {
            Culture = System.Globalization.CultureInfo.InvariantCulture;

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

            Register(new EnumConverterFactory());
        }


        public static bool IsTypeConverterSpecified(Type type)
        {
            if (!TypeConverters.ContainsKey(type))
            {
                return FindConverter(type, false) != null;
            }
            return true;
        }

        private static ITypeConverter FindConverter(Type type, bool exceptionIfNotFound)
        {
            if ( TypeConverters.ContainsKey(type) ) 
            {
                return TypeConverters[type];
            }

            var tagConverterAttribute = type.GetAttribute<YamlTypeConverterAttribute>();
            if (tagConverterAttribute != null)
            {
                var converterType = Type.GetType(tagConverterAttribute.ConverterTypeName);
                var converter = Activator.CreateInstance(converterType) as ITypeConverter;
                if (converter != null)
                {
                    Register(type, converter);
                    return converter;
                }
            }

            // Try to resolve it via a factory
            foreach (var typeConverterFactory in TypeConverterFactories)
            {
                var converter = typeConverterFactory.TryCreate(type);
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

        public static void Register(Type type, ITypeConverter converter)
        {
            TypeConverters[type] = converter;
        }

        public static void Register(ITypeConverterFactory factory)
        {
            TypeConverterFactories.Add(factory);
        }

        public string ConvertToString(object obj)
        {
            if ( obj == null )
                return "null";

            var converter = FindConverter(obj.GetType(), true);
            return converter != null ? converter.ConvertToString(null, Culture, obj) : obj.ToString();
        }

        public object ConvertFromString(string s, Type type)
        {
            return FindConverter(type, true).ConvertFromString(null, Culture, s);
        }
    }
}
