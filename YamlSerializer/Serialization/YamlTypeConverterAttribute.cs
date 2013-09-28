using System;

namespace YamlSerializer.Serialization
{
    [AttributeUsage(AttributeTargets.All)]
    public sealed class YamlTypeConverterAttribute : Attribute
    {
        public static readonly YamlTypeConverterAttribute Default = new YamlTypeConverterAttribute();
        private string typeName;

        public string ConverterTypeName
        {
            get { return this.typeName; }
        }

        static YamlTypeConverterAttribute()
        {
        }

        public YamlTypeConverterAttribute()
        {
            this.typeName = string.Empty;
        }

        public YamlTypeConverterAttribute(Type type)
        {
            this.typeName = type.AssemblyQualifiedName;
        }

        public YamlTypeConverterAttribute(string typeName)
        {
            this.typeName = typeName.ToUpperInvariant();
        }

        public override bool Equals(object obj)
        {
            YamlTypeConverterAttribute converterAttribute = obj as YamlTypeConverterAttribute;
            if (converterAttribute != null)
                return converterAttribute.ConverterTypeName == this.typeName;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return this.typeName.GetHashCode();
        }
    }
}