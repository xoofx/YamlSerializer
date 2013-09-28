using System;

namespace YamlSerializer.Serialization
{
    class SerializableFromAttributeFactory : IYamlSerializableFactory
    {
        public IYamlSerializable TryCreate(SerializerContext context, Type type)
        {
            var attribute = type.GetAttribute<YamlSerializableAttribute>();
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

            if (!typeof(IYamlSerializable).IsAssignableFrom(serializableType))
            {
                throw new InvalidOperationException(string.Format("Serializable type [{0}] is not a IYamlSerializable", typeName));
            }

            return (IYamlSerializable)Activator.CreateInstance(serializableType);
        }
    }
}