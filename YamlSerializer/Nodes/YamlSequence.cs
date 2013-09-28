using System;
using System.Collections.Generic;
using System.Linq;

namespace YamlSerializer
{
    /// <summary>
    /// Represents a sequence node in a YAML document.
    /// Use <see cref="IList&lt;YamlNode&gt;">IList&lt;YamlNode&gt;</see> interface 
    /// to manipulate child nodes.
    /// </summary>
    public class YamlSequence: YamlComplexNode, IList<YamlNode>, IDisposable
    {
        /// <summary>
        /// Create a sequence node that has <paramref name="nodes"/> as its child.
        /// </summary>
        /// <param name="nodes">Child nodes of the sequence.</param>
        public YamlSequence(params YamlNode[] nodes)
        {
            Tag = DefaultTagPrefix + "seq";
            for ( int i = 0; i < nodes.Length; i++ )
                Add(nodes[i]);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Clear();
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
        protected override int GetHashCodeCoreSub(int path, Dictionary<YamlNode, int> dict)
        {
            if ( dict.ContainsKey(this) )
                return dict[this].GetHashCode() * 27 + path;
            dict.Add(this, path);

            // Unless !!seq, the hash code is based on the node's identity.
            if ( ShorthandTag() != "!!seq" )
                return TypeUtils.GetHashCode(this);

            var result = Tag.GetHashCode();
            for ( int i=0; i<Count; i++) {
                var item= sequence[i];
                if ( item is YamlComplexNode ) {
                    result += GetHashCodeCoreSub(path * 317 ^ i.GetHashCode(), dict);
                } else {
                    result += item.GetHashCode() ^ i.GetHashCode();
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

            // Unless !!seq, the hash equality is evaluated by the node's identity.
            if ( ShorthandTag() != "!!seq" )
                return false;

            var aa = this;
            var bb = (YamlSequence)b;
            if ( aa.Count != bb.Count )
                return false;

            var iter_a = aa.GetEnumerator();
            var iter_b = bb.GetEnumerator();
            while ( iter_a.MoveNext() && iter_b.MoveNext() )
                if ( !iter_a.Current.Equals(iter_b.Current, repository) )
                    return false;
            return true;
        }
        
        void OnItemAdded(YamlNode item)
        {
            item.Changed += ItemChanged;
        }
        void OnItemRemoved(YamlNode item)
        {
            item.Changed -= ItemChanged;
        }
        void ItemChanged(object sender, EventArgs e)
        {
            OnChanged();
        }
        
        internal override string ToString(ref int length)
        {
            var t = ( ShorthandTag() == "!!seq" ? "" : ShorthandTag() + " " );
            length -= t.Length + 2;
            if ( length < 0 )
                return "[" + t + "...";
            var s = "";
            foreach ( var item in this ) {
                if ( item != this.First() ) {
                    s += ", ";
                    length -= 2;
                }
                s += item.ToString(ref length);
                if ( length < 0 )
                    return "[" + t + s;
            }
            return "[" + t + s + "]";
        }

        #region IList<Node> members
        List<YamlNode> sequence = new List<YamlNode>();
        /// <summary>
        /// Determines the index of a specific child node in the <see cref="YamlSequence"/>.
        /// </summary>
        /// <remarks>
        /// If an node appears multiple times in the sequence, the IndexOf method always returns the first instance found.
        /// </remarks>
        /// <param name="item">The child node to locate in the <see cref="YamlSequence"/>.</param>
        /// <returns>The index of <paramref name="item"/> if found in the sequence; otherwise, -1.</returns>
        public int IndexOf(YamlNode item)
        {
            return sequence.IndexOf(item);
        }
        /// <summary>
        /// Inserts an item to the <see cref="YamlSequence"/> at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The node to insert into the <see cref="YamlSequence"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the 
        /// <see cref="YamlSequence"/>.</exception>
        /// <remarks>
        /// <para>If <paramref name="index"/> equals the number of items in the <see cref="YamlSequence"/>, 
        /// then <paramref name="item"/> is appended to the sequence.</para>
        /// <para>The nodes that follow the insertion point move down to accommodate the new node.</para>
        /// </remarks>
        public void Insert(int index, YamlNode item)
        {
            sequence.Insert(index, item);
            OnItemAdded(item);
        }
        /// <summary>
        /// Removes the <see cref="YamlSequence"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the node to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="YamlSequence"/>.</exception>
        /// <remarks>
        /// The nodes that follow the removed node move up to occupy the vacated spot. 
        /// </remarks>
        public void RemoveAt(int index)
        {
            var item = sequence[index];
            sequence.RemoveAt(index);
            OnItemRemoved(item);
        }
        /// <summary>
        /// Gets or sets the node at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the node to get or set.</param>
        /// <returns>The node at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="YamlSequence"/>).</exception>
        /// <remarks>
        /// <para>This property provides the ability to access a specific node in the sequence by using the following syntax: mySequence[index].</para>
        /// </remarks>
        public YamlNode this[int index]
        {
            get { return sequence[index]; }
            set {
                if ( index < sequence.Count ) {
                    var item = sequence[index];
                    sequence[index] = value;
                    OnItemRemoved(item);
                } else {
                    sequence[index] = value;
                }
                OnItemAdded(value);
            }
        }
        /// <summary>
        /// Adds an item to the <see cref="YamlSequence"/>.
        /// </summary>
        /// <param name="item">The node to add to the <see cref="YamlSequence"/>.</param>
        public void Add(YamlNode item)
        {
            sequence.Add(item);
            OnItemAdded(item);
        }
        /// <summary>
        /// Removes all nodes from the <see cref="YamlSequence"/>.
        /// </summary>
        public void Clear()
        {
            var old = sequence;
            sequence = new List<YamlNode>();
            foreach ( var item in old )
                OnItemRemoved(item);
        }
        /// <summary>
        /// Determines whether a sequence contains a child node that equals to the specified <paramref name="value"/>
        /// by using the default equality comparer.
        /// </summary>
        /// <param name="value">The node value to locate in the sequence.</param>
        /// <returns>true If the sequence contains an node that has the specified value; otherwise, false.</returns>
        /// <example>
        /// <code>
        /// var seq = new YamlSequence(new YamlScalar("a"));
        /// 
        /// // different object that has same value
        /// Assert.IsTrue(seq.Contains(new YamlScalar("a")));
        /// 
        /// // different value
        /// Assert.IsFalse(s.Contains(str("b")));
        /// </code>
        /// </example>
        public bool Contains(YamlNode value)
        {
            return sequence.Contains(value);
        }
        /// <summary>
        /// Copies the child nodes of the <see cref="YamlSequence"/> to an <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="YamlSequence"/>.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> is a null reference.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
        /// <exception cref="ArgumentException">
        /// <para>array is multidimensional.</para>
        /// <para>-or-</para>
        /// <para>The number of elements in the source <see cref="YamlSequence"/> is greater than the available space from 
        /// <paramref name="arrayIndex"/> to the end of the destination array.</para>
        /// </exception>
        public void CopyTo(YamlNode[] array, int arrayIndex)
        {
            sequence.CopyTo(array, arrayIndex);
        }
        /// <summary>
        /// Gets the number of child nodes of the <see cref="YamlSequence"/>.
        /// </summary>
        /// <value>The number of child nodes of the sequence.</value>
        public int Count
        {
            get { return sequence.Count; }
        }
        bool ICollection<YamlNode>.IsReadOnly
        {
            get { return ( (ICollection<YamlNode>)sequence ).IsReadOnly; }
        }
        /// <summary>
        /// Removes the first occurrence of a specific node from the <see cref="YamlSequence"/>.
        /// </summary>
        /// <param name="node">The node to remove from the <see cref="YamlSequence"/>.</param>
        /// <returns> true if <paramref name="node"/> was successfully removed from the <see cref="YamlSequence"/>; otherwise, false. 
        /// This method also returns false if <paramref name="node"/> is not found in the original <see cref="YamlSequence"/>.</returns>
        /// 
        public bool Remove(YamlNode node)
        {
            var i = sequence.FindIndex(item => item.Equals(node));
            if ( i < 0 )
                return false;
            var item2 = sequence[i];
            sequence.RemoveAt(i);
            OnItemRemoved(item2);
            return true;
        }
        /// <summary>
        /// Returns an enumerator that iterates through the all child nodes.
        /// </summary>
        /// <returns>An enumerator that iterates through the all child nodes.</returns>
        public IEnumerator<YamlNode> GetEnumerator()
        {
            return sequence.GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ( (System.Collections.IEnumerable)sequence ).GetEnumerator();
        }
        #endregion
    }
}