﻿using System;
using System.Collections.Generic;
using System.Text;

using System.Collections;
using System.Runtime.InteropServices;
using System.Reflection;

namespace YamlSerializer.Serialization
{
    /// <summary>
    /// Converts C# object to YamlNode
    /// </summary>
    /// <example>
    /// <code>
    /// object obj;
    /// YamlNode node = YamlRepresenter.ObjectToNode(obj);
    /// </code>
    /// </example>
    internal class YamlRepresenter: YamlNodeManipulator
    {
        private YamlConfig config;
        private SerializerContext context;
        Dictionary<object, YamlNode> appeared =
            new Dictionary<object, YamlNode>(TypeUtils.EqualityComparerByRef<object>.Default);

        public YamlNode ObjectToNode(object obj, SerializerContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            this.context = context;
            this.config = context.Config;

            appeared.Clear();
            if ( config.OmitTagForRootNode ) {
                return ObjectToNode(obj, obj.GetType());
            } else {
                return ObjectToNode(obj, (Type)null);
            }
        }

        private YamlNode ObjectToNode(object obj, Type expect)
        {
#if !NETCORE
            if ( obj != null && obj.GetType().IsClass && ( !(obj is string) || ((string)obj).Length >= 1000 ) )
#else
            if (obj != null && obj.GetType().GetTypeInfo().IsClass && (!(obj is string) || ((string)obj).Length >= 1000))
#endif
                if ( appeared.ContainsKey(obj) )
                    return appeared[obj];

            var node = ObjectToNodeSub(obj, expect);

            if ( node != null && expect != null && expect != typeof(Object) )
                node.Properties["expectedTag"] = TypeNameToYamlTag(expect);

            AppendToAppeared(obj, node);

            return node;
        }

        private void AppendToAppeared(object obj, YamlNode node)
        {
#if !NETCORE
            if ( obj != null && obj.GetType().IsClass && ( !( obj is string ) || ( (string)obj ).Length >= 1000 ) )
#else
            if (obj != null && obj.GetType().GetTypeInfo().IsClass && (!(obj is string) || ((string)obj).Length >= 1000))
#endif
                if ( !appeared.ContainsKey(obj) )
                    appeared.Add(obj, node);
        }

        private YamlNode ObjectToNodeSub(object obj, Type expect)
        {
            // !!null
            if ( obj == null )
                return str("!!null", "null");

            // 1) give a chance to config.TagResolver
            YamlScalar node;
            if ( config.TagResolver.Encode(obj, out node) )
                return node;

            var type = obj.GetType();

            // 2) give a chance to config.Serializable
            var serializable = config.Serializable.FindSerializable(context, obj, expect ?? type);
            if (serializable != null)
            {
                return serializable.Serialize(context, obj, type);
            }

            // 3) give a chance to specialized converter or config.TypeConverter
            if (obj is IntPtr || type.IsPointer)
                throw new ArgumentException("Pointer object '{0}' can not be serialized.".DoFormat(obj.ToString()));

            if ( obj is char ) {
                // config.TypeConverter.ConvertTo("\0") does not show "\0"
                var n = str(TypeNameToYamlTag(type), obj.ToString() );
                return n;
            }

            // bool, byte, sbyte, decimal, double, float, int ,uint, long, ulong, short, ushort, string, enum
#if !NETCORE
            if ( type.IsPrimitive || type.IsEnum || type == typeof(decimal) || type == typeof(string) )
#else
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsPrimitive || typeInfo.IsEnum || type == typeof(decimal) || type == typeof(string))
#endif
            {
                var n = str(TypeNameToYamlTag(type), config.TypeConverter.ConvertToString(context, obj) );
                return n;
            }

            // TypeConverterAttribute 
            if ( config.TypeConverter.IsTypeConverterSpecified(context, type) )
                return str(TypeNameToYamlTag(type), config.TypeConverter.ConvertToString(context, obj));

            // array
#if !NETCORE
            if ( type.IsArray ) 
#else
            if (typeInfo.IsArray)
#endif
                return CreateArrayNode((Array)obj);

            // TODO check if we can handle generics here
            if (type == typeof(Dictionary<object, object>))
            {
                var yamlMapping = map();
                DictionaryToMap(obj, yamlMapping);
                return yamlMapping;
            }

            // class / struct
#if !NETCORE
            if ( type.IsClass || type.IsValueType )
#else
            if (typeInfo.IsClass || typeInfo.IsValueType)
#endif
                return CreateMappingOrSequence(TypeNameToYamlTag(type), obj);

            throw new NotImplementedException("Type '{0}' is not supported by serialization".DoFormat(type.FullName));
        }

        private YamlNode CreateArrayNode(Array array)
        {
            Type type = array.GetType();
            return CreateArrayNodeSub(array, 0, new int[type.GetArrayRank()]);
        }

        private YamlNode CreateArrayNodeSub(Array array, int i, int[] indices)
        {
            var type= array.GetType();
            var element = type.GetElementType();
            var sequence = seq();
            if ( i == 0 ) {
                sequence.Tag = TypeNameToYamlTag(type);
                AppendToAppeared(array, sequence);
            }
#if !NETCORE
            if ( element.IsPrimitive || element.IsEnum || element == typeof(decimal) )
#else
            if (element.GetTypeInfo().IsPrimitive || element.GetTypeInfo().IsEnum || element == typeof(decimal))
#endif
                if ( array.Rank == 1 || ArrayLength(array, i+1) < 20 )
                    sequence.Properties["Compact"] = "true";
            for ( indices[i] = 0; indices[i] < array.GetLength(i); indices[i]++ )
                if ( i == array.Rank - 1 ) {
                    var n = ObjectToNode(array.GetValue(indices), type.GetElementType());
                    sequence.Add(n);
                } else {
                    var s = CreateArrayNodeSub(array, i + 1, indices);
                    sequence.Add(s);
                }
            return sequence;
        }

        private YamlNode CreateBinaryArrayNode(Array array)
        {
            var type = array.GetType();
            var element = type.GetElementType();
            if (typeof (byte) != element || array.Rank != 1 )
            {
                throw new InvalidOperationException(
                    "Can not serialize {0} as binary because it is not a byte[] array.".DoFormat(type.FullName));
            }

            var binary = (byte[]) array;
            var result= str(TypeNameToYamlTag(type), Base64Encode(type, binary));
            result.Properties["Don'tCareLineBreaks"] = "true";
            return result;
        }

        private static string Base64Encode(Type type, byte[] binary)
        {
            var s = System.Convert.ToBase64String(binary);
            var sb = new StringBuilder();
            for ( int i = 0; i < s.Length; i += 80 ) {
                if ( i + 80 < s.Length ) {
                    sb.AppendLine(s.Substring(i, 80));
                } else {
                    sb.AppendLine(s.Substring(i));
                }
            }
            return sb.ToString();
        }

        private YamlScalar MapKey(string key)
        {
            var node = (YamlScalar)key;
            node.Properties["expectedTag"] = YamlNode.ExpandTag("!!str");
            node.Properties["plainText"] = "true";
            return node;
        }

        private YamlNode CreateMappingOrSequence(string tag, object obj /*, bool by_content */ )
        {
            var type = obj.GetType();
            var accessor = ObjectMemberAccessor.FindFor(type);


            // In the case of a pure list, we are creating directly a sequence
            if (accessor.IsPureList)
            {
                // if the object is ICollection<> or IList

                var isReadOnly = accessor.IsReadOnly(obj);
                if (accessor.CollectionAdd != null && !isReadOnly)
                {
                    var iter = ((IEnumerable)obj).GetEnumerator();
                    if (iter.MoveNext())
                    { // Count > 0
                        iter.Reset();
                        return CreateSequence("!!seq", iter, accessor.ValueType);
                    }
                }
                return null;
            }

            /*
            if ( type.IsClass && !by_content && type.GetConstructor(Type.EmptyTypes) == null )
                throw new ArgumentException("Type {0} has no default constructor.".DoFormat(type.FullName));
            */

            var mapping = map();
            mapping.Tag = tag;
            AppendToAppeared(obj, mapping);

            var isList = obj is IList;

            // iterate props / fields
            foreach ( var entry in accessor ) {
                var name = entry.Key;
                var access = entry.Value;
                if (!access.ShouldSeriealize(obj) || (isList && name == "Capacity" && !config.EmitCapacityForList))
                    continue;
                if ( access.SerializeMethod == YamlSerializeMethod.Binary ) {
                    var array = CreateBinaryArrayNode((Array)access.Get(obj));
                    AppendToAppeared(access.Get(obj), array);
                    array.Properties["expectedTag"] = TypeNameToYamlTag(access.Type);
                    mapping.Add(MapKey(entry.Key), array);
                } else {
                    try {
                        var value = ObjectToNode(access.Get(obj), access.Type);
                        if (value != null && (access.SerializeMethod != YamlSerializeMethod.Content ||
                            !(value is YamlMapping) || ((YamlMapping)value).Count>0 ))
                        {
                            mapping.Add(MapKey(entry.Key), value);
                        }
                    } 
                    catch (Exception ex)
                    {
                        // TODO log this exception
                    }
                }
            }
            // if the object is IDictionary or IDictionary<,>
            if ( accessor.IsDictionary && !accessor.IsReadOnly(obj) ) 
            {
                DictionaryToMap(obj, mapping);
            } 
            else 
            {
                // if the object is ICollection<> or IList
                if ( accessor.CollectionAdd != null && !accessor.IsReadOnly(obj)) {
                    var iter = ( (IEnumerable)obj ).GetEnumerator();
                    if ( iter.MoveNext() ) { // Count > 0
                        iter.Reset();
                        mapping.Add(MapKey("~Items"), CreateSequence("!!seq", iter, accessor.ValueType));
                    }
                }
            }
            return mapping;
        }

        private void DictionaryToMap(object obj, YamlMapping mapping)
        {
            var accessor = ObjectMemberAccessor.FindFor(obj.GetType());
            var iter = ((IEnumerable)obj).GetEnumerator();

            // When this is a pure dictionary, we can directly add key,value to YamlMapping
            var dictionary = accessor.IsPureDictionary ? mapping : map();
            Func<object, object> key = null, value = null;
            while (iter.MoveNext())
            {
                if (key == null)
                {
                    var keyvalue = iter.Current.GetType();
                    var keyprop = keyvalue.GetProperty("Key");
                    var valueprop = keyvalue.GetProperty("Value");
                    key = o => keyprop.GetValue(o, new object[0]);
                    value = o => valueprop.GetValue(o, new object[0]);
                }
                dictionary.Add(
                    ObjectToNode(key(iter.Current), accessor.KeyType),
                    ObjectToNode(value(iter.Current), accessor.ValueType)
                    );
            }

            if (!accessor.IsPureDictionary && dictionary.Count > 0)
                mapping.Add(MapKey("~Items"), dictionary);
        }

        private YamlSequence CreateSequence(string tag, IEnumerator iter, Type expect)
        {
            var sequence = seq();
            sequence.Tag = tag;
#if !NETCORE
            if ( expect != null && ( expect.IsPrimitive || expect.IsEnum || expect == typeof(decimal) ) )
#else
            if (expect != null && (expect.GetTypeInfo().IsPrimitive || expect.GetTypeInfo().IsEnum || expect == typeof(decimal)))
#endif
                sequence.Properties["Compact"] = "true";

            while ( iter.MoveNext() )
                sequence.Add(ObjectToNode(iter.Current, expect));
            return sequence;
        }

        private string TypeNameToYamlTag(Type type)
        {
            if (type == typeof(int))
                return YamlNode.ExpandTag("!!int");
            if (type == typeof(string))
                return YamlNode.ExpandTag("!!str");
            if (type == typeof(Double))
                return YamlNode.ExpandTag("!!float");
            if (type == typeof(bool))
                return YamlNode.ExpandTag("!!bool");
            if (type == typeof(object[]))
                return YamlNode.ExpandTag("!!seq");

            return config.TagResolver.TagFromType(type);
        }

        private static long ArrayLength(Array array, int i)
        {
            long n = 1;
            for (; i < array.Rank; i++)
                n *= array.GetLength(i);
            return n;
        }
    }
}
