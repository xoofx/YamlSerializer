using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace YamlSerializer.Serialization
{
    /// <summary>
    /// Construct YAML node tree that represents a given C# object.
    /// </summary>
    internal class YamlConstructor
    {
        private SerializerContext context;
        private YamlConfig config;

        /// <summary>
        /// Construct YAML node tree that represents a given C# object.
        /// </summary>
        /// <param name="node"><see cref="YamlNode" /> to be converted to C# object.</param>
        /// <param name="context">The context.</param>
        /// <returns>System.Object.</returns>
        public object NodeToObject(YamlNode node, SerializerContext context)
        {
            return NodeToObject(node, null, context);
        }

        /// <summary>
        /// Construct YAML node tree that represents a given C# object.
        /// </summary>
        /// <param name="node"><see cref="YamlNode"/> to be converted to C# object.</param>
        /// <param name="expected">Expected type for the root object.</param>
        /// <param name="config"><see cref="YamlConfig"/> to customize serialization.</param>
        /// <returns></returns>
        public object NodeToObject(YamlNode node, Type expected, SerializerContext context)
        {
            this.context = context;
            this.config = context.Config;
            var appeared = new Dictionary<YamlNode, object>(TypeUtils.EqualityComparerByRef<YamlNode>.Default);
            return NodeToObjectInternal(node, expected, appeared);
        }

        private Type TypeFromTag(string tag)
        {
            if ( tag.StartsWith(YamlNode.DefaultTagPrefix) ) {
                switch ( tag.Substring(YamlNode.DefaultTagPrefix.Length) ) {
                case "str":
                    return typeof(string);
                case "int":
                    return typeof(Int32);
                case "null":
                    return typeof(object);
                case "bool":
                    return typeof(bool);
                case "float":
                    return typeof(double);
                case "seq":
                case "map":
                    return null;
                default:
                    throw new NotImplementedException();
                }
            } else {
                return TypeUtils.GetType(config.LookupAssemblies, tag.Substring(1));
            }
        }

        object NodeToObjectInternal(YamlNode node, Type expected, Dictionary<YamlNode, object> appeared)
        {
            if ( appeared.ContainsKey(node) )
                return appeared[node];

            object obj = null;
            
            // Type resolution
            Type type = expected == typeof(object) ? null : expected;
            Type fromTag = config.TagResolver.TypeFromTag(node.Tag);
            if ( fromTag == null )
                fromTag = TypeFromTag(node.Tag);
            if ( fromTag != null && type != fromTag && fromTag.IsClass && fromTag != typeof(string) )
                type = fromTag;
            if ( type == null )
                type = fromTag;

            // try TagResolver
            if ( type == fromTag && fromTag != null )
                if (node is YamlScalar && config.TagResolver.Decode((YamlScalar)node, out obj))
                    return obj;

            if (node.Tag == YamlNode.DefaultTagPrefix + "null")
            {
                obj = null;
            }
            else
            {
                if (node is YamlScalar)
                {
                    obj = ScalarToObject((YamlScalar) node, type);
                }
                else if (node is YamlMapping)
                {
                    obj = MappingToObject((YamlMapping) node, type, null, appeared);
                }
                else if (node is YamlSequence)
                {
                    obj = SequenceToObject((YamlSequence) node, type, appeared);
                }
                else
                    throw new NotImplementedException();
            }

            if ( !appeared.ContainsKey(node) )
                if(obj != null && obj.GetType().IsClass && ( !(obj is string) || ((string)obj).Length >= 1000 ) )
                    appeared.Add(node, obj);
            
            return obj;
        }

        object ScalarToObject(YamlScalar node, Type type)
        {
            if ( type == null )
                throw new FormatException("Could not find a type '{0}'.".DoFormat(node.Tag));

            // 1) Give a chance to deserialize through a IYamlSerializable interface
            var serializable = config.Serializable.FindSerializable(context, null, type);
            if (serializable != null)
            {
                return serializable.Deserialize(context, node);
            }

            // 2) Give a chance to IYamlTypeConverter
            var hasTypeConverter = config.TypeConverter.IsTypeConverterSpecified(type);
            var nodeValue = node.Value;

            // To accommodate the !!int and !!float encoding, all "_"s in integer and floating point values
            // are simply neglected.
            if (type == typeof (byte) || type == typeof (sbyte) || type == typeof (short) || type == typeof (ushort) ||
                type == typeof (int) || type == typeof (uint) || type == typeof (long) || type == typeof (ulong)
                || type == typeof (float) || type == typeof (decimal))
            {
                nodeValue = nodeValue.Replace("_", string.Empty);
                return config.TypeConverter.ConvertFromString(nodeValue, type);
            }

            // 変換結果が見かけ上他の型に見える可能性がある場合を優先的に変換
            // 予想通りの型が見つからなければエラーになる条件でもある
            if ( type.IsEnum || type.IsPrimitive || type == typeof(char) || type == typeof(bool) ||
                 type == typeof(string) || hasTypeConverter )
                return config.TypeConverter.ConvertFromString(nodeValue, type);

            // 3) If an array of bytes, try to deserialize it directly in base64
            if (type.IsArray)
            {
                // Split dimension from base64 strings
                var s = node.Value;
                var regex = new Regex(@" *\[([0-9 ,]+)\][\r\n]+((.+|[\r\n])+)");
                int[] dimension;
                byte[] binary;

                if (type.GetElementType() != typeof (byte) || type.GetArrayRank() != 1)
                {
                    throw new FormatException("Expecting single dimension byte[] array");
                }

                return System.Convert.FromBase64CharArray(s.ToCharArray(), 0, s.Length);
            } 

            // 4) If value is empty, try to activate the object through Activator.
            if ( node.Value == string.Empty ) {
                return config.Activator.Activate(type);
            }

            // Else throw an exception
            throw new FormatException(string.Format("Unable to deserialize yaml node [{0}]", node));
        }

        object SequenceToObject(YamlSequence seq, Type type, Dictionary<YamlNode, object> appeared)
        {
            if ( type == null )
                type = typeof(object[]);

            // 1) Give a chance to deserialize through a IYamlSerializable interface
            var serializable = config.Serializable.FindSerializable(context, null, type);
            if (serializable != null)
            {
                return serializable.Deserialize(context, seq);
            }

            if (type.IsArray)
            {
                var lengthes= new int[type.GetArrayRank()];
                GetLengthes(seq, 0, lengthes);
                var array = (Array)type.GetConstructor(lengthes.Select(l => typeof(int) /* l.GetType() */).ToArray())
                               .Invoke(lengthes.Cast<object>().ToArray());
                appeared.Add(seq, array);
                var indices = new int[type.GetArrayRank()];
                SetArrayElements(array, seq, 0, indices, type.GetElementType(), appeared);
                return array;
            } else {
                throw new NotImplementedException();
            }
        }

        void SetArrayElements(Array array, YamlSequence seq, int i, int[] indices, Type elementType, Dictionary<YamlNode, object> appeared)
        {
            if ( i < indices.Length - 1 ) {
                for ( indices[i] = 0; indices[i] < seq.Count; indices[i]++ )
                    SetArrayElements(array, (YamlSequence)seq[indices[i]], i + 1, indices, elementType, appeared);
            } else {
                for ( indices[i] = 0; indices[i] < seq.Count; indices[i]++ )
                    array.SetValue(NodeToObjectInternal(seq[indices[i]], elementType, appeared), indices);
            }
        }

        private static void GetLengthes(YamlSequence seq, int i, int[] lengthes)
        {
            lengthes[i] = Math.Max(lengthes[i], seq.Count);
            if ( i < lengthes.Length - 1 )
                for ( int j = 0; j < seq.Count; j++ )
                    GetLengthes((YamlSequence)seq[j], i + 1, lengthes);
        }

        object MappingToObject(YamlMapping map, Type type, object obj, Dictionary<YamlNode, object> appeared)
        {
            // 1) Give a chance to deserialize through a IYamlSerializable interface
            var serializable = config.Serializable.FindSerializable(context, obj, type);
            if (serializable != null)
            {
                return serializable.Deserialize(context, map);
            }
            
            // 2) Naked !!map is constructed as Dictionary<object, object>.
            if ( ( ( map.ShorthandTag() == "!!map" && type == null ) || type == typeof(Dictionary<object,object>) ) && obj == null ) {
                var dict = new Dictionary<object, object>();
                appeared.Add(map, dict);
                foreach ( var entry in map ) 
                    dict.Add(NodeToObjectInternal(entry.Key, null, appeared), NodeToObjectInternal(entry.Value, null, appeared));
                return dict;
            }

            // 3) Give a chance to config.Activator
            if ( obj == null ) 
            {
                obj = config.Activator.Activate(type);
                appeared.Add(map, obj);
            } else {
                if ( appeared.ContainsKey(map) )
                    throw new InvalidOperationException("This member is not writeable: {0}".DoFormat(obj.ToString()));
            }

            var access = ObjectMemberAccessor.FindFor(type);
            foreach(var entry in map){
                if ( obj == null )
                    throw new InvalidOperationException("Object is not initialized");
                var name = (string)NodeToObjectInternal(entry.Key, typeof(string), appeared);
                switch ( name ) {
                case "ICollection.Items":
                    if ( access.CollectionAdd == null )
                        throw new FormatException("{0} is not a collection type.".DoFormat(type.FullName));
                    access.CollectionClear(obj);                                           
                    foreach(var item in (YamlSequence)entry.Value)
                        access.CollectionAdd(obj, NodeToObjectInternal(item, access.ValueType, appeared));
                    break;
                case "IDictionary.Entries":
                    if ( !access.IsDictionary )
                        throw new FormatException("{0} is not a dictionary type.".DoFormat(type.FullName));
                    var dict = obj as IDictionary;
                    dict.Clear();
                    foreach ( var child in (YamlMapping)entry.Value )
                        dict.Add(NodeToObjectInternal(child.Key, access.KeyType, appeared), NodeToObjectInternal(child.Value, access.ValueType, appeared));
                    break;
                default:
                    if(!access.ContainsKey(name))
                        throw new FormatException("{0} does not have a member {1}.".DoFormat(type.FullName, name));
                    switch ( access[name].SerializeMethod ) {
                    case YamlSerializeMethod.Assign:
                        access[obj, name] = NodeToObjectInternal(entry.Value, access[name].Type, appeared);
                        break;
                    case YamlSerializeMethod.Content:
                        MappingToObject((YamlMapping)entry.Value, access[name].Type, access[obj, name], appeared);
                        break;
                    case YamlSerializeMethod.Binary:
                        access[obj, name] = ScalarToObject((YamlScalar)entry.Value, access[name].Type);
                        break;
                    default:
                        throw new InvalidOperationException(
                            "Member {0} of {1} is not serializable.".DoFormat(name, type.FullName));
                    }
                    break;
                }
            }
            return obj;
        }

    }
}
