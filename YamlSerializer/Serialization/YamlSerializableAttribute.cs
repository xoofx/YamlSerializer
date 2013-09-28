using System;

namespace YamlSerializer.Serialization
{
    /// <summary>
    /// Attribute providing a <see cref="IYamlSerializable"/>. This class cannot be inherited.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Enum)]
    public sealed class YamlSerializableAttribute : Attribute
    {
        private readonly string typeName;

        /// <summary>
        /// Gets the name of the <see cref="IYamlSerializable"/> type
        /// </summary>
        /// <value>The name of the serializable type.</value>
        public string SerializableTypeName
        {
            get { return this.typeName; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlSerializableAttribute"/> class.
        /// </summary>
        /// <param name="type">The type must be of type <see cref="IYamlSerializable"/>.</param>
        public YamlSerializableAttribute(Type type)
        {
            this.typeName = type.AssemblyQualifiedName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlSerializableAttribute"/> class.
        /// </summary>
        /// <param name="typeName">The type must be of type <see cref="IYamlSerializable"/>.</param>
        public YamlSerializableAttribute(string typeName)
        {
            this.typeName = typeName.ToUpperInvariant();
        }

        public override bool Equals(object obj)
        {
            var converterAttribute = obj as YamlSerializableAttribute;
            if (converterAttribute != null)
                return converterAttribute.SerializableTypeName == this.typeName;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return typeName.GetHashCode();
        }
    }
}