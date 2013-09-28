using System;
using System.Text.RegularExpressions;
using YamlSerializer.Serialization;

namespace YamlSerializer
{
    /// <summary>
    /// Represents a scalar node in a YAML document.
    /// </summary>
    /// <example>
    /// <code>
    /// var string_node = new YamlNode("abc");
    /// Assert.AreEqual("!!str", string_node.ShorthandTag());
    /// 
    /// var int_node1= new YamlNode(YamlNode.DefaultTagPrefix + "int", "1");
    /// Assert.AreEqual("!!int", int_node1.ShorthandTag());
    /// 
    /// // shorthand tag style can be specified
    /// var int_node2= new YamlNode("!!int", "1");
    /// Assert.AreEqual(YamlNode.DefaultTagPrefix + "int", int_node1.Tag);
    /// Assert.AreEqual("!!int", int_node1.ShorthandTag());
    /// 
    /// // or use implicit conversion
    /// YamlNode int_node3 = 1;
    /// 
    /// // YamlNodes Equals to another node when their values are equal.
    /// Assert.AreEqual(int_node1, int_node2);
    /// 
    /// // Of course, they are different if compaired by references.
    /// Assert.IsTrue(int_node1 != int_node2);
    /// </code>
    /// </example>
    public class YamlScalar: YamlNode
    {
        /// <summary>
        /// String expression of the node value.
        /// </summary>
        public string Value
        {
            get { return value; }
            set { this.value = value; OnChanged(); }
        }
        string value;

        #region constructors
        /// <summary>
        /// Create empty string node.
        /// </summary>
        public YamlScalar() { Tag = ExpandTag("!!str"); Value = ""; }
        /// <summary>
        /// Initialize string node that has <paramref name="value"/> as its content.
        /// </summary>
        /// <param name="value">Value of the node.</param>
        public YamlScalar(string value) { Tag = ExpandTag("!!str"); Value = value; }
        /// <summary>
        /// Create a scalar node with arbitral tag.
        /// </summary>
        /// <param name="tag">Tag to the node.</param>
        /// <param name="value">Value of the node.</param>
        public YamlScalar(string tag, string value) { Tag = ExpandTag(tag); Value = value; }
        /// <summary>
        /// Initialize an integer node that has <paramref name="value"/> as its content.
        /// </summary>
        public YamlScalar(int value)
        {
            Tag = ExpandTag("!!int");
            Value = YamlNode.DefaultConfig.TypeConverter.ConvertToString(value);
        }
        /// <summary>
        /// Initialize a float node that has <paramref name="value"/> as its content.
        /// </summary>
        public YamlScalar(double value)
        {
            Tag = ExpandTag("!!float");
            Value = YamlNode.DefaultConfig.TypeConverter.ConvertToString(value);
        }
        /// <summary>
        /// Initialize a bool node that has <paramref name="value"/> as its content.
        /// </summary>
        public YamlScalar(bool value)
        {
            Tag = ExpandTag("!!bool");
            Value = YamlNode.DefaultConfig.TypeConverter.ConvertToString(value);
        }
        /// <summary>
        /// Initialize a timestamp node that has <paramref name="value"/> as its content.
        /// </summary>
        public YamlScalar(DateTime value)
        {
            YamlScalar node = value;
            Tag = node.Tag;
            Value = node.Value;
        } 

        /// <summary>
        /// Implicit conversion from string to <see cref="YamlScalar"/>.
        /// </summary>
        /// <param name="value">Value to be converted.</param>
        /// <returns>Conversion result.</returns>
        public static implicit operator YamlScalar(string value)
        {
            return new YamlScalar(value);
        }
        /// <summary>
        /// Implicit conversion from string to <see cref="YamlScalar"/>.
        /// </summary>
        /// <param name="value">Value to be converted.</param>
        /// <returns>Conversion result.</returns>
        public static implicit operator YamlScalar(int value)
        {
            return new YamlScalar(value);
        }
        /// <summary>
        /// Implicit conversion from string to <see cref="YamlScalar"/>.
        /// </summary>
        /// <param name="value">Value to be converted.</param>
        /// <returns>Conversion result.</returns>
        public static implicit operator YamlScalar(double value)
        {
            return new YamlScalar(value);
        }
        /// <summary>
        /// Implicit conversion from string to <see cref="YamlScalar"/>.
        /// </summary>
        /// <param name="value">Value to be converted.</param>
        /// <returns>Conversion result.</returns>
        public static implicit operator YamlScalar(bool value)
        {
            return new YamlScalar(value);
        }
        /// <summary>
        /// Implicit conversion from <see cref="DateTime"/> to <see cref="YamlScalar"/>.
        /// </summary>
        /// <param name="value">Value to be converted.</param>
        /// <returns>Conversion result.</returns>
        public static implicit operator YamlScalar(DateTime value)
        {
            YamlScalar node;
            DefaultConfig.TagResolver.Encode(value, out node);
            return node;
        }
        #endregion

        /// <summary>
        /// Call this function when the content of the node is changed.
        /// </summary>
        protected override void OnChanged()
        {
            base.OnChanged();
            UpdateNativeObject();
        }
        
        void UpdateNativeObject()
        {
            object value;
            if ( NativeObjectAvailable = DefaultConfig.TagResolver.Decode(this, out value) ) {
                NativeObject = value;
            } else {
                if ( ( ShorthandTag() == "!!float" ) && ( Value != null ) && new Regex(@"0|[1-9][0-9]*").IsMatch(Value) ) {
                    NativeObject = Convert.ToDouble(Value);
                    NativeObjectAvailable = true;
                }
            }
        }
        /// <summary>
        /// <para>When the node has YAML's standard scalar type, the native object corresponding to
        /// it can be got from this property. To see if this property contains a valid data,
        /// refer to <see cref="NativeObjectAvailable"/>.</para>
        /// </summary>
        /// <exception cref="InvalidOperationException">This property is not available. See <see cref="NativeObjectAvailable"/>.</exception>
        /// <remarks>
        /// <para>This property is available when <see cref="YamlNode.DefaultConfig"/>.<see cref="YamlConfig.TagResolver"/> contains
        /// an entry for the nodes tag and defines how to decode the <see cref="Value"/> property into native objects.</para>
        /// <para>When this property is available, equality of the scalar node is evaluated by comparing the <see cref="NativeObject"/>
        /// properties by the language default equality operator.</para>
        /// </remarks>
        [YamlSerialize(YamlSerializeMethod.Never)]
        public object NativeObject {
            get
            {
                if ( !NativeObjectAvailable )
                    throw new InvalidOperationException("NativeObject is not available.");
                return nativeObject;
            }
            private set
            {
                nativeObject = value;
            } 
        }
        object nativeObject;
        /// <summary>
        /// Gets if <see cref="NativeObject"/> contains a valid content.
        /// </summary>
        public bool NativeObjectAvailable { get; private set; }

        internal override bool Equals(YamlNode b, ObjectRepository repository)
        {
            bool skip;
            if(! base.EqualsSub(b, repository, out skip) )
                return false;
            if(skip)
                return true;
            YamlScalar aa = this;
            YamlScalar bb = (YamlScalar)b;
            if ( NativeObjectAvailable ) {
                return bb.NativeObjectAvailable && 
                       (aa.NativeObject == null ? 
                            bb.NativeObject==null :
                            aa.NativeObject.Equals(bb.NativeObject) );
            } else {
                if ( ShorthandTag() == "!!str" ) {
                    return aa.Value == bb.Value;
                } else {
                    // Node with non standard tag is compared by its identity.
                    return false; 
                }
            }
        }
        /// <summary>
        /// Returns the hash code. 
        /// The returned value will be cached until <see cref="YamlNode.OnChanged"/> is called.
        /// </summary>
        /// <returns>Hash code</returns>
        protected override int GetHashCodeCore()
        {
            if ( NativeObjectAvailable ) {
                if ( NativeObject == null ) {
                    return 0;
                } else {
                    return NativeObject.GetHashCode();
                }
            } else {
                if ( ShorthandTag() == "!!str" ) {
                    return ( Value.GetHashCode() * 193 ) ^ Tag.GetHashCode();
                } else {
                    return TypeUtils.GetHashCode(this);
                }
            }
        }

        internal override string ToString(ref int length)
        {
            var tag= ShorthandTag() == "!!str" ? "" : ShorthandTag() + " ";
            length -= tag.Length + 1;
            if ( length <= 0 )
                return tag + "\"" + "...";
            if ( Value.Length > length )
                return tag + "\"" + Value.Substring(0, length) + "...";
            length -= Value.Length + 1;
            return tag + "\"" + Value + "\"";
        }
    }
}