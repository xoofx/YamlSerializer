namespace YamlSerializer
{
    /// <summary>
    /// Implements utility functions to instantiating YamlNode's
    /// </summary>
    /// <example>
    /// <code>
    /// var node_tree = seq(
    ///     str("abc"),
    ///     str("def"),
    ///     map(
    ///         str("key"), str("value"),
    ///         str("key2"), seq( str("value2a"), str("value2b") )
    ///     ),
    ///     str("2"), // !!str
    ///     str("!!int", "2")
    /// );
    /// 
    /// string yaml = node_tree.ToYaml();
    /// 
    /// // %YAML 1.2
    /// // ---
    /// // - abc
    /// // - def
    /// // - key: value
    /// //   key2: [ value2a, value2b ]
    /// // - "2"         # !!str
    /// // - 2           # !!int
    /// // ...
    /// </code>                                                   
    /// </example>
    public class YamlNodeManipulator
    {
        /// <summary>
        /// Create a scalar node. Tag is set to be "!!str".
        /// </summary>
        /// <example>
        /// <code>
        /// var node_tree = seq(
        ///     str("abc"),
        ///     str("def"),
        ///     map(
        ///         str("key"), str("value"),
        ///         str("key2"), seq( str("value2a"), str("value2b") )
        ///     ),
        ///     str("2"), // !!str
        ///     str("!!int", "2")
        /// );
        /// 
        /// string yaml = node_tree.ToYaml();
        /// 
        /// // %YAML 1.2
        /// // ---
        /// // - abc
        /// // - def
        /// // - key: value
        /// //   key2: [ value2a, value2b ]
        /// // - "2"         # !!str
        /// // - 2           # !!int
        /// // ...
        /// </code>                                                   
        /// </example>
        /// <param name="value">Value for the scalar node.</param>
        /// <returns>Created scalar node.</returns>
        protected static YamlScalar str(string value)
        {
            return new YamlScalar(value);
        }
        /// <summary>
        /// Create a scalar node.
        /// </summary>
        /// <param name="tag">Tag for the scalar node.</param>
        /// <param name="value">Value for the scalar node.</param>
        /// <returns>Created scalar node.</returns>
        protected static YamlScalar str(string tag, string value)
        {
            return new YamlScalar(tag, value);
        }
        /// <summary>
        /// Create a sequence node. Tag is set to be "!!seq".
        /// </summary>
        /// <param name="nodes">Child nodes.</param>
        /// <returns>Created sequence node.</returns>
        protected static YamlSequence seq(params YamlNode[] nodes)
        {
            return new YamlSequence(nodes);
        }
        /// <summary>
        /// Create a sequence node. 
        /// </summary>
        /// <param name="nodes">Child nodes.</param>
        /// <param name="tag">Tag for the seuqnce.</param>
        /// <returns>Created sequence node.</returns>
        protected static YamlSequence seq_tag(string tag, params YamlNode[] nodes)
        {
            var result= new YamlSequence(nodes);
            result.Tag= tag;
            return result;
        }
        /// <summary>
        /// Create a mapping node. Tag is set to be "!!map".
        /// </summary>
        /// <param name="nodes">Sequential list of key/value pairs.</param>
        /// <returns>Created mapping node.</returns>
        protected static YamlMapping map(params YamlNode[] nodes)
        {
            return new YamlMapping(nodes);
        }
        /// <summary>
        /// Create a mapping node. 
        /// </summary>
        /// <param name="nodes">Sequential list of key/value pairs.</param>
        /// <param name="tag">Tag for the mapping.</param>
        /// <returns>Created mapping node.</returns>
        protected static YamlMapping map_tag(string tag, params YamlNode[] nodes)
        {
            var map = new YamlMapping(nodes);
            map.Tag = tag;
            return map;
        }
    }
}