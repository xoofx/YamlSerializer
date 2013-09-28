using System;
using System.Collections.Generic;

namespace YamlSerializer.Serialization
{
    public class SerializerContext
    {
        private readonly YamlConfig config;
        private Dictionary<object, object> properties;

        public SerializerContext(YamlConfig config)
        {
            if (config == null) throw new ArgumentNullException("config");
            this.config = config;
        }

        public YamlConfig Config
        {
            get { return config; }
        }

        public Type ResolveType(string typeName)
        {
            return TypeUtils.GetType(Config.LookupAssemblies, typeName);
        }

        public object GetProperty(object key)
        {
            if (key == null) throw new ArgumentNullException("key");

            if (properties == null)
            {
                return null;
            }

            object value;
            properties.TryGetValue(key, out value);
            return value;
        }

        public void SetProperty(object key, object value)
        {
            if (key == null) throw new ArgumentNullException("key");

            if (properties == null)
            {
              properties = new Dictionary<object, object>();
            }

            properties[key] = value;
        }
    }
}