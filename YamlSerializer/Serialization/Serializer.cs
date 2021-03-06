﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;
using System.Collections;
using System.Text.RegularExpressions;
using System.Diagnostics;

using System.IO;

namespace YamlSerializer.Serialization
{                   
    /// <summary>
    /// <para><see cref="Serializer"/> class has instance methods <see cref="Serialize(object)"/> and <see cref="Deserialize(string,Type[])"/>, 
    /// with which C# native objects can be converted into / from YAML text without any preparations.</para>
    /// <code>
    /// var serializer = new Serializer);
    /// object obj = GetObjectToSerialize();
    /// string yaml = serializer.Serialize(obj);
    /// object restored = serializer.Deserialize(yaml);
    /// Assert.AreEqual(obj, restored);
    /// </code>
    /// </summary>
    /// 
    /// <remarks>
    /// <h3>What kind of objects can be serialized?</h3>
    /// 
    /// <para><see cref="Serializer"/> can serialize / deserialize native C# objects of primitive types 
    /// (bool, char, int,...), enums, built-in non-primitive types (string, decimal), structures, 
    /// classes and arrays of these types. </para>
    /// 
    /// <para>
    /// On the other hand, it does not deal with IntPtr (which is a primitive type, though) and 
    /// pointer types (void*, int*, ...) because these types are, by their nature, not persistent.
    /// </para>
    /// 
    /// <para>
    /// Classes without a default constructor can be deserialized only when the way of activating an instance 
    /// is explicitly specified by <see cref="YamlConfig.AddActivator"/>.
    /// </para>
    /// 
    /// <para><code>
    /// object obj = new object[]{ 
    ///     null,
    ///     "abc", 
    ///     true, 
    ///     1, 
    ///     (Byte)1,
    ///     1.0, 
    ///     "1",
    ///     new double[]{ 1.1, 2, -3 },
    ///     new string[]{ "def", "ghi", "1" },
    ///     new System.Drawing.Point(1,3), 
    ///     new System.Drawing.SolidBrush(Color.Blue)
    /// };
    /// 
    /// var serializer = new Serializer);
    /// string yaml = serializer.Serialize(obj);
    /// // %YAML 1.2
    /// // ---
    /// // - null
    /// // - abc
    /// // - True
    /// // - 1
    /// // - !System.Byte 1
    /// // - !!float 1
    /// // - "1"
    /// // - !&lt;!System.Double[]%gt; [1.1, 2, -3]
    /// // - !&lt;!System.String[]%gt;
    /// //   - def
    /// //   - ghi
    /// // - !System.Drawing.Point 1, 3
    /// // - !System.Drawing.SolidBrush
    /// //   Color: Blue
    /// // ...
    /// 
    /// object restored;
    /// try {
    ///     restored = Serializer.Deserialize(yaml)[0];
    /// } catch(MissingMethodException) {
    ///     // default constructor is missing for SolidBrush
    /// }
    ///  
    /// // Let the library know how to activate an instance of SolidBrush.
    /// YamlNode.DefaultConfig.AddActivator&lt;System.Drawing.SolidBrush&gt;(
    ///     () => new System.Drawing.SolidBrush(Color.Black /* dummy */));
    /// 
    /// // Then, all the objects can be restored correctly.
    /// restored = serializer.Deserialize(yaml)[0];
    /// </code></para>
    /// 
    /// <para>A YAML document generated by <see cref="Serializer"/> always have a %YAML directive and
    /// explicit document start (<c>"---"</c>) and end (<c>"..."</c>) marks. 
    /// This allows several documents to be written in a single YAML stream.</para>
    /// 
    /// <code>
    ///  var yaml = "";
    ///  var serializer = new Serializer);
    ///  yaml += serializer.Serialize("a");
    ///  yaml += serializer.Serialize(1);
    ///  yaml += serializer.Serialize(1.1);
    ///  // %YAML 1.2
    ///  // ---
    ///  // a
    ///  // ...
    ///  // %YAML 1.2
    ///  // ---
    ///  // 1
    ///  // ...
    ///  // %YAML 1.2
    ///  // ---
    ///  // 1.1
    ///  // ...
    /// 
    ///  object[] objects = serializer.Deserialize(yaml);
    ///  // objects[0] == "a"
    ///  // objects[1] == 1
    ///  // objects[2] == 1.1
    /// </code>
    /// 
    /// <para>Since a YAML stream can consist of multiple YAML documents as above,
    /// <see cref="Deserialize(string, Type[])"/> returns an array of <see cref="object"/>.
    /// </para>
    /// 
    /// <h3>Serializing structures and classes</h3>
    /// 
    /// <para>For structures and classes, by default, all public fields and public properties are 
    /// serialized. Note that protected / private members are always ignored.</para>
    /// 
    /// <h4>Serialization methods</h4>
    /// 
    /// <para>Readonly value-type members are also ignored because there is no way to 
    /// assign a new value to them on deserialization, while readonly class-type members 
    /// are serialized. When deserializing, instead of creating a new object and assigning it 
    /// to the member, the child members of such class instance are restored independently. 
    /// Such a deserializing method is refered to <see cref="YamlSerializeMethod.Content">
    /// YamlSerializeMethod.Content</see>. 
    /// </para>
    /// <para>
    /// On the other hand, when writeable fields/properties are deserialized, new objects are 
    /// created by using the parameters in the YAML description and assiend to the fields/properties. 
    /// Such a deserializing method is refered to <see cref="YamlSerializeMethod.Assign">
    /// YamlSerializeMethod.Assign</see>. Writeable properties can be explicitly specified to use 
    /// <see cref="YamlSerializeMethod.Content"> YamlSerializeMethod.Content</see> method for 
    /// deserialization, by adding <see cref="YamlSerializeAttribute"/> to its definition.
    /// </para>
    /// 
    /// <para>Another type of serializing method is <see cref="YamlSerializeMethod.Binary">
    /// YamlSerializeMethod.Binary</see>. 
    /// This method is only applicable to an array-type field / property that contains
    /// only value-type members.</para>
    /// 
    /// <para>If serializing method <see cref="YamlSerializeMethod.Never"/> is specified,
    /// the member is never serialized nor deserialized.</para>
    /// 
    /// <code>
    /// public class Test1
    /// {
    ///     public int PublicProp { get; set; }         // processed (by assign)
    ///     protected int ProtectedProp { get; set; }           // Ignored
    ///     private int PrivateProp { get; set; }               // Ignored
    ///     internal int InternalProp { get; set; }             // Ignored
    /// 
    ///     public int PublicField;                     // processed (by assign)
    ///     protected int ProtectedField;                       // Ignored
    ///     private int PrivateField;                           // Ignored
    ///     internal int InternalField;                         // Ignored
    /// 
    ///     public List&lt;string&gt; ClassPropByAssign // processed (by assign)
    ///     { get; set; }
    ///     
    ///     public int ReadOnlyValueProp { get; private set; }  // Ignored
    ///     public List&lt;string&gt; ReadOnlyClassProp // processed (by content)
    ///     { get; private set; }
    /// 
    ///     [YamlSerialize(YamlSerializeMethod.Content)]
    ///     public List&lt;string&gt; ClassPropByContent// processed (by content)
    ///     { get; set; }
    ///
    ///     public int[] IntArrayField =                // processed (by assign)
    ///        new int[10];
    ///
    ///     [YamlSerialize(YamlSerializeMethod.Binary)]
    ///     public int[] IntArrayFieldBinary =          // processed (as binary)
    ///        new int[100];
    ///
    ///     [YamlSerialize(YamlSerializeMethod.Never)]
    ///     public int PublicPropHidden;                        // Ignored
    ///
    ///     public Test1()
    ///     {
    ///         ClassPropByAssign = new List&lt;string&gt;();
    ///         ReadOnlyClassProp = new List&lt;string&gt;();
    ///         ClassPropByContent = new List&lt;string&gt;();
    ///     }
    /// }
    /// 
    /// public void TestPropertiesAndFields1()
    /// {
    ///    var test1 = new Test1();
    ///    test1.ClassPropByAssign.Add("abc");
    ///    test1.ReadOnlyClassProp.Add("def");
    ///    test1.ClassPropByContent.Add("ghi");
    ///    var rand = new Random(0);
    ///    for ( int i = 0; i &lt; test1.IntArrayFieldBinary.Length; i++ )
    ///        test1.IntArrayFieldBinary[i] = rand.Next();
    /// 
    ///    var serializer = new Serializer);
    ///    string yaml = serializer.Serialize(test1);
    ///    // %YAML 1.2
    ///    // ---
    ///    // !SerializerTest.Test1
    ///    // PublicProp: 0
    ///    // ClassPropByAssign: 
    ///    //   Capacity: 4
    ///    //   ~Items: 
    ///    //     - abc
    ///    // ReadOnlyClassProp: 
    ///    //   Capacity: 4
    ///    //   ~Items: 
    ///    //     - def
    ///    // ClassPropByContent: 
    ///    //   Capacity: 4
    ///    //   ~Items: 
    ///    //     - ghi
    ///    // PublicField: 0
    ///    // IntArrayField: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0]
    ///    // IntArrayFieldBinary: |+2
    ///    //   Gor1XAwenmhGkU5ib9NxR11LXxp1iYlH5LH4c9hImTitWSB9Z78II2UvXSXV99A79fj6UBn3GDzbIbd9
    ///    //   yBDjAyslYm58iGd/NN+tVjuLRCg3cJBo+PWMbIWm9n4AEC0E7LKXWV5HXUNk7I13APEDWFMM/kWTz2EK
    ///    //   s7LzFw2gBjpKugkmQJqIfinpQ1J1yqhhz/XjA3TBxDBsEuwrD+SNevQSqEC+/KRbwgE6D011ACMeyRt0
    ///    //   BOG6ZesRKCtL0YU6tSnLEpgKVBz+R300qD3/W0aZVk+1vHU+auzyGCGUaHCGd6dpRoEhXoIg2m3+AwJX
    ///    //   EJ37T+TA9BuEPJtyGoq+crQMFQtXj1Zriz3HFbReclLvDdVpZlcOHPga/3+3Y509EHZ7UyT7H1xGeJxn
    ///    //   eXPrDDb0Ul04MfZb4UYREOfR3HNzNTUYGRsIPUvHOEW7AaoplIfkVQp19DvGBrBqlP2TZ9atlWUHVdth
    ///    //   7lIBeIh0wiXxoOpCbQ7qVP9GkioQUrMkOcAJaad3exyZaOsXxznFCA==
    ///    // ...
    /// }
    /// </code>
    /// 
    /// <h4>Default values of fields and properties</h4>
    /// 
    /// <para><see cref="Serializer"/> is aware of <see cref="System.ComponentModel.DefaultValueAttribute">
    /// System.ComponentModel.DefaultValueAttribute</see>.
    /// So, when a member of a structure / class instance has a value that equals to the default value, 
    /// the member will not be written in the YAML text.</para>
    /// 
    /// <para>It also checkes for the result of ShouldSerializeXXX method. For instance, just before serializing <c>Font</c>
    /// property of some type, <c>bool ShouldSerializeFont()</c> method is called if exists. If the method returns false, 
    /// <c>Font</c> property will not be written in the YAML text. ShouldSerializeXXX method can be non-public.</para>
    /// 
    /// <code>
    /// using System.ComponentModel;
    /// 
    /// public class Test2
    /// {
    ///     [DefaultValue(0)]
    ///     public int Default0 = 0;
    /// 
    ///     [DefaultValue("a")]
    ///     public string Defaulta = "a";
    /// 
    ///     public int DynamicDefault = 0;
    /// 
    ///     bool ShouldSerializeDynamicDefault()
    ///     {
    ///         return Default0 != DynamicDefault;
    ///     }
    /// }
    /// 
    /// public void TestDefaultValue()
    /// {
    ///     var test2 = new Test2();
    ///     var serializer = new Serializer);
    ///     
    ///     // All properties have defalut values.
    ///     var yaml = serializer.Serialize(test2);
    ///     // %YAML 1.2
    ///     // ---
    ///     // !YamlSerializerTest.Test2 {}
    ///     // ...
    /// 
    ///     test2.Defaulta = "b";
    ///     yaml = serializer.Serialize(test2);
    ///     // %YAML 1.2
    ///     // ---
    ///     // !YamlSerializerTest.Test2
    ///     // Defaulta: b
    ///     // ...
    /// 
    ///     test2.Defaulta = "a";
    ///     var yaml = serializer.Serialize(test2);
    ///     // %YAML 1.2
    ///     // ---
    ///     // !YamlSerializerTest.Test2 {}
    ///     // ...
    /// 
    ///     test2.DynamicDefault = 1;
    ///     yaml = serializer.Serialize(test2);
    ///     // %YAML 1.2
    ///     // ---
    ///     // !YamlSerializerTest.Test2
    ///     // DynamicDefault: 1
    ///     // ...
    /// 
    ///     test2.Default0 = 1;
    ///     yaml = serializer.Serialize(test2);
    ///     // %YAML 1.2
    ///     // ---
    ///     // !YamlSerializerTest.Test2
    ///     // Default0: 1
    ///     // ...
    /// }
    /// </code>
    /// 
    /// <h4>Collection classes</h4>
    /// 
    /// <para>If an object implements <see cref="ICollection&lt;T&gt;"/>, <see cref="IList"/> or <see cref="IDictionary"/>
    /// the child objects are serialized as well its other public members. 
    /// Pseudproperty <c>~Items</c> appears to hold the child objects.</para>
    /// 
    /// <h3>Multitime appearance of a same object</h3>
    /// 
    /// <para><see cref="Serializer"/> preserve C# objects' graph structure. Namely, when a same objects are refered to
    /// from several points in the object graph, the structure is correctly described in YAML text and restored objects
    /// preserve the structure. <see cref="Serializer"/> can safely manipulate directly / indirectly self refering 
    /// objects, too.</para>
    /// 
    /// <code>
    /// public class TestClass
    /// {
    ///     public List&lt;TestClass&gt; list = 
    ///         new List&lt;TestClass&gt;();
    /// }
    /// 
    /// public class ChildClass: TestClass
    /// {
    /// }
    /// 
    /// void RecursiveObjectsTest()
    /// {
    ///     var a = new TestClass();
    ///     var b = new ChildClass();
    ///     a.list.Add(a);
    ///     a.list.Add(a);
    ///     a.list.Add(b);
    ///     a.list.Add(a);
    ///     a.list.Add(b);
    ///     b.list.Add(a);
    ///     var serializer = new Serializer);
    ///     string yaml = serializer.Serialize(a);
    ///     // %YAML 1.2
    ///     // ---
    ///     // &amp;A !TestClass
    ///     // list: 
    ///     //   Capacity: 8
    ///     //   ~Items: 
    ///     //     - *A
    ///     //     - *A
    ///     //     - &amp;B !ChildClass
    ///     //       list: 
    ///     //         Capacity: 4
    ///     //         ~Items: 
    ///     //           - *A
    ///     //     - *A
    ///     //     - *B
    ///     // ...
    ///     
    ///     var restored = (TestClass)serializer.Deserialize(yaml)[0];
    ///     Assert.IsTrue(restored == restored.list[0]);
    ///     Assert.IsTrue(restored == restored.list[1]);
    ///     Assert.IsTrue(restored == restored.list[3]);
    ///     Assert.IsTrue(restored == restored.list[5]);
    ///     Assert.IsTrue(restored.list[2] == restored.list[4]);
    /// }
    /// </code>
    /// 
    /// <para>This is not the case if the object is <see cref="string"/>. Same instances of 
    /// <see cref="string"/> are repeatedly written in a YAML text and restored as different 
    /// instance of <see cref="string"/> when deserialized, unless the content of the string
    /// is extremely long (longer than 999 chars).</para>
    /// 
    /// <code>
    ///  // 1000 chars
    ///  string long_str =
    ///      "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
    ///      "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
    ///      "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
    ///      "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
    ///      "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
    ///      "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
    ///      "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
    ///      "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
    ///      "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
    ///      "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";
    ///  string short_str = "12345";
    ///  object obj = new object[] { long_str, long_str, short_str, short_str };
    ///  var serializer = new Serializer);
    ///  string yaml = serializer.Serialize(obj);
    ///  // %YAML 1.2
    ///  // ---
    ///  // - &amp;A 01234567890123456789012345678901234567890123456789 ... (snip) ... 789
    ///  // - *A
    ///  // - "12345"
    ///  // - "12345"
    ///  // ...
    /// </code>
    /// 
    /// <h3>YAML text written / read by <see cref="Serializer"/></h3>
    /// 
    /// <para>When serializing, <see cref="Serializer"/> intelligently uses various YAML 1.2 styles, 
    /// namely the block style, flow style, explicit mapping and implicit mapping, to maximize readability
    /// of the YAML stream.</para>
    /// 
    /// <code>
    /// [Flags]
    /// enum TestEnum: uint 
    /// { 
    ///     abc = 1, 
    ///     あいう = 2 
    /// } 
    /// 
    /// public void TestVariousFormats()
    /// {
    ///     var dict = new Dictionary&lt;object, object&gt;();
    ///     dict.Add(new object[] { 1, "a" }, new object());
    ///     object obj = new object[]{
    ///         dict,
    ///         null,
    ///         "abc",
    ///         "1",
    ///         "a ",
    ///         "- a",
    ///         "abc\n", 
    ///         "abc\ndef\n", 
    ///         "abc\ndef\nghi", 
    ///         new double[]{ 1.1, 2, -3, 3.12, 13.2 },
    ///         new int[,] { { 1, 3}, {4, 5}, {10, 1} },
    ///         new string[]{ "jkl", "mno\npqr" },
    ///         new System.Drawing.Point(1,3),
    ///         TestEnum.abc,
    ///         TestEnum.abc | TestEnum.あいう,
    ///     };
    ///     var config = new YamlConfig();
    ///     config.ExplicitlyPreserveLineBreaks = false;
    ///     var serializer = new Serializerconfig);
    ///     string yaml = serializer.Serialize(obj);
    ///     
    ///     // %YAML 1.2
    ///     // ---
    ///     // - !&lt;!System.Collections.Generic.Dictionary%602[[System.Object,...],[System.Object,...]]&gt;
    ///     //   Keys: {}
    ///     //   Values: {}
    ///     //   IDictionary.Entries: 
    ///     //     ? - 1
    ///     //       - a
    ///     //     : !System.Object {}
    ///     // - null
    ///     // - abc
    ///     // - "1"
    ///     // - "a "
    ///     // - "- a"
    ///     // - "abc\n"
    ///     // - |+2
    ///     //   abc
    ///     //   def
    ///     // - |-2
    ///     //   abc
    ///     //   def
    ///     //     ghi
    ///     // - !&lt;!System.Double[]&gt; [1.1, 2, -3, 3.12, 13.2]
    ///     // - !&lt;!System.Int32[,]&gt; [[1, 3], [4, 5], [10, 1]]
    ///     // - !&lt;!System.String[]&gt;
    ///     //   - jkl
    ///     //   - |-2
    ///     //     mno
    ///     //     pqr
    ///     // - !System.Drawing.Point 1, 3
    ///     // - !TestEnum abc
    ///     // - !TestEnum abc, あいう
    ///     // ...
    /// }
    /// </code>
    /// 
    /// <para>When deserializing, <see cref="Serializer"/> accepts any valid YAML 1.2 documents.
    /// TAG directives, comments, flow / block styles, implicit / explicit mappings can be freely used
    /// to express valid C# objects. Namely, the members of the array can be given eighter in a flow style
    /// or in a block style.
    /// </para>
    /// 
    /// <para>By default, <see cref="Serializer"/> outputs a YAML stream with line break of "\r\n".
    /// This can be customized either by setting <c>YamlNode.DefaultConfig.LineBreakForOutput</c> or 
    /// by giving an instance of <see cref="YamlConfig"/> to the <see cref="SerializerYamlConfig">
    /// constructor</see>.
    /// </para>
    /// 
    /// <code>
    /// var serializer = new Serializer);
    /// var yaml = serializer.Serialize("abc");
    /// // %YAML 1.2\r\n    // line breaks are explicitly shown in this example
    /// // ---\r\n
    /// // abc\r\n
    /// // ...\r\n
    /// 
    /// var config = new YamlConfig();
    /// config.LineBreakForOutput = "\n";
    /// serializer = new Serializerconfig);
    /// var yaml = serializer.Serialize("abc");
    /// // %YAML 1.2\n
    /// // ---\n
    /// // abc\n
    /// // ...\n
    /// 
    /// YamlNode.DefaultConfig.LineBreakForOutput = "\n";
    /// 
    /// var serializer = new Serializer);
    /// serializer = new Serializer);
    /// var yaml = serializer.Serialize("abc");
    /// // %YAML 1.2\n
    /// // ---\n
    /// // abc\n
    /// // ...\n
    /// </code>
    /// 
    /// <h4>Line breaks in YAML text</h4>
    /// 
    /// <para>By default, line breaks in multi line values are explicitly presented as escaped style. 
    /// Although, this makes the resulting YAML stream hard to read, it is necessary to preserve
    /// the exact content of the string because the YAML specification requires that a YAML parser 
    /// must normalize evely line break that is not escaped in a YAML document to be a single line 
    /// feed "\n" when deserializing.</para>
    /// 
    /// <para>In order to have the YAML documents easy to be read, set 
    /// <see cref="YamlConfig.ExplicitlyPreserveLineBreaks">YamlConfig.ExplicitlyPreserveLineBreaks
    /// </see> false. Then, the multiline values of will be written in literal style.</para> 
    /// 
    /// <para>Of course, it makes all the line breaks to be normalized into a single line feeds "\n".</para>
    /// 
    /// <code>
    /// var serializer = new Serializer);
    /// var text = "abc\r\n  def\r\nghi\r\n";
    /// // abc
    /// //   def
    /// // ghi
    /// 
    /// // By default, line breaks explicitly appears in escaped form.
    /// var yaml = serializer.Serialize(text);
    /// // %YAML 1.2
    /// // ---
    /// // "abc\r\n\
    /// // \  def\r\n\
    /// // ghi\r\n"
    /// // ...
    /// 
    /// // Original line breaks are preserved
    /// var restored = (string)serializer.Deserialize(yaml)[0];
    /// // "abc\r\n  def\r\nghi\r\n"
    ///
    /// 
    /// YamlNode.DefaultConfig.ExplicitlyPreserveLineBreaks = false;
    /// 
    /// // Literal style is easier to be read.
    /// var yaml = serializer.Serialize(text);
    /// // %YAML 1.2
    /// // ---
    /// // |+2
    /// //   abc
    /// //     def
    /// //   ghi
    /// // ...
    /// 
    /// // Original line breaks are lost.
    /// var restored = (string)serializer.Deserialize(yaml)[0];
    /// // "abc\n  def\nghi\n"
    /// </code>
    /// 
    /// <para>This library offers two work arounds for this problem, although both of which
    /// violates the official behavior of a YAML parser defined in the YAML specification.</para>
    /// 
    /// <para>One is to set <see cref="YamlConfig.LineBreakForInput">YamlConfig.LineBreakForInput</see> 
    /// to be "\r\n". Then, the YAML parser normalizes all line breaks into "\r\n" instead of "\n".</para>
    ///
    /// <para>The other is to set <see cref="YamlConfig.NormalizeLineBreaks">YamlConfig.NormalizeLineBreaks</see> 
    /// false. It disables the line break normalization both at output and at input. Namely, the line breaks are 
    /// written and read as-is when serialized / deserialized.</para>
    /// 
    /// <code>
    /// var serializer = new Serializer);
    /// 
    /// // text with mixed line breaks
    /// var text = "abc\r  def\nghi\r\n"; 
    /// // abc\r        // line breaks are explicitly shown in this example
    /// //   def\n
    /// // ghi\r\n
    /// 
    /// YamlNode.DefaultConfig.ExplicitlyPreserveLineBreaks = false;
    /// 
    /// // By default, all line breaks are normalized to "\r\n" when serialized.
    /// var yaml = serializer.Serialize(text);
    /// // %YAML 1.2\r\n
    /// // ---\r\n
    /// // |+2\r\n
    /// //   abc\r\n
    /// //     def\r\n
    /// //   ghi\r\n
    /// // ...\r\n
    /// 
    /// // When deserialized, line breaks are normalized into "\n".
    /// var restored = (string)serializer.Deserialize(yaml)[0];
    /// // "abc\n  def\nghi\n"
    /// 
    /// // Line breaks are normalized into "\r\n" instead of "\n".
    /// YamlNode.DefaultConfig.LineBreakForInput = "\r\n";
    /// restored = (string)serializer.Deserialize(yaml)[0];
    /// // "abc\r\n  def\r\nghi\r\n"
    /// 
    /// // Line breaks are written as is,
    /// YamlNode.DefaultConfig.NormalizeLineBreaks = false;
    /// var yaml = serializer.Serialize(text);
    /// // %YAML 1.2\r\n
    /// // ---\r\n
    /// // |+2\r\n
    /// //   abc\r
    /// //     def\n
    /// //   ghi\r\n
    /// // ...\r\n
    /// 
    /// // and are read as is.
    /// restored = (string)serializer.Deserialize(yaml)[0];
    /// // "abc\r  def\nghi\r\n"
    /// 
    /// // Note that when the line breaks of YAML stream is changed 
    /// // between serialization and deserialization, the original
    /// // line breaks are lost.
    /// yaml = yaml.Replace("\r\n", "\n").Replace("\r", "\n");
    /// restored = (string)serializer.Deserialize(yaml)[0];
    /// // "abc\n  def\nghi\n"
    /// </code>
    /// 
    /// <para>It is repeatedly stated that although these two options are useful in many situation,
    /// they makes the YAML parser violate the YAML specification. </para>
    /// 
    /// </remarks>
    public class Serializer
    {
        private readonly static YamlRepresenter Representer = new YamlRepresenter();
        private readonly static YamlConstructor Constructor = new YamlConstructor();

        private readonly YamlConfig config = null;

        /// <summary>
        /// Initialize an instance of <see cref="Serializer"/> that obeys
        /// <see cref="YamlNode.DefaultConfig"/>.
        /// </summary>
        public Serializer() : this(YamlNode.DefaultConfig)
        {
        }

        /// <summary>
        /// Initialize an instance of <see cref="Serializer"/> with custom <paramref name="config"/>.
        /// </summary>
        /// <param name="config">Custom <see cref="YamlConfig"/> to customize serialization.</param>
        public Serializer(YamlConfig config)
        {
            if (config == null) throw new ArgumentNullException("config");
            this.config = config;
        }

        /// <summary>
        /// Gets the configuration attached to this serializer.
        /// </summary>
        /// <value>The configuration.</value>
        public YamlConfig Config
        {
            get { return config; }
        }

        /// <summary>
        /// Serialize C# object <paramref name="obj"/> into YAML text.
        /// </summary>
        /// <param name="obj">Object to be serialized.</param>
        /// <returns>YAML text.</returns>
        public string Serialize(object obj)
        {
            var context = config.CreateContext();
            var node = Representer.ObjectToNode(obj, context);
            return node.ToYaml(config);
        }
        /// <summary>
        /// Serialize C# object <paramref name="obj"/> into YAML text and write it into a <see cref="Stream"/> <paramref name="s"/>.
        /// </summary>
        /// <param name="s">A <see cref="Stream"/> to which the YAML text is written.</param>
        /// <param name="obj">Object to be serialized.</param>
        public void Serialize(Stream s, object obj)
        {
            var context = config.CreateContext();
            var node = Representer.ObjectToNode(obj, context);
            node.ToYaml(s, config);
        }
        /// <summary>
        /// Serialize C# object <paramref name="obj"/> into YAML text and write it into a <see cref="TextWriter"/> <paramref name="tw"/>.
        /// </summary>
        /// <param name="tw">A <see cref="TextWriter"/> to which the YAML text is written.</param>
        /// <param name="obj">Object to be serialized.</param>
        public void Serialize(TextWriter tw, object obj)
        {
            var context = config.CreateContext();
            var node = Representer.ObjectToNode(obj, context);
            node.ToYaml(tw, config);
        }

        /// <summary>
        /// Deserialize C# object(s) from a YAML text. Since a YAML text can contain multiple YAML documents, each of which 
        /// represents a C# object, the result is returned as an array of <see cref="object"/>.
        /// </summary>
        /// <param name="yaml">A YAML text from which C# objects are deserialized.</param>
        /// <param name="types">Expected type(s) of the root object(s) in the YAML stream.</param>
        /// <returns>C# object(s) deserialized from YAML text.</returns>
        public object[] Deserialize(string yaml, params Type[] types)
        {
            var context = config.CreateContext();
            var parser = new YamlParser();
            var nodes = parser.Parse(yaml, config);
            var objects = new List<object>();
            for ( int i = 0; i < nodes.Count; i++ ) {
                var node = nodes[i];
                objects.Add(Constructor.NodeToObject(node, i < types.Length ? types[i] : null, context));
            }
            return objects.ToArray();
        }

        /// <summary>
        /// Deserialize C# object(s) from a YAML text in a <see cref="Stream"/> <paramref name="s"/>. 
        /// Since a YAML text can contain multiple YAML documents, each of which 
        /// represents a C# object, the result is returned as an array of <see cref="object"/>.
        /// </summary>
        /// <param name="s">A <see cref="Stream"/> that contains YAML text from which C# objects are deserialized.</param>
        /// <param name="types">Expected type(s) of the root object(s) in the YAML stream.</param>
        /// <returns>C# object(s) deserialized from YAML text.</returns>
        public object[] Deserialize(Stream s, params Type[] types)
        {
            return Deserialize(new StreamReader(s), types);
        }

        /// <summary>
        /// Deserialize C# object(s) from a YAML text in a <see cref="TextReader"/> <paramref name="tr"/>. 
        /// Since a YAML text can contain multiple YAML documents, each of which 
        /// represents a C# object, the result is returned as an array of <see cref="object"/>.
        /// </summary>
        /// <param name="tr">A <see cref="TextReader"/> that contains YAML text from which C# objects are deserialized.</param>
        /// <param name="types">Expected type(s) of the root object(s) in the YAML stream.</param>
        /// <returns>C# object(s) deserialized from YAML text.</returns>
        public object[] Deserialize(TextReader tr, params Type[] types)
        {
            return Deserialize(tr.ReadToEnd(), types);
        }
    }
}
