using System;
using System.Collections.Generic;
using System.Linq;

namespace YamlSerializer
{
    /// <summary>
    /// Represents a mapping node in a YAML document. 
    /// Use <see cref="IDictionary&lt;YamlNode,YamlNode&gt;">IDictionary&lt;YamlNode,YamlNode&gt;</see> interface to
    /// manipulate child key/value pairs.
    /// </summary>
    /// <remarks>
    /// Child items can be accessed via IDictionary&lt;YamlNode, YamlNode&gt; interface.
    /// 
    /// Note that mapping object can not contain multiple keys with same value.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create a mapping.
    /// var map1 = new YamlMapping(
    ///     // (key, value) pairs should be written sequential
    ///     new YamlScalar("key1"), new YamlScalar("value1"),
    ///     "key2", "value2" // implicitely converted to YamlScalar
    ///     );
    ///     
    /// // Refer to the mapping.
    /// Assert.AreEqual( map1[new Scalar("key1")], new YamlScalar("value1") );
    /// Assert.AreEqual( map1["key1"], "value1" );
    /// 
    /// // Add an entry.
    /// map1.Add( "key3", new YamlSequence( "value3a", "value3b" ) );
    /// 
    /// // Create another mapping.
    /// var map2 = new YamlMapping(
    ///     "key1", "value1",
    ///     "key2", "value2",
    ///     "key3", new YamlSequence( "value3a", "value3b" )
    ///     );
    ///     
    /// // Mappings are equal when they have objects that are equal to each other.
    /// Assert.IsTrue( map1.Equals( map2 ) );
    /// </code>
    /// </example>
    public class YamlMapping: YamlComplexNode, IDictionary<YamlNode, YamlNode>
    {
        RehashableDictionary<YamlNode, YamlNode> mapping =
            new RehashableDictionary<YamlNode, YamlNode>();

        /// <summary>
        /// Calculates the hash code for a collection object. This function is called recursively 
        /// on the child objects with the sub cache code repository for the nodes already appeared
        /// in the node tree.
        /// </summary>
        /// <param name="path">The cache code for the path where this node was found.</param>
        /// <param name="dict">Repository of the nodes that already appeared in the node tree.
        /// Sub hash code for the nodes can be refered to from this dictionary.</param>
        /// <returns></returns>
        protected override int GetHashCodeCoreSub(int path, Dictionary<YamlNode, int> dict)
        {
            if ( dict.ContainsKey(this) )
                return dict[this].GetHashCode() * 27 + path;
            dict.Add(this, path);

            // Unless !!map, the hash code is based on the node's identity.
            if ( ShorthandTag() != "!!map" )
                return TypeUtils.HashCodeByRef<YamlMapping>.GetHashCode(this);

            var result = Tag.GetHashCode();
            foreach ( var item in this ) {
                int hash_for_key;
                if ( item.Key is YamlComplexNode ) {
                    hash_for_key = GetHashCodeCoreSub(path * 317, dict);
                } else {
                    hash_for_key = item.Key.GetHashCode();
                }
                result += hash_for_key * 971;
                if ( item.Value is YamlComplexNode ) {
                    result += GetHashCodeCoreSub(path * 317 + hash_for_key * 151, dict);
                } else {
                    result += item.Value.GetHashCode() ^ hash_for_key;
                }
            }
            return result;
        }
        
        internal override bool Equals(YamlNode b, ObjectRepository repository)
        {
            YamlNode a = this;

            bool skip;
            if ( !base.EqualsSub(b, repository, out skip) )
                return false;
            if ( skip )
                return true;

            // Unless !!map, the hash equality is evaluated by the node's identity.
            if ( ShorthandTag() != "!!map" )
                return false;

            var aa = this;
            var bb = (YamlMapping)b;
            if ( aa.Count != bb.Count )
                return false;

            var status= repository.CurrentStatus;
            foreach ( var item in this ) {
                var candidates = bb.ItemsFromHashCode(item.Key.GetHashCode());
                KeyValuePair<YamlNode, YamlNode> theone = new KeyValuePair<YamlNode,YamlNode>();
                if ( !candidates.Any(subitem => {
                                                    if ( item.Key.Equals((YamlNode) subitem.Key, repository) ) {
                                                        theone = subitem;
                                                        return true;
                                                    }
                                                    repository.CurrentStatus = status;
                                                    return false;
                }) )
                    return false;
                if(!item.Value.Equals(theone.Value, repository))
                    return false;
            }
            return true;
        }

        internal ICollection<KeyValuePair<YamlNode, YamlNode>> ItemsFromHashCode(int key_hash)
        {
            return mapping.ItemsFromHash(key_hash);
        }

        /// <summary>
        /// Create a YamlMapping that contains <paramref name="nodes"/> in it.
        /// </summary>
        /// <example>
        /// <code>
        /// // Create a mapping.
        /// var map1 = new YamlMapping(
        ///     // (key, value) pairs should be written sequential
        ///     new YamlScalar("key1"), new YamlScalar("value1"),
        ///     new YamlScalar("key2"), new YamlScalar("value2")
        ///     );
        /// </code>
        /// </example>
        /// <exception cref="ArgumentException">Even number of arguments are expected.</exception>
        /// <param name="nodes">(key, value) pairs are written sequential.</param>
        public YamlMapping(params YamlNode[] nodes)
        {
            mapping.Added += ChildAdded;
            mapping.Removed += ChildRemoved;
            if ( nodes.Length / 2 != nodes.Length / 2.0 )
                throw new ArgumentException("Even number of arguments are expected.");
            Tag = DefaultTagPrefix + "map";
            for ( int i = 0; i < nodes.Length; i += 2 )
                Add(nodes[i + 0], nodes[i + 1]);
        }

        void CheckDuplicatedKeys()
        {
            foreach ( var entry in this )
                CheckDuplicatedKeys(entry.Key);
        }

        void CheckDuplicatedKeys(YamlNode key)
        {
            foreach(var k in mapping.ItemsFromHash(key.GetHashCode()))
                if( ( k.Key != key ) && k.Key.Equals(key) )
                    throw new InvalidOperationException("Duplicated key found.");
        }

        void ChildRemoved(object sender, RehashableDictionary<YamlNode, YamlNode>.DictionaryEventArgs e)
        {
            e.Key.Changed -= KeyChanged;
            e.Value.Changed -= ChildChanged;
            OnChanged();
            CheckDuplicatedKeys();
        }

        void ChildAdded(object sender, RehashableDictionary<YamlNode, YamlNode>.DictionaryEventArgs e)
        {
            e.Key.Changed += KeyChanged;
            e.Value.Changed += ChildChanged;
            OnChanged();
            CheckDuplicatedKeys();
        }

        void KeyChanged(object sender, EventArgs e)
        {
            ChildChanged(sender, e);
            CheckDuplicatedKeys((YamlNode)sender);
        }

        void ChildChanged(object sender, EventArgs e)
        {
            OnChanged();
        }

        internal override void OnLoaded()
        {
            base.OnLoaded();
            ProcessMergeKey();
        }
        void ProcessMergeKey()
        {
            // find merge key
            var merge_key = Keys.FirstOrDefault(key => key.Tag == YamlNode.ExpandTag("!!merge"));
            if ( merge_key == null )
                return;

            // merge the value
            var value = this[merge_key];
            if ( value is YamlMapping ) {
                Remove(merge_key);
                Merge((YamlMapping)value);
            } else
                if ( value is YamlSequence ) {
                    Remove(merge_key);
                    foreach ( var item in (YamlSequence)value )
                        if ( item is YamlMapping )
                            Merge((YamlMapping)item);
                } else {
                    // ** ignore
                    // throw new InvalidOperationException(
                    //     "Can't merge the value into a mapping: " + value.ToString());
                }
        }
        void Merge(YamlMapping map)
        {
            foreach ( var entry in map ) 
                if ( !ContainsKey(entry.Key) )
                    Add(entry.Key, entry.Value);
        }

        /// <summary>
        /// Enumerate child nodes.
        /// </summary>
        /// <returns>Inumerator that iterates child nodes</returns>
        internal override string ToString(ref int length)
        {
            var s = "";
            var t = ( ShorthandTag() == "!!map" ? "" : ShorthandTag() + " " );
            length -= t.Length + 2;
            if ( length < 0 )
                return "{" + t + "...";
            foreach ( var entry in this ) {
                if ( s != "" ) {
                    s += ", ";
                    length -= 2;
                }
                s += entry.Key.ToString(ref length);
                if ( length < 0 )
                    return "{" + t + s;
                s += ": ";
                length -= 2;
                s += entry.Value.ToString(ref length);
                if ( length < 0 )
                    return "{" + t + s;
            }
            return "{" + t + s + "}";
        }

        #region IDictionary<Node,Node> members

        /// <summary>
        /// Adds an element with the provided key and value.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="value"/> is a null reference.</exception>
        /// <exception cref="ArgumentException">An element with the same key already exists.</exception>
        /// <param name="key">The node to use as the key of the element to add.</param>
        /// <param name="value">The node to use as the value of the element to add.</param>
        public void Add(YamlNode key, YamlNode value)
        {
            if ( key == null || value == null )
                throw new ArgumentNullException("Key and value must be a valid YamlNode.");
            mapping.Add(key, value);
        }

        /// <summary>
        /// Determines whether the <see cref="YamlMapping"/> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="YamlMapping"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is a null reference </exception>
        /// <returns> true if the <see cref="YamlMapping"/> contains an element with the key that is equal to the specified value; otherwise, false.</returns>
        public bool ContainsKey(YamlNode key)
        {
            return mapping.ContainsKey(key);
        }
        /// <summary>
        /// Gets an ICollection&lt;YamlNode&gt; containing the keys of the <see cref="YamlMapping"/>.
        /// </summary>
        public ICollection<YamlNode> Keys
        {
            get { return mapping.Keys; }
        }
        /// <summary>
        /// Removes the element with the specified key from the <see cref="YamlMapping"/>.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns> true if the element is successfully removed; otherwise, false. This method also returns false if key was not found in the original <see cref="YamlMapping"/>.</returns>
        public bool Remove(YamlNode key)
        {
            return mapping.Remove(key);
        }
        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; 
        /// otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns> true if the object that implements <see cref="YamlMapping"/> contains an element with the specified key; otherwise, false.</returns>
        public bool TryGetValue(YamlNode key, out YamlNode value)
        {
            return mapping.TryGetValue(key, out value);
        }
        /// <summary>
        /// Gets an ICollection&lt;YamlNode&gt; containing the values of the <see cref="YamlMapping"/>.
        /// </summary>
        public ICollection<YamlNode> Values
        {
            get { return mapping.Values; }
        }
        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <param name="key">The key of the element to get or set.</param>
        /// <returns>The element with the specified key.</returns>
        /// <exception cref="ArgumentNullException">key is a null reference</exception>
        /// <exception cref="KeyNotFoundException">The property is retrieved and key is not found.</exception>
        public YamlNode this[YamlNode key]
        {
            get { return mapping[key]; }
            set { mapping[key] = value; }
        }
        #region ICollection<KeyValuePair<Node,Node>> members
        void ICollection<KeyValuePair<YamlNode, YamlNode>>.Add(KeyValuePair<YamlNode, YamlNode> item)
        {
            ( (ICollection<KeyValuePair<YamlNode, YamlNode>>)mapping ).Add(item);
        }
        /// <summary>
        /// Removes all entries from the <see cref="YamlMapping"/>.
        /// </summary>
        public void Clear()
        {
            mapping.Clear();
        }
        /// <summary>
        /// Determines whether the <see cref="YamlMapping"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="YamlMapping"/>.</param>
        /// <returns>true if item is found in the <see cref="YamlMapping"/> otherwise, false.</returns>
        public bool Contains(KeyValuePair<YamlNode, YamlNode> item)
        {
            return ( (ICollection<KeyValuePair<YamlNode, YamlNode>>)mapping ).Contains(item);
        }
        void ICollection<KeyValuePair<YamlNode, YamlNode>>.CopyTo(KeyValuePair<YamlNode, YamlNode>[] array, int arrayIndex)
        {
            ( (ICollection<KeyValuePair<YamlNode, YamlNode>>)mapping ).CopyTo(array, arrayIndex);
        }
        /// <summary>
        /// Returns the number of entries in a <see cref="YamlMapping"/>.
        /// </summary>
        public int Count
        {
            get { return mapping.Count; }
        }
        bool ICollection<KeyValuePair<YamlNode, YamlNode>>.IsReadOnly
        {
            get { return false; }
        }
        bool ICollection<KeyValuePair<YamlNode, YamlNode>>.Remove(KeyValuePair<YamlNode, YamlNode> item)
        {
            return ( (ICollection<KeyValuePair<YamlNode, YamlNode>>)mapping ).Remove(item);
        }
        #endregion
        #region IEnumerable<KeyValuePair<Node,Node>> members
        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="YamlMapping"/>.
        /// </summary>
        /// <returns>An enumerator that iterates through the <see cref="YamlMapping"/>.</returns>
        public IEnumerator<KeyValuePair<YamlNode, YamlNode>> GetEnumerator()
        {
            return mapping.GetEnumerator();
        }
        #endregion
        #region IEnumerable members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return mapping.GetEnumerator();
        }
        #endregion
        #endregion
    }
}