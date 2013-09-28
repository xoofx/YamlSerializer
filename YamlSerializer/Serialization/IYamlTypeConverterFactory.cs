using System;

namespace YamlSerializer.Serialization
{
    /// <summary>
    /// Allows to dynamically create a <see cref="IYamlTypeConverter"/> based on a type.
    /// </summary>
    public interface IYamlTypeConverterFactory
    {
        /// <summary>
        /// Try to create a <see cref="IYamlTypeConverter"/> or return null if not supported.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="type">The type.</param>
        /// <returns>If supported, return an instance of <see cref="IYamlTypeConverter"/> else return <c>null</c>.</returns>
        IYamlTypeConverter TryCreate(SerializerContext context, Type type);
    }
}