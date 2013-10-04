using System;

namespace YamlSerializer.Serialization
{
    /// <summary>
    /// Provides custom formatting for YAML serialization and deserialization
    /// </summary>
    public interface IYamlSerializable
    {
        /// <summary>
        /// Converts an object into its YAML representation.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="value">The value.</param>
        /// <param name="type"></param>
        /// <returns>The yaml node.</returns>
        YamlNode Serialize(SerializerContext context, object value, Type type);

        /// <summary>
        /// Generates an object from its YAML representation.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="node">The yaml node.</param>
        /// <param name="expectedType"></param>
        /// <returns>A representation of the object.</returns>
        object Deserialize(SerializerContext context, YamlNode node, Type expectedType);
    }
}