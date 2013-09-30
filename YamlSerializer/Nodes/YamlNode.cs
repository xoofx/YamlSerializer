using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.ComponentModel;
using YamlSerializer.Serialization;

namespace YamlSerializer
{
    /// <summary>
    /// <para>Abstract base class of YAML data nodes.</para>
    /// 
    /// <para>See <see cref="YamlScalar"/>, <see cref="YamlSequence"/> and <see cref="YamlMapping"/> 
    /// for actual data classes.</para>
    /// </summary>
    /// <remarks>
    /// <h3>YAML data model</h3>
    /// <para>See <a href="http://yaml.org/">http://yaml.org/</a> for the official definition of 
    /// Information Models of YAML.</para>
    /// 
    /// <para>YAML data structure is defined as follows. 
    /// Note that this does not represents the text syntax of YAML text 
    /// but does logical data structure.</para>
    /// 
    /// <para>
    /// yaml-stream     ::= yaml-document*<br/>
    /// yaml-document   ::= yaml-directive* yaml-node<br/>
    /// yaml-directive  ::= YAML-directive | TAG-directive | user-defined-directive<br/>
    /// yaml-node       ::= yaml-scalar | yaml-sequence | yaml-mapping<br/>
    /// yaml-scalar     ::= yaml-tag yaml-value<br/>
    /// yaml-sequence   ::= yaml-tag yaml-node*<br/>
    /// yaml-mapping    ::= yaml-tag ( yaml-node yaml-node )*<br/>
    /// yaml-tag        ::= yaml-global-tag yaml-local-tag<br/>
    /// yaml-global-tag ::= "tag:" taggingEntity ":" specific [ "#" fragment ]<br/>
    /// yaml-local-tag  ::= "!" yaml-local-tag-name<br/>
    /// </para>
    /// 
    /// <para>Namely,</para>
    /// 
    /// <para>
    /// A YAML stream consists of zero or more YAML documents.<br/>
    /// A YAML documents have zero or more YAML directives and a root YAML node.<br/>
    /// A YAML directive is either YAML-directive, TAG-directive or user-defined-directive.<br/>
    /// A YAML node is either YAML scalar, YAML sequence or YAML mapping.<br/>
    /// A YAML scalar consists of a YAML tag and a scalar value.<br/>
    /// A YAML sequence consists of a YAML tag and zero or more child YAML nodes.<br/>
    /// A YAML mapping cosists of a YAML tag and zero or more key/value pairs of YAML nodes.<br/>
    /// A YAML tag is either a YAML global tag or a YAML local tag.<br/>
    /// A YAML global tag starts with "tag:" and described in the "tag:" URI scheme defined in RFC4151.<br/>
    /// A YAML local tag starts with "!" with a YAML local tag name<br/>
    /// </para>
    /// 
    /// <code>
    /// // Construct YAML node tree
    /// YamlNode node = 
    ///     new YamlSequence(                           // !!seq node
    ///         new YamlScalar("abc"),                  // !!str node
    ///         new YamlScalar("!!int", "123"),         // !!int node
    ///         new YamlScalar("!!float", "1.23"),      // !!float node
    ///         new YamlSequence(                       // nesting !!seq node
    ///             new YamlScalar("def"),
    ///             new YamlScalar("ghi")
    ///         ),
    ///         new YamlMapping(                        // !!map node
    ///             new YamlScalar("key1"), new YamlScalar("value1"),
    ///             new YamlScalar("key2"), new YamlScalar("value2"),
    ///             new YamlScalar("key3"), new YamlMapping(    // nesting !!map node
    ///                 new YamlScalar("value3key1"), new YamlScalar("value3value1")
    ///             ),
    ///             new YamlScalar("key4"), new YamlScalar("value4")
    ///         )
    ///     );
    ///     
    /// // Convert it to YAML stream
    /// string yaml = node.ToYaml();
    /// 
    /// // %YAML 1.2
    /// // ---
    /// // - abc
    /// // - 123
    /// // - 1.23
    /// // - - def
    /// //   - ghi
    /// // - key1: value1
    /// //   key2: value2
    /// //   key3:
    /// //     value3key1: value3value1
    /// //   key4: value4
    /// // ...
    /// 
    /// // Load the YAML node from the YAML stream.
    /// // Note that a YAML stream can contain several YAML documents each of which
    /// // contains a root YAML node.
    /// YamlNode[] nodes = YamlNode.FromYaml(yaml);
    /// 
    /// // The only one node in the stream is the one we have presented above.
    /// Assert.AreEqual(1, nodes.Length);
    /// YamlNode resotred = nodes[0];
    /// 
    /// // Check if they are equal to each other.
    /// Assert.AreEquel(node, restored);
    /// 
    /// // Extract sub nodes.
    /// var seq = (YamlSequence)restored;
    /// var map = (YamlMapping)seq[4];
    /// var map2 = (YamlMapping)map[new YamlScalar("key3")];
    /// 
    /// // Modify the restored node tree
    /// map2[new YamlScalar("value3key1")] = new YamlScalar("value3value1 modified");
    /// 
    /// // Now they are not equal to each other.
    /// Assert.AreNotEquel(node, restored);
    /// </code>
    /// 
    /// <h3>YamlNode class</h3>
    /// 
    /// <para><see cref="YamlNode"/> is an abstract class that represents a YAML node.</para>
    /// 
    /// <para>In reality, a <see cref="YamlNode"/> is either <see cref="YamlScalar"/>, <see cref="YamlSequence"/> or 
    /// <see cref="YamlMapping"/>.</para>
    /// 
    /// <para>All <see cref="YamlNode"/> has <see cref="YamlNode.Tag"/> property that denotes
    /// the actual data type represented in the YAML node.</para>
    /// 
    /// <para>Default Tag value for <see cref="YamlScalar"/>, <see cref="YamlSequence"/> or <see cref="YamlMapping"/> are
    /// <c>"tag:yaml.org,2002:str"</c>, <c>"tag:yaml.org,2002:seq"</c>, <c>"tag:yaml.org,2002:map"</c>.</para>
    /// 
    /// <para>Global tags that starts with <c>"tag:yaml.org,2002:"</c> ( = <see cref="YamlNode.DefaultTagPrefix">
    /// YamlNode.DefaultTagPrefix</see>) are defined in the YAML tag repository at 
    /// <a href="http://yaml.org/type/">http://yaml.org/type/</a>. In this library, such a tags can be also 
    /// represented in a short form that starts with <c>"!!"</c>, like <c>"!!str"</c>, <c>"!!seq"</c> and <c>"!!map"</c>. 
    /// Tags in the formal style and the shorthand form can be converted to each other by the static methods of 
    /// <see cref="YamlNode.ExpandTag"/> and <see cref="YamlNode.ShorthandTag(string)"/>. 
    /// In addition to these three basic tags, this library uses <c>"!!null"</c>, <c>"!!bool"</c>, <c>"!!int"</c>, 
    /// <c>"!!float"</c> and <c>"!!timestamp"</c> tags, by default.</para>
    /// 
    /// <para><see cref="YamlNode"/>s can be read from a YAML stream with <see cref="YamlNode.FromYaml(string)"/>,
    /// <see cref="YamlNode.FromYaml(Stream)"/>, <see cref="YamlNode.FromYaml(TextReader)"/> and
    /// <see cref="YamlNode.FromYamlFile(string)"/> static methods. Since a YAML stream generally consist of multiple
    /// YAML documents, each of which has a root YAML node, these methods return an array of <see cref="YamlNode"/>
    /// that is contained in the stream.</para>
    /// 
    /// <para><see cref="YamlNode"/>s can be written to a YAML stream with <see cref="YamlNode.ToYaml()"/>,
    /// <see cref="YamlNode.ToYaml(Stream)"/>, <see cref="YamlNode.ToYaml(TextWriter)"/> and
    /// <see cref="YamlNode.ToYamlFile(string)"/>.</para>
    /// 
    /// <para>The way of serialization can be configured in some aspects. The custom settings are specified
    /// by an instance of <see cref="YamlConfig"/> class. The serialization methods introduced above has
    /// overloaded styles that accepts <see cref="YamlConfig"/> instance to customize serialization.
    /// It is also possible to change the default serialization method by modifying <see cref="YamlNode.DefaultConfig">
    /// YamlNode.DefaultConfig</see> static property.</para>
    /// 
    /// <para>A <see cref="YamlScalar"/> has <see cref="YamlScalar.Value"/> property, which holds the string expression
    /// of the node value.</para>
    /// 
    /// <para>A <see cref="YamlSequence"/> implements <see cref="IList&lt;YamlNode&gt;">IList&lt;YamlNode&gt;</see> 
    /// interface to access the child nodes.</para>
    /// 
    /// <para><see cref="YamlMapping"/> implements 
    /// <see cref="IDictionary&lt;YamlNode,YamlNode&gt;">IDictionary&lt;YamlNode,YamlNode&gt;</see> interface
    /// to access the key/value pairs under the node.</para>
    /// 
    /// <h3>Implicit conversion from C# native object to YamlScalar</h3>
    /// 
    /// <para>Implicit cast operators from <see cref="string"/>, <see cref="bool"/>, <see cref="int"/>, 
    /// <see cref="double"/> and <see cref="DateTime"/> to <see cref="YamlNode"/> is defined. Thus, anytime 
    /// <see cref="YamlNode"/> is required in C# source, naked scalar value can be written. Namely,
    /// methods of <see cref="YamlSequence"/> and <see cref="YamlMapping"/> accept such C# native types 
    /// as arguments in addition to <see cref="YamlNode"/> types. </para>
    /// 
    /// <code>
    /// var map = new YamlMapping();
    /// map["Time"] = DateTime.Now;                 // implicitly converted to YamlScalar
    /// Assert.IsTrue(map.ContainsKey(new YamlScalar("Time")));
    /// Assert.IsTrue(map.ContainsKey("Time"));     // implicitly converted to YamlScalar
    /// </code>
    /// 
    /// <h3>Equality of YamlNodes</h3>
    /// 
    /// <para>Equality of <see cref="YamlNode"/>s are evaluated on the content base. Different <see cref="YamlNode"/> 
    /// objects that have the same content are evaluated to be equal. Use <see cref="Equals(object)"/> method for 
    /// equality evaluation.</para>
    /// 
    /// <para>In detail, two <see cref="YamlNode"/>s are logically equal to each other when the <see cref="YamlNode"/> 
    /// and its child nodes have the same contents (<see cref="YamlNode.Tag"/> and <see cref="YamlScalar.Value"/>) 
    /// and their node graph topology is exactly same.
    /// </para>
    /// 
    /// <code>
    /// YamlNode a1 = "a";  // implicit conversion
    /// YamlNode a2 = "a";  // implicit conversion
    /// YamlNode a3 = new YamlNode("!char", "a");
    /// YamlNode b  = "b";  // implicit conversion
    /// 
    /// Assert.IsTrue(a1 != a2);        // different objects
    /// Assert.IsTrue(a1.Equals(a2));   // different objects having same content
    /// 
    /// Assert.IsFalse(a1.Equals(a3));  // Tag is different
    /// Assert.IsFalse(a1.Equals(b));   // Value is different
    /// 
    /// var s1 = new YamlMapping(a1, new YamlSequence(a1, a2));
    /// var s2 = new YamlMapping(a1, new YamlSequence(a2, a1));
    /// var s3 = new YamlMapping(a2, new YamlSequence(a1, a2));
    /// 
    /// Assert.IsFalse(s1.Equals(s2)); // node graph topology is different
    /// Assert.IsFalse(s1.Equals(s3)); // node graph topology is different
    /// Assert.IsTrue(s2.Equals(s3));  // different objects having same content and node graph topology
    /// </code>
    /// 
    /// </remarks>
    /// <example>
    /// Example 2.27 in YAML 1.2 specification
    /// 
    /// <code>
    /// // %YAML 1.2
    /// // --- 
    /// // !&lt;tag:clarkevans.com,2002:invoice&gt;
    /// // invoice: 34843
    /// // date   : 2001-01-23
    /// // bill-to: &amp;id001
    /// //     given  : Chris
    /// //     family : Dumars
    /// //     address:
    /// //         lines: |
    /// //             458 Walkman Dr.
    /// //             Suite #292
    /// //         city    : Royal Oak
    /// //         state   : MI
    /// //         postal  : 48046
    /// // ship-to: *id001
    /// // product:
    /// //     - sku         : BL394D
    /// //       quantity    : 4
    /// //       description : Basketball
    /// //       price       : 450.00
    /// //     - sku         : BL4438H
    /// //       quantity    : 1
    /// //       description : Super Hoop
    /// //       price       : 2392.00
    /// // tax  : 251.42
    /// // total: 4443.52
    /// // comments:
    /// //     Late afternoon is best.
    /// //     Backup contact is Nancy
    /// //     Billsmer @ 338-4338.
    /// // ...
    /// 
    /// var invoice = new YamlMapping(
    ///     "invoice", 34843,
    ///     "date", new DateTime(2001, 01, 23),
    ///     "bill-to", new YamlMapping(
    ///         "given", "Chris",
    ///         "family", "Dumars",
    ///         "address", new YamlMapping(
    ///             "lines", "458 Walkman Dr.\nSuite #292\n",
    ///             "city", "Royal Oak",
    ///             "state", "MI",
    ///             "postal", 48046
    ///             )
    ///         ),
    ///     "product", new YamlSequence(
    ///         new YamlMapping(
    ///             "sku",         "BL394D",
    ///             "quantity",    4,
    ///             "description", "Basketball",
    ///             "price",       450.00
    ///             ),
    ///         new YamlMapping(
    ///             "sku",         "BL4438H",
    ///             "quantity",    1,
    ///             "description", "Super Hoop",
    ///             "price",       2392.00
    ///             )
    ///         ),
    ///     "tax", 251.42,
    ///     "total", 4443.52,
    ///     "comments", "Late afternoon is best. Backup contact is Nancy Billsmer @ 338-4338."
    ///     );
    /// invoice["ship-to"] = invoice["bill-to"];
    /// invoice.Tag = "tag:clarkevans.com,2002:invoice";
    /// 
    /// invoice.ToYamlFile("invoice.yaml");
    /// // %YAML 1.2
    /// // ---
    /// // !&lt;tag:clarkevans.com,2002:invoice&gt;
    /// // invoice: 34843
    /// // date: 2001-01-23
    /// // bill-to: &amp;A 
    /// //   given: Chris
    /// //   family: Dumars
    /// //   address: 
    /// //     lines: "458 Walkman Dr.\n\
    /// //       Suite #292\n"
    /// //     city: Royal Oak
    /// //     state: MI
    /// //     postal: 48046
    /// // product: 
    /// //   - sku: BL394D
    /// //     quantity: 4
    /// //     description: Basketball
    /// //     price: !!float 450
    /// //   - sku: BL4438H
    /// //     quantity: 1
    /// //     description: Super Hoop
    /// //     price: !!float 2392
    /// // tax: 251.42
    /// // total: 4443.52
    /// // comments: Late afternoon is best. Backup contact is Nancy Billsmer @ 338-4338.
    /// // ship-to: *A
    /// // ...
    /// 
    /// </code>
    /// </example>
    public abstract class YamlNode: IRehashableKey
    {
        private static readonly Dictionary<string, string> ShortToLongTags = new Dictionary<string, string>()
            {
                {"!!map", "tag:yaml.org,2002:map"},
                {"!!seq", "tag:yaml.org,2002:seq"},
                {"!!str", "tag:yaml.org,2002:str"},
                {"!!null", "tag:yaml.org,2002:null"},
                {"!!bool", "tag:yaml.org,2002:bool"},
                {"!!int", "tag:yaml.org,2002:int"},
                {"!!float", "tag:yaml.org,2002:float"},
            };

        private static readonly Dictionary<string, string> LongToShortTags = new Dictionary<string, string>();

        #region Non content values
        /// <summary>
        /// Position in a YAML document, where the node appears. 
        /// Both <see cref="ToYaml()"/> and <see cref="FromYaml(string)"/> sets this property.
        /// When the node appeared multiple times in the document, this property returns the position
        /// where it appeared for the first time.
        /// </summary>
        [DefaultValue(0)]
        public int Row { get; set; }

        /// <summary>
        /// Position in a YAML document, where the node appears. 
        /// Both <see cref="ToYaml()"/> and <see cref="FromYaml(string)"/> sets this property.
        /// When the node appeared multiple times in the document, this property returns the position
        /// where it appeared for the first time.
        /// </summary>
        [DefaultValue(0)]
        public int Column { get; set; }

        /// <summary>
        /// Temporary data, transfering information between YamlRepresenter and YamlPresenter.
        /// </summary>
        internal Dictionary<string, string> Properties { get; private set; }

        /// <summary>
        /// Initialize a node.
        /// </summary>
        public YamlNode()
        {
            Properties = new Dictionary<string, string>();
        }
        #endregion

        /// <summary>
        /// YAML Tag for this node, which represents the type of node's value.
        /// </summary>
        /// <remarks>
        /// <para>
        /// YAML standard types has tags in a form of "tag:yaml.org,2002:???". Well known tags are
        /// tag:yaml.org,2002:null, tag:yaml.org,2002:bool, tag:yaml.org,2002:int, tag:yaml.org,2002:str,
        /// tag:yaml.org,2002:map, tag:yaml.org,2002:seq, tag:yaml.org,2002:float and tag:yaml.org,2002:timestamp.
        /// </para>
        /// </remarks>
        public string Tag
        { 
            get { return tag; }
            set {
                /* strict tag check
                if ( value.StartsWith("!!") )
                    throw new ArgumentException(
                        "Tag vallue {0} must be resolved to a local or global tag before assignment".DoFormat(value));
                if ( !value.StartsWith("!") && !DefaultTagValidator.IsValid(value) )
                    throw new ArgumentException(
                        "{0} is not a valid global tag.".DoFormat(value));
                */
                tag = value;
                shortTag = null;
                OnChanged();
            }
        }
        string tag;
        private string shortTag;

//        static YamlTagValidator TagValidator = new YamlTagValidator();
        /// <summary>
        /// YAML Tag for this node, which represents the type of node's value.
        /// The <see cref="Tag"/> property is returned in a shorthand style.
        /// </summary>
        public string ShorthandTag()
        {
            return shortTag ?? (shortTag = ShorthandTag(Tag));
        }

        #region Hash code
        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// Hash code is calculated using Tag and Value properties.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            // caches hash code
            if ( HashInvalid ) {
                HashCode = GetHashCodeCore();
                HashInvalid = false;
            }
            return HashCode;
        }
        int HashCode;
        bool HashInvalid = true;
        bool ToBeRehash = false;
        /// <summary>
        /// Return the hash code. 
        /// The returned value will be cached until <see cref="OnChanged"/> is called.
        /// </summary>
        /// <returns>Hash code</returns>
        protected abstract int GetHashCodeCore();
        /// <summary>
        /// Call this function when the content of the node is changed.
        /// </summary>
        protected virtual void OnChanged()
        {
            // avoiding inifinite loop
            if ( !ToBeRehash ) {
                try {
                    HashInvalid = true;
                    ToBeRehash = true;
                    if ( Changed != null )
                        Changed(this, EventArgs.Empty);
                } finally {
                    ToBeRehash = false;
                }
            }
        }
        /// <summary>
        /// Invoked when the node's content or its childrens' content was changed.
        /// </summary>
        public event EventHandler Changed;
        #endregion

        /// <summary>
        /// Returns true if <paramref name="obj"/> is of same type as the <see cref="YamlNode"/> and
        /// its content is also logically same.
        /// </summary>
        /// <remarks>
        /// Two <see cref="YamlNode"/>'s are logically equal when the <see cref="YamlNode"/> and its child nodes
        /// have the same contents (<see cref="YamlNode.Tag"/> and <see cref="YamlScalar.Value"/>) 
        /// and their node graph topology is exactly same as the other.
        /// </remarks>
        /// <example>
        /// <code>
        /// var a1 = new YamlNode("a");
        /// var a2 = new YamlNode("a");
        /// var a3 = new YamlNode("!char", "a");
        /// var b  = new YamlNode("b");
        /// 
        /// Assert.IsTrue(a1 != a2);        // different objects
        /// Assert.IsTrue(a1.Equals(a2));   // different objects having same content
        /// 
        /// Assert.IsFalse(a1.Equals(a3));  // Tag is different
        /// Assert.IsFalse(a1.Equals(b));   // Value is different
        /// 
        /// var s1 = new YamlMapping(a1, new YamlSequence(a1, a2));
        /// var s2 = new YamlMapping(a1, new YamlSequence(a2, a1));
        /// var s3 = new YamlMapping(a2, new YamlSequence(a1, a2));
        /// 
        /// Assert.IsFalse(s1.Equals(s2)); // node graph topology is different
        /// Assert.IsFalse(s1.Equals(s3)); // node graph topology is different
        /// Assert.IsTrue(s2.Equals(s3));  // different objects having same content and node graph topology
        /// </code>
        /// </example>
        /// <param name="obj">Object to be compared.</param>
        /// <returns>True if the <see cref="YamlNode"/> logically equals to the <paramref name="obj"/>; otherwise false.</returns>
        public override bool Equals(object obj)
        {
            if ( obj == null || !( obj is YamlNode ) )
                return false;
            var repository = new ObjectRepository();
            return Equals((YamlNode)obj, repository);
        }

        /// <summary>
        /// Called when the node is loaded from a document.
        /// </summary>
        internal virtual void OnLoaded()
        {
        }

        /// <summary>
        /// Remember the order of appearance of nodes. It also has ability of rewinding.
        /// </summary>
        internal class ObjectRepository
        {
            Dictionary<YamlNode, int> nodes_a = 
                new Dictionary<YamlNode, int>(TypeUtils.EqualityComparerByRef<YamlNode>.Default);
            Dictionary<YamlNode, int> nodes_b = 
                new Dictionary<YamlNode, int>(TypeUtils.EqualityComparerByRef<YamlNode>.Default);
            Stack<YamlNode> stack_a = new Stack<YamlNode>();
            Stack<YamlNode> stack_b = new Stack<YamlNode>();

            public class Status
            {
                public int count { get; private set; }
                public Status(int c)
                {
                    count= c;
                }
            }

            public bool AlreadyAppeared(YamlNode a, YamlNode b, out bool identity)
            {
                int ai, bi;
                bool ar = nodes_a.TryGetValue(a, out ai);
                bool br = nodes_b.TryGetValue(b, out bi);
                if ( ar && br && ai == bi ) {
                    identity = true;
                    return true;
                }
                if ( ar ^ br ) {
                    identity = false;
                    return true;
                }
                nodes_a.Add(a, nodes_a.Count);
                nodes_b.Add(b, nodes_b.Count);
                stack_a.Push(a);
                stack_b.Push(b);
                if ( a == b ) {
                    identity = true;
                    return true;
                }
                identity = false;
                return false;
            }

            public Status CurrentStatus
            {
                get { return new Status(stack_a.Count); }
                set
                {
                    var count = value.count;
                    while ( stack_a.Count > count ) {
                        var a = stack_a.Pop();
                        nodes_a.Remove(a);
                        var b = stack_b.Pop();
                        nodes_b.Remove(b);
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if <paramref name="b"/> is of same type as the <see cref="YamlNode"/> and
        /// its content is also logically same.
        /// </summary>
        /// <param name="b">Node to be compared.</param>
        /// <param name="repository">Node repository holds the nodes that already appeared and 
        /// the corresponding node in the other node tree.</param>
        /// <returns>true if they are equal to each other.</returns>
        internal abstract bool Equals(YamlNode b, ObjectRepository repository);
        /// <summary>
        /// Returns true if <paramref name="b"/> is of same type as the <see cref="YamlNode"/> and
        /// its Tag is same as the node. It returns true for <paramref name="skip"/> if they
        /// both already appeared in the node trees and were compared.
        /// </summary>
        /// <param name="b">Node to be compared.</param>
        /// <param name="repository">Node repository holds the nodes that already appeared and 
        /// the corresponding node in the other node tree.</param>
        /// <param name="skip">true if they already appeared in the node tree and were compared.</param>
        /// <returns>true if they are equal to each other.</returns>
        internal bool EqualsSub(YamlNode b, ObjectRepository repository, out bool skip)
        {
            YamlNode a = this;
            bool identity;
            if ( repository.AlreadyAppeared(a, b, out identity) ) {
                skip = true;
                return identity;
            }
            skip = false;
            if ( a.GetType() != b.GetType() || a.Tag != b.Tag )
                return false;
            return true;
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="Object"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="Object"/></returns>
        public override string ToString()
        {
            var length = 1024;
            return ToString(ref length);
        }
        internal abstract string ToString(ref int length);

        #region ToYaml
        /// <summary>
        /// Convert <see cref="YamlNode"/> to a YAML text.
        /// </summary>
        /// <returns>YAML stream.</returns>
        public string ToYaml()
        {
            return ToYaml(DefaultConfig);
        }
        /// <summary>
        /// Convert <see cref="YamlNode"/> to a YAML text.
        /// </summary>
        /// <returns>YAML stream.</returns>
        /// <param name="config"><see cref="YamlConfig">YAML configuration</see> to customize serialization.</param>
        public string ToYaml(YamlConfig config)
        {
            return DefaultPresenter.ToYaml(this, config);
        }
        /// <summary>
        /// Convert <see cref="YamlNode"/> to a YAML text and save it to <see cref="Stream"/> <paramref name="s"/>.
        /// </summary>
        /// <param name="s"><see cref="Stream"/> to output.</param>
        public void ToYaml(Stream s)
        {
            ToYaml(s, DefaultConfig);
        }
        /// <summary>
        /// Convert <see cref="YamlNode"/> to a YAML text and save it to <see cref="Stream"/> <paramref name="s"/>.
        /// </summary>
        /// <param name="s"><see cref="Stream"/> to output.</param>
        /// <param name="config"><see cref="YamlConfig">YAML configuration</see> to customize serialization.</param>
        public void ToYaml(Stream s, YamlConfig config)
        {
            DefaultPresenter.ToYaml(s, this, config);
        }
        /// <summary>
        /// Convert <see cref="YamlNode"/> to a YAML text and save it to <see cref="TextWriter"/> <paramref name="tw"/>.
        /// </summary>
        /// <param name="tw"><see cref="TextWriter"/> to output.</param>
        public void ToYaml(TextWriter tw)
        {
            ToYaml(tw, DefaultConfig);
        }
        /// <summary>
        /// Convert <see cref="YamlNode"/> to a YAML text and save it to <see cref="TextWriter"/> <paramref name="tw"/>.
        /// </summary>
        /// <param name="tw"><see cref="TextWriter"/> to output.</param>
        /// <param name="config"><see cref="YamlConfig">YAML configuration</see> to customize serialization.</param>
        public void ToYaml(TextWriter tw, YamlConfig config)
        {
            DefaultPresenter.ToYaml(tw, this, config);
        }
        #endregion

        #region static members

        /// <summary>
        /// Gets YAML's default tag prefix.
        /// </summary>
        /// <value>"tag:yaml.org,2002:"</value>
        public static string DefaultTagPrefix { get; private set; }
        /// <summary>
        /// Gets or sets the default configuration to customize serialization of <see cref="YamlNode"/>.
        /// </summary>
        public static YamlConfig DefaultConfig { get; set; }
        internal static SerializerContext DefaultSerializerContext { get; set; }
        internal static YamlParser DefaultParser { get; set; }
        internal static YamlPresenter DefaultPresenter { get; set; }

        static YamlNode()
        {
            // Initializing order matters !
            DefaultTagPrefix = "tag:yaml.org,2002:";
            DefaultConfig = new YamlConfig();
            DefaultParser = new YamlParser();
            DefaultPresenter = new YamlPresenter();
            DefaultSerializerContext = new SerializerContext(DefaultConfig);

            foreach (var shortToLongTag in ShortToLongTags)
            {
                LongToShortTags.Add(shortToLongTag.Value, shortToLongTag.Key);
            }


        }

        /// <summary>
        /// Convert YAML text <paramref name="yaml"/> to a list of <see cref="YamlNode"/>.
        /// </summary>
        /// <param name="yaml">YAML text</param>
        /// <returns>YAML nodes</returns>
        public static YamlNode[] FromYaml(string yaml)
        {
            return DefaultParser.Parse(yaml).ToArray();
        }
        /// <summary>
        /// Convert YAML text <paramref name="yaml"/> to a list of <see cref="YamlNode"/>.
        /// </summary>
        /// <param name="yaml">YAML text</param>
        /// <returns>YAML nodes</returns>
        /// <param name="config"><see cref="YamlConfig">YAML configuration</see> to customize serialization.</param>
        public static YamlNode[] FromYaml(string yaml, YamlConfig config)
        {
            return DefaultParser.Parse(yaml, config).ToArray();
        }
        /// <summary>
        /// Convert YAML text <paramref name="yaml"/> to a list of <see cref="YamlNode"/>.
        /// </summary>
        /// <param name="s"><see cref="Stream"/> from which YAML document is read.</param>
        /// <returns>YAML nodes</returns>
        public static YamlNode[] FromYaml(Stream s)
        {
            using ( var sr = new StreamReader(s) )
                return FromYaml(sr);
        }
        /// <summary>
        /// Convert YAML text <paramref name="yaml"/> to a list of <see cref="YamlNode"/>.
        /// </summary>
        /// <param name="s"><see cref="Stream"/> from which YAML document is read.</param>
        /// <returns>YAML nodes</returns>
        /// <param name="config"><see cref="YamlConfig">YAML configuration</see> to customize serialization.</param>
        public static YamlNode[] FromYaml(Stream s, YamlConfig config)
        {
            using ( var sr = new StreamReader(s) )
                return FromYaml(sr, config);
        }
        /// <summary>
        /// Convert YAML text <paramref name="yaml"/> to a list of <see cref="YamlNode"/>.
        /// </summary>
        /// <param name="tr"><see cref="TextReader"/> from which YAML document is read.</param>
        /// <returns>YAML nodes</returns>
        public static YamlNode[] FromYaml(TextReader tr)
        {
            var yaml = tr.ReadToEnd();
            return FromYaml(yaml);
        }
        /// <summary>
        /// Convert YAML text <paramref name="yaml"/> to a list of <see cref="YamlNode"/>.
        /// </summary>
        /// <param name="tr"><see cref="TextReader"/> from which YAML document is read.</param>
        /// <returns>YAML nodes</returns>
        /// <param name="config"><see cref="YamlConfig">YAML configuration</see> to customize serialization.</param>
        public static YamlNode[] FromYaml(TextReader tr, YamlConfig config)
        {
            var yaml = tr.ReadToEnd();
            return FromYaml(yaml, config);
        }

        /// <summary>
        /// Implicit conversion from string to <see cref="YamlScalar"/>.
        /// </summary>
        /// <param name="value">Value to be converted.</param>
        /// <returns>Conversion result.</returns>
        public static implicit operator YamlNode(string value)
        {
            return new YamlScalar(value);
        }
        /// <summary>
        /// Implicit conversion from string to <see cref="YamlScalar"/>.
        /// </summary>
        /// <param name="value">Value to be converted.</param>
        /// <returns>Conversion result.</returns>
        public static implicit operator YamlNode(int value)
        {
            return new YamlScalar("!!int",  YamlNode.DefaultConfig.TypeConverter.ConvertToString(DefaultSerializerContext, value));
        }
        /// <summary>
        /// Implicit conversion from string to <see cref="YamlScalar"/>.
        /// </summary>
        /// <param name="value">Value to be converted.</param>
        /// <returns>Conversion result.</returns>
        public static implicit operator YamlNode(double value)
        {
            return new YamlScalar("!!float", YamlNode.DefaultConfig.TypeConverter.ConvertToString(DefaultSerializerContext, value));
        }
        /// <summary>
        /// Implicit conversion from string to <see cref="YamlScalar"/>.
        /// </summary>
        /// <param name="value">Value to be converted.</param>
        /// <returns>Conversion result.</returns>
        public static implicit operator YamlNode(bool value)
        {
            return new YamlScalar("!!bool", YamlNode.DefaultConfig.TypeConverter.ConvertToString(DefaultSerializerContext, value));
        }
        /// <summary>
        /// Implicit conversion from <see cref="DateTime"/> to <see cref="YamlScalar"/>.
        /// </summary>
        /// <param name="value">Value to be converted.</param>
        /// <returns>Conversion result.</returns>
        public static implicit operator YamlNode(DateTime value)
        {
            YamlScalar node;
            DefaultConfig.TagResolver.Encode(value, out node);
            return node;
        }

        /// <summary>
        /// Convert shorthand tag starting with "!!" to the formal style that starts with "tag:yaml.org,2002:".
        /// </summary>
        /// <remarks>
        /// When <paramref name="tag"/> starts with "!!", it is converted into formal style.
        /// Otherwise, <paramref name="tag"/> is returned as is.
        /// </remarks>
        /// <example>
        /// <code>
        /// var tag = YamlNode.DefaultTagPrefix + "int";    // -> "tag:yaml.org,2002:int"
        /// tag = YamlNode.ShorthandTag(tag);               // -> "!!int"
        /// tag = YamlNode.ExpandTag(tag);                  // -> "tag:yaml.org,2002:int"
        /// </code>
        /// </example>
        /// <param name="tag">Tag in the shorthand style.</param>
        /// <returns>Tag in formal style.</returns>
        public static string ExpandTag(string tag)
        {
            var newTag = tag;
            if (newTag.StartsWith("!!"))
            {
                if (!ShortToLongTags.TryGetValue(tag, out newTag))
                {
                    newTag = new StringBuilder(DefaultTagPrefix.Length + tag.Length - 2).Append(DefaultTagPrefix)
                                                                                      .Append(tag, 2, tag.Length - 2)
                                                                                      .ToString();
                }
            }
            return newTag;
        }

        /// <summary>
        /// Convert a formal style tag that starts with "tag:yaml.org,2002:" to 
        /// the shorthand style that starts with "!!".
        /// </summary>
        /// <remarks>
        /// When <paramref name="tag"/> contains YAML standard types, it is converted into !!xxx style.
        /// Otherwise, <paramref name="tag"/> is returned as is.
        /// </remarks>
        /// <example>
        /// <code>
        /// var tag = YamlNode.DefaultTagPrefix + "int";    // -> "tag:yaml.org,2002:int"
        /// tag = YamlNode.ShorthandTag(tag);               // -> "!!int"
        /// tag = YamlNode.ExpandTag(tag);                  // -> "tag:yaml.org,2002:int"
        /// </code>
        /// </example>
        /// <param name="tag">Tag in formal style.</param>
        /// <returns>Tag in compact style.</returns>
        public static string ShorthandTag(string tag)
        {
            var newTag = tag;
            if (newTag != null && newTag.StartsWith(DefaultTagPrefix))
            {
                if (!LongToShortTags.TryGetValue(tag, out newTag))
                {
                    newTag =
                        new StringBuilder(2 + tag.Length - DefaultTagPrefix.Length).Append("!!")
                                                                                   .Append(tag, DefaultTagPrefix.Length,
                                                                                           tag.Length -
                                                                                           DefaultTagPrefix.Length)
                                                                                   .ToString();
                }
            }
            return newTag;
        }

        #endregion
    }
}