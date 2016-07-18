using System;
using System.Reflection;

namespace YamlSerializer.Serialization
{
    class SerializableFromAttributeFactory : IYamlSerializableFactory
    {
        public IYamlSerializable TryCreate(SerializerContext context, Type type)
        {
#if !NETCORE
            var attribute = type.GetAttribute<YamlSerializableAttribute>();
#else
            var typeInfo = type.GetTypeInfo();
            var attribute = typeInfo.GetAttribute<YamlSerializableAttribute>();
#endif
            if (attribute == null)
            {
                return null;
            }

            var typeName = attribute.SerializableTypeName;
            var serializableType = context.ResolveType(typeName);

            if (serializableType == null)
            {
                throw new InvalidOperationException(string.Format("Unable to find serializable type [{0}] from current and registered assemblies", typeName));
            }

#if !NETCORE
            if (!typeof(IYamlSerializable).IsAssignableFrom(serializableType))
#else
            if (!typeof(IYamlSerializable).GetTypeInfo().IsAssignableFrom(serializableType))
#endif
            {
                throw new InvalidOperationException(string.Format("Serializable type [{0}] is not a IYamlSerializable", typeName));
            }

            return (IYamlSerializable)Activator.CreateInstance(serializableType);
        }
    }
}