using System.Collections.Generic;

namespace YamlSerializer
{
    /// <summary>
    /// Abstract base class of <see cref="YamlNode"/> that have child nodes.
    /// 
    /// <see cref="YamlMapping"/> and <see cref="YamlSequence"/> inherites from this class.
    /// </summary>
    public abstract class YamlComplexNode: YamlNode
    {
        /// <summary>
        /// Calculate hash code from <see cref="YamlNode.Tag"/> property and all child nodes.
        /// The result is cached.
        /// </summary>
        /// <returns>Hash value for the object.</returns>
        protected override int GetHashCodeCore() 
        {
            return GetHashCodeCoreSub(0,
                                      new Dictionary<YamlNode, int>(
                                          TypeUtils.EqualityComparerByRef<YamlNode>.Default));
        }

        /// <summary>
        /// Calculates the hash code for a collection object. This function is called recursively 
        /// on the child objects with the sub cache code repository for the nodes already appeared
        /// in the node tree.
        /// </summary>
        /// <param name="path">The cache code for the path where this node was found.</param>
        /// <param name="dict">Repository of the nodes that already appeared in the node tree.
        /// Sub hash code for the nodes can be refered to from this dictionary.</param>
        /// <returns></returns>
        protected abstract int GetHashCodeCoreSub(int path, Dictionary<YamlNode, int> dict);
    }
}