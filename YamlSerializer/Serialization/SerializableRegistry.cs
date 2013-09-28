using System;
using System.Collections.Generic;
using System.Reflection;

namespace YamlSerializer.Serialization
{
    internal class SerializableRegistry
    {
        private readonly Dictionary<Type, IYamlSerializable> typeToSerializable = new Dictionary<Type, IYamlSerializable>();
        private readonly List<Assembly> lookupAssemblies;
        private readonly List<IYamlSerializableFactory> factories = new List<IYamlSerializableFactory>();
        private static readonly SerializableFromAttributeFactory DefaultSerializableFromAttributeFactory = new SerializableFromAttributeFactory();

        public SerializableRegistry(List<Assembly> lookupAssemblies)
        {
            this.lookupAssemblies = lookupAssemblies;
            Register(DefaultSerializableFromAttributeFactory);
        }

        public IYamlSerializable FindSerializable(SerializerContext context, object value, Type type)
        {
            var serializable = value as IYamlSerializable;
            // Type may be null, so return null in this case
            if (serializable == null && type != null)
            {
                if (!typeToSerializable.TryGetValue(type, out serializable))
                {
                    foreach (var factory in factories)
                    {
                        serializable = factory.TryCreate(context, type);
                        if (serializable != null)
                        {
                            typeToSerializable.Add(type, serializable);
                            break;
                        }
                    }
                }
            }

            return serializable;
        }

        public void Register(Type type, IYamlSerializable serializable)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (serializable == null) throw new ArgumentNullException("serializable");

            typeToSerializable[type] = serializable;
        }

        public void Register(IYamlSerializableFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory");

            factories.Add(factory);
        }
    }
}