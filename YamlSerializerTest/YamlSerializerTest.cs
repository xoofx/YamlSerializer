﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using YamlSerializer;
using YamlSerializer.Serialization;
using System.ComponentModel;
using System.Drawing;
using System.Collections;

namespace YamlSerializerTest
{
    public class TestClass
    {
        public List<TestClass> list = new List<TestClass>();
    }
    
    public class ChildClass: TestClass
    {
    }

    [Flags]
    enum TestEnum: uint
    {
        abc = 1,
        あいう = 2
    }

    public class Test1
    {
        #pragma warning disable 169, 649
        public int PublicProp { get; set; }         // processed (by assign)
        protected int ProtectedProp { get; set; }           // Ignored
        private int PrivateProp { get; set; }               // Ignored
        internal int InternalProp { get; set; }             // Ignored

        public int PublicField;                     // processed (by assign)
        protected int ProtectedField;                       // Ignored
        private int PrivateField;                           // Ignored
        internal int InternalField;                         // Ignored

        public List<string> ClassPropByAssign // processed (by assign)
        { get; set; }
        
        public int ReadOnlyValueProp { get; private set; }  // Ignored
        public List<string> ReadOnlyClassProp // processed (by content)
        { get; private set; }

        [YamlSerialize(YamlSerializeMethod.Content)]
        public List<string> ClassPropByContent// processed (by content)
        { get; set; }

        public int[] IntArrayField =                // processed (by assign)
           new int[10];

        [YamlSerialize(YamlSerializeMethod.Binary)]
        public byte[] ByteArrayFieldBinary =          // processed (as binary)
           new byte[100];

        [YamlSerialize(YamlSerializeMethod.Never)]
        public int PublicPropHidden;                        // Ignored

        public Test1()
        {
            ClassPropByAssign = new List<string>();
            ReadOnlyClassProp = new List<string>();
            ClassPropByContent = new List<string>();
        }
        #pragma warning restore 169, 649
    }

    public class Test2
    {
        [DefaultValue(0)]
        public int Default0 = 0;

        [DefaultValue("a")]
        public string Defaulta = "a";

        public int DynamicDefault = 0;

        bool ShouldSerializeDynamicDefault()
        {
            return Default0 != DynamicDefault;
        }
    }

    [TestFixture]
    public class YamlSerializerTest
    {
        public static string BuildResult(params string[] lines)
        {
            var result = "%YAML 1.2\r\n---\r\n";
            foreach ( var line in lines )
                result += line + "\r\n";
            result += "...\r\n";
            return result;
        }

        private Serializer serializer { get; set; }

        [TestFixtureSetUp]
        public void InitSerializer()
        {
            var config = new YamlConfig();
            config.Register(new LegacyTypeConverterFactory());
            config.LookupAssemblies.Add(typeof(System.Drawing.SolidBrush).Assembly);
            config.LookupAssemblies.Add(typeof(YamlSerializerTest).Assembly);
            serializer = new Serializer(config);
        }

        [Test]
        public void SequentialWrite()
        {
            var yaml = "";
            yaml += serializer.Serialize("a");
            yaml += serializer.Serialize(1);
            yaml += serializer.Serialize(1.1);
            // %YAML 1.2
            // ---
            // a
            // ...
            // %YAML 1.2
            // ---
            // 1
            // ...
            // %YAML 1.2
            // ---
            // 1.1
            // ...
            Assert.AreEqual(
                BuildResult("a")+
                BuildResult("1")+
                BuildResult("1.1"),
                yaml);

            object[] objects = serializer.Deserialize(yaml);
            // objects[0] == "a"
            // objects[1] == 1
            // objects[2] == 1.1

            Assert.AreEqual(new object[] { "a", 1, 1.1 }, objects);
        }

        [Test]
        public void TestDefaultValue()
        {
            var test2 = new Test2();
            var yaml = serializer.Serialize(test2);
            // %YAML 1.2
            // ---
            // !YamlSerializerTest.Test2 {}
            // ...

            test2.Defaulta = "b";
            yaml = serializer.Serialize(test2);
            // %YAML 1.2
            // ---
            // !YamlSerializerTest.Test2
            // Defaulta: b
            // ...

            test2.Defaulta = "a";
            test2.DynamicDefault = 1;
            yaml = serializer.Serialize(test2);
            // %YAML 1.2
            // ---
            // !YamlSerializerTest.Test2
            // DynamicDefault: 1
            // ...

            test2.Default0 = 1;
            yaml = serializer.Serialize(test2);
            // %YAML 1.2
            // ---
            // !YamlSerializerTest.Test2
            // Default0: 1
            // ...
        }

        [Test]
        public void PropertiesAndFields1()
        {
            var test1 = new Test1();
            test1.PublicField = 2;
            test1.PublicProp = 3;
            test1.ClassPropByAssign.Add("abc");
            test1.ReadOnlyClassProp.Add("def");
            test1.ClassPropByContent.Add("ghi");
            var rand = new Random(0);
            for ( int i = 0; i < test1.ByteArrayFieldBinary.Length; i++ )
                test1.ByteArrayFieldBinary[i] = (byte)rand.Next();

            string yaml = serializer.Serialize(test1);
            // %YAML 1.2
            // ---
            // !YamlSerializerTest.Test1
            // PublicProp: 0
            // ReadOnlyClassProp: 
            //   Capacity: 4
            //   ICollection.Items: 
            //     - abc
            // ClassPropByContent: 
            //   Capacity: 0
            // PublicField: 0
            // IntArrayFieldBinary: |+2
            //     Gor1XAwenmhGkU5ib9NxR11LXxp1iYlH5LH4c9hImTitWSB9Z78II2UvXSXV99A79fj6UBn3GDzbIbd9
            //     yBDjAyslYm58iGd/NN+tVjuLRCg3cJBo+PWMbIWm9n4AEC0E7LKXWV5HXUNk7I13APEDWFMM/kWTz2EK
            //     s7LzFw2gBjpKugkmQJqIfinpQ1J1yqhhz/XjA3TBxDBsEuwrD+SNevQSqEC+/KRbwgE6D011ACMeyRt0
            //     BOG6ZesRKCtL0YU6tSnLEpgKVBz+R300qD3/W0aZVk+1vHU+auzyGCGUaHCGd6dpRoEhXoIg2m3+AwJX
            //     EJ37T+TA9BuEPJtyGoq+crQMFQtXj1Zriz3HFbReclLvDdVpZlcOHPga/3+3Y509EHZ7UyT7H1xGeJxn
            //     eXPrDDb0Ul04MfZb4UYREOfR3HNzNTUYGRsIPUvHOEW7AaoplIfkVQp19DvGBrBqlP2TZ9atlWUHVdth
            //     7lIBeIh0wiXxoOpCbQ7qVP9GkioQUrMkOcAJaad3exyZaOsXxznFCA==
            // ...
            Assert.AreEqual(
                BuildResult(
                    "!YamlSerializerTest.Test1",
                    "ByteArrayFieldBinary: |+2",
                    "  GgxGb1115NitZ2XV9RnbyCt8NDs3+IUA7F5kAFOTsw1KQCl1z3RsD/S+wk0eBOtLtZj+qEa1aiGGRoL+",
                    "  EOSEGrRXi7TvZvi3ECRGeTY44edzGUu7lArGlNYH7ojxbf8QOaeZxw==",
                    "ClassPropByAssign: ",
                    "  Capacity: 4",
                    "  ICollection.Items: ",
                    "    - abc",
                    "ClassPropByContent: ",
                    "  Capacity: 4",
                    "  ICollection.Items: ",
                    "    - ghi",
                    "IntArrayField: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0]",
                    "PublicField: 2",
                    "PublicProp: 3",
                    "ReadOnlyClassProp: ",
                    "  Capacity: 4",
                    "  ICollection.Items: ",
                    "    - def"
                    ),
                    yaml);

            object restored = serializer.Deserialize(yaml)[0];
            Assert.AreEqual(test1.PublicField, ( (Test1)restored ).PublicField);
            Assert.AreEqual(test1.PublicProp, ( (Test1)restored ).PublicProp);
            Assert.AreEqual(test1.ReadOnlyClassProp, ( (Test1)restored ).ReadOnlyClassProp);
            Assert.AreEqual(test1.ClassPropByContent, ( (Test1)restored ).ClassPropByContent);
            Assert.AreEqual(test1.ByteArrayFieldBinary, ((Test1)restored).ByteArrayFieldBinary);
        }

        [Test]
        public void ObjectArray()
        {
            object obj = new object[]{ 
                null,
                "abc", 
                true, 
                1, 
                (Byte)1,
                1.0, 
                "1",
                new double[]{ 1.1, 2, -3 },
                new string[]{ "def", "ghi", "1" },
                //new System.Drawing.Point(1,3),    // TODO replace System.Drawing.Point
                new YamlScalar("brabrabra") 
            };

            string yaml = serializer.Serialize(obj);
            // %YAML 1.2
            // ---
            // - null
            // - abc
            // - true
            // - 1
            // - !System.Byte 1
            // - !!float 1
            // - "1"
            // - !&lt;!System.Double[]&gt; [1.1, 2, -3]
            // - !&lt;!System.String[]&gt;
            //   - def
            //   - ghi
            //   - "1"
            // - !System.Drawing.Point 1, 3
            // - !YamlSerializer.YamlScalar
            //   Value: brabrabra
            //   Tag: tag:yaml.org,2002:str
            // ...

            Assert.AreEqual(
                BuildResult(
                    "- null",
                    "- abc",
                    "- true",
                    "- 1",
                    "- !System.Byte 1",
                    "- !!float 1",
                    "- \"1\"",
                    "- !<!System.Double[]> [1.1, 2, -3]",
                    "- !<!System.String[]>",
                    "  - def",
                    "  - ghi",
                    "  - \"1\"",
                    //"- !System.Drawing.Point 1, 3", // TODO Replace System.Drawing.Point
                    "- !YamlSerializer.YamlScalar",
                    "  Tag: tag:yaml.org,2002:str",
                    "  Value: brabrabra"
                    ),
                    yaml);
            object restored = serializer.Deserialize(yaml)[0];
            Assert.AreEqual(obj, restored);
        }

        [Test]
        public void RepeatedLargeString()
        {
            // 1000 chars
            string a =
                "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
                "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
                "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
                "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
                "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
                "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
                "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
                "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
                "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
                "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";
            string b = "12345";
            object obj = new object[] { a, a, b, b };
            string yaml = serializer.Serialize(obj);
            // %YAML 1.2
            // ---
            // - &A 01234567890123456789012345678901234567890123456789...
            // - *A
            // - "12345"
            // - "12345"
            // ...
            Assert.AreEqual(
                BuildResult(
                    "- &A " + a,
                    "- *A",
                    "- \"12345\"",
                    "- \"12345\""),
                yaml);
            object restored = serializer.Deserialize(yaml)[0];
            Assert.AreEqual(obj, restored);
            Assert.AreSame(( (object[])restored )[0], ( (object[])restored )[1]);
            Assert.AreNotSame(( (object[])restored )[2], ( (object[])restored )[3]);
        }
        
        [Test]
        public void RecursiveObjectsTest()
        {
            var a = new TestClass();
            var b = new ChildClass();
            a.list.Add(a);
            a.list.Add(a);
            a.list.Add(b);
            a.list.Add(a);
            a.list.Add(b);
            b.list.Add(a);
            string yaml = serializer.Serialize(a);
            // %YAML 1.2
            // ---
            // &A !YamlSerializerTest.TestClass
            // list: 
            //   Capacity: 8
            //   ICollection.Items: 
            //     - *A
            //     - *A
            //     - &B !YamlSerializerTest.ChildClass
            //       list: 
            //         Capacity: 4
            //         ICollection.Items: 
            //           - *A
            //     - *A
            //     - *B
            // ...
            Assert.AreEqual(
                BuildResult(
                    "&A !YamlSerializerTest.TestClass",
                    "list: ",
                    "  Capacity: 8",
                    "  ICollection.Items: ",
                    "    - *A",
                    "    - *A",
                    "    - &B !YamlSerializerTest.ChildClass",
                    "      list: ",
                    "        Capacity: 4",
                    "        ICollection.Items: ",
                    "          - *A",
                    "    - *A",
                    "    - *B"
                    ),
                yaml);

            object restored = serializer.Deserialize(yaml)[0];
            Assert.AreSame(restored, ( (TestClass)restored ).list[0]);
            Assert.AreSame(restored, ( (TestClass)restored ).list[1]);
            Assert.AreSame(restored, ( (TestClass)restored ).list[2].list[0]);
            Assert.AreSame(restored, ( (TestClass)restored ).list[3]);
            Assert.AreSame(( (TestClass)restored ).list[2], ( (TestClass)restored ).list[4]);
            Assert.IsInstanceOf<ChildClass>(( (TestClass)restored ).list[2]);
        }

        [Test]
        public void TestVariousFormats()
        {
            var dict = new Dictionary<object, object>();
            dict.Add(new object[] { 1, "a" }, new object());
            object obj = new object[]{
                dict,
                null,
                "abc",
                "1",
                "a ",
                "- a",
                "abc\n", 
                "abc\r\ndef\r\n", 
                "abc\r\ndef\r\nghi", 
                new double[]{ 1.1, 2, -3, 3.12, 13.2 },
                new int[,] { { 1, 3}, {4, 5}, {10, 1} },
                new string[]{ "jkl", "mno\r\npqr" },
                //new System.Drawing.Point(1,3), // TODO Check replacement for System.Drawing.Point
                TestEnum.abc,
                TestEnum.abc | TestEnum.あいう,
            };
            // %YAML 1.2
            // ---
            // - 
            //   Keys: {}
            //   Values: {}
            //   IDictionary.Entries: 
            //     ? - 1
            //       - a
            //     : !System.Object {}
            // - null
            // - abc
            // - "1"
            // - "a "
            // - "- a"
            // - "abc\n"
            // - |+2
            //   abc
            //   def
            // - |-2
            //   abc
            //   def
            //   ghi
            // - !&lt;!System.Double[]&gt; [1.1, 2, -3, 3.12, 13.2]
            // - !&lt;!System.Int32[,]&gt; [[1, 3], [4, 5], [10, 1]]
            // - !&lt;!System.String[]&gt;
            //   - jkl
            //   - |-2
            //     mno
            //     pqr
            // - !System.Drawing.Point 1, 3
            // - !YamlSerializerTest.TestEnum abc
            // - !YamlSerializerTest.TestEnum abc, あいう
            // ...

            string yaml = serializer.Serialize(obj);
            Assert.AreEqual(
                BuildResult(
                    "- ? - 1",
                    "    - a",
                    "  : !System.Object {}",
                    "- null",
                    "- abc",
                    "- \"1\"",
                    "- \"a \"",
                    "- \"- a\"",
                    "- \"abc\\n\"",
                    @"- ""abc\r\n\",
                    @"  def\r\n""",
                    @"- ""abc\r\n\",
                    @"  def\r\n\",
                    @"  ghi""",
                    "- !<!System.Double[]> [1.1, 2, -3, 3.12, 13.2]",
                    "- !<!System.Int32[,]> [[1, 3], [4, 5], [10, 1]]",
                    "- !<!System.String[]>",
                    "  - jkl",
                    @"  - ""mno\r\n\",
                    @"    pqr""",
                    //"- !System.Drawing.Point 1, 3", // TODO CHECK REPLACEMENT
                    "- !YamlSerializerTest.TestEnum abc",
                    "- !YamlSerializerTest.TestEnum abc, あいう"),
                yaml);

            object restored = serializer.Deserialize(yaml)[0];
            var dict2= (Dictionary<object, object>)( (object[])restored )[0];
            Assert.AreEqual(dict.Count, dict2.Count);
            Assert.AreEqual(dict.Keys.First(), dict2.Keys.First());
            Assert.AreEqual(dict.Values.First().GetType(), dict2.Values.First().GetType());
            Assert.AreEqual(
                ( (object[])obj ).Skip(1).ToArray(),
                ( (object[])restored ).Skip(1).ToArray());
        }

        [Test]
        public void TestVariousTypes()
        {
            // TODO Check for test replacement. Color is using a specialized TypeConverter
            /*
            var yaml = serializer.Serialize(Color.Aqua);
            Assert.AreEqual(
                BuildResult(
                    "!System.Drawing.Color Aqua"),
                yaml);
            object obj = serializer.Deserialize(yaml);

            yaml = serializer.Serialize(Color.FromArgb(128, Color.Blue));
            Assert.AreEqual(
                BuildResult(
                    "!System.Drawing.Color 128, 0, 0, 255"),
                yaml);
            obj = serializer.Deserialize(yaml);

            yaml = serializer.Serialize(Brushes.Black);
            Assert.AreEqual(
                BuildResult(
                    "!System.Drawing.SolidBrush",
                    "Color: Black"),
                yaml);
            // obj = serializer.Deserialize(yaml);

            yaml = serializer.Serialize(new Pen(new SolidBrush(Color.White), 10));
            Assert.AreEqual(
                BuildResult(
                    "!System.Drawing.Pen",
                    "Alignment: Center",
                    "Brush: !System.Drawing.SolidBrush",
                    "  Color: 255, 255, 255",
                    "Color: 255, 255, 255",
                    "CompoundArray: []",
                    "DashCap: Flat",
                    "DashOffset: 0",
                    "DashStyle: Solid",
                    "EndCap: Flat",
                    "LineJoin: Miter",
                    "MiterLimit: 10",
                    "StartCap: Flat",
                    "Transform: ",
                    "  Elements: [1, 0, 0, 1, 0, 0]",
                    "Width: 10"
                    ),
                yaml);
            // obj = serializer.Deserialize(yaml);

            yaml = serializer.Serialize(new Font("Times", 12));
            Assert.AreEqual(
                BuildResult(
                    "!System.Drawing.Font Times New Roman, 12pt"),
                yaml);
            obj = serializer.Deserialize(yaml);
            */
        }

        [Test]
        public void TestVariousString()
        {
            var obj = new object[] {
                "",
                "abc",
                "1",
                "true",
                "null",
                "1.0",
                "@",
                "`",
                "-",
                "*",
                "|",
                ">",
                "=",
                "&",
                "#",
                "?",
                "abc\n",
                "abc\r\ndef",
                "abc\r\ndef\r\n",
                "abc\r\ndef\r\n\r\n",
                "\r\nabc",
                "\r\n\r\nabc",
                "\r\n\r\nabc  ",
                " \x5c \x22 \x07 \x08 \x1b \x0c \x09 \x0b \x00 \xa0 \x85 \u2028 \u2029",
                ",a",
                "]a",
                "}a",
                "'a",
                "%a"
            };

            var yaml = serializer.Serialize(obj);
            Assert.AreEqual(
                BuildResult(
                    "- \"\"",
                    "- abc",
                    "- \"1\"",
                    "- \"true\"",
                    "- \"null\"",
                    "- \"1.0\"",
                    "- \"@\"",
                    "- \"`\"",
                    "- \"-\"",
                    "- \"*\"",
                    "- \"|\"",
                    "- \">\"",
                    "- =",
                    "- \"&\"",
                    "- \"#\"",
                    "- \"?\"",
                    "- \"abc\\n\"",
                    @"- ""abc\r\n\",
                    @"  def""",
                    @"- ""abc\r\n\",
                    @"  def\r\n""",
                    @"- ""abc\r\n\",
                    @"  def\r\n\",
                    @"  \r\n""",
                    @"- ""\r\n\",
                    @"  abc""",
                    @"- ""\r\n\",
                    @"  \r\n\",
                    @"  abc""",
                    @"- ""\r\n\",                       
                    @"  \r\n\",
                    @"  abc  """,
                    @"- "" \\ \"" \a \b \e \f "+"\t"+@" \v \0 \_ \N \L \P""",
                    "- \",a\"",
                    "- \"]a\"",
                    "- \"}a\"",
                    "- \"'a\"",
                    "- \"%a\""
                ),
                yaml
            );

            var restored = serializer.Deserialize(yaml)[0];
            Assert.AreEqual(obj, restored);
        }

        public struct ArrayTestElement
        {
            public int a;
            public char b;
        }

        public class ArrayTestElement2
        {
            public int a;
            public char b;
            public override bool Equals(object obj)
            {
                return ( obj is ArrayTestElement2 ) &&
                    ( (ArrayTestElement2)obj ).a == a &&
                    ( (ArrayTestElement2)obj ).b == b;
            }
            public override int GetHashCode()
            {
                return a.GetHashCode() ^ b.GetHashCode();
            }
        }

        public class ArrayTestClass
        {
            [YamlSerialize(YamlSerializeMethod.Binary)]
            public int[] IntArray1 = new int[100];

            [YamlSerialize(YamlSerializeMethod.Binary)]
            public int[,] IntArray2 = new int[10, 10];

            [YamlSerialize(YamlSerializeMethod.Binary)]
            public ArrayTestElement[] StructArray1 = new ArrayTestElement[100];

            [YamlSerialize(YamlSerializeMethod.Binary)]
            public ArrayTestElement[,] StructArray2 = new ArrayTestElement[10,10];
        }

        [Test]
        public void TestArray()
        {
            var rand = new Random(0);
            var a1 = new int[100];
            for ( int i = 0; i < a1.Length; i++ )
                a1[i] = rand.Next();
            var yaml = serializer.Serialize(a1);
            Assert.AreEqual(a1, serializer.Deserialize(yaml)[0]);

            var a2 = new byte[100];
            for ( int i = 0; i < a2.Length; i++ )
                a2[i] = Convert.ToByte(rand.Next() & 0xff);
            yaml = serializer.Serialize(a2);
            Assert.AreEqual(a2, serializer.Deserialize(yaml)[0]);

            var a3 = new double[100];
            for ( int i = 0; i < a3.Length; i++ )
                a3[i] = rand.NextDouble();
            yaml = serializer.Serialize(a3);
            var a3restored = (double[])serializer.Deserialize(yaml)[0];
            for ( int i = 0; i < a3.Length; i++ )
                Assert.AreEqual(a3[i].ToString(), a3restored[i].ToString());

            var a4 = new double[10,10];
            for ( int i = 0; i < a4.Length; i++ )
                a4[i / 10, i % 10] = rand.NextDouble();
            yaml = serializer.Serialize(a4);
            var a4restored = (double[,])serializer.Deserialize(yaml)[0];
            for ( int i = 0; i < a4.Length; i++ )
                Assert.AreEqual(a4[i / 10, i % 10].ToString(), a4restored[i / 10, i % 10].ToString());

            var a5 = new ArrayTestElement[100];
            var a6 =new ArrayTestElement[10,10];
            var a7 = new ArrayTestElement2[100];
            var a8 = new ArrayTestElement2[10, 10];
            var a9 = new ArrayTestClass();

            for ( int i = 0; i < 100; i++ ) {
                a5[i].a = rand.Next();
                a5[i].b = (char)( rand.Next() & 0xffff );
                a6[i % 10, i / 10].a = rand.Next();
                a6[i % 10, i / 10].b = (char)( rand.Next() & 0xffff );
                a7[i] = new ArrayTestElement2();
                a7[i].a = rand.Next();
                a7[i].b = (char)(rand.Next() & 0xff);
                a8[i % 10, i / 10] = new ArrayTestElement2();
                a8[i % 10, i / 10].a = rand.Next();
                a8[i % 10, i / 10].b = (char)( rand.Next() & 0xffff );
                a9.IntArray1[i] = rand.Next();
                a9.IntArray2[i % 10, i / 10] = rand.Next();
                a9.StructArray1[i].a = rand.Next();
                a9.StructArray1[i].b = (char)( rand.Next() & 0xffff );
                a9.StructArray2[i % 10, i / 10].a = rand.Next();
                a9.StructArray2[i % 10, i / 10].b = (char)( rand.Next() & 0xffff );
            }
            
            yaml = serializer.Serialize(a5);
            var a5restored = (ArrayTestElement[])serializer.Deserialize(yaml)[0];
            Assert.AreEqual(a5, a5restored);
            
            yaml = serializer.Serialize(a6);
            var a6restored = (ArrayTestElement[,])serializer.Deserialize(yaml)[0];
            Assert.AreEqual(a6, a6restored);
            
            yaml = serializer.Serialize(a7);
            var a7restored = (ArrayTestElement2[])serializer.Deserialize(yaml)[0];
            Assert.AreEqual(a7, a7restored);

            yaml = serializer.Serialize(a8);
            var a8restored = (ArrayTestElement2[,])serializer.Deserialize(yaml)[0];
            Assert.AreEqual(a8, a8restored);

            yaml = serializer.Serialize(a9);
            var a9restored = (ArrayTestClass)serializer.Deserialize(yaml)[0];
            Assert.AreEqual(a9.IntArray1, a9restored.IntArray1);
            Assert.AreEqual(a9.IntArray2, a9restored.IntArray2);
            Assert.AreEqual(a9.StructArray1, a9restored.StructArray1);
            Assert.AreEqual(a9.StructArray2, a9restored.StructArray2);

            var a10 = new TestEnum[10];
            for ( int i = 0; i < a10.Length; i++ )
                a10[i] = ( rand.Next() & 1 ) == 0 ? TestEnum.abc : TestEnum.あいう;
            yaml = serializer.Serialize(a10);
            var a10restored = (TestEnum[])serializer.Deserialize(yaml)[0];
            Assert.AreEqual(a10, a10restored);
        }

        [Test]
        public void TestCollections()
        {
            var c1 = new ArrayList();
            c1.Add("abc");
            c1.Add(1);
            c1.Add(new Test1());
            var yaml = serializer.Serialize(c1);
            Assert.AreEqual(
                BuildResult(
                "!System.Collections.ArrayList",
                "Capacity: 4",
                "ICollection.Items: ",
                "  - abc",
                "  - 1",
                "  - !YamlSerializerTest.Test1",
                "    ByteArrayFieldBinary: |+2",
                "      AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
                "      AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==",
                "    ClassPropByAssign: ",
                "      Capacity: 0",
                "    ClassPropByContent: ",
                "      Capacity: 0",
                "    IntArrayField: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0]",
                "    PublicField: 0",
                "    PublicProp: 0",
                "    ReadOnlyClassProp: ",
                "      Capacity: 0"
                ),
                yaml
            );
            Assert.AreEqual(yaml, serializer.Serialize(serializer.Deserialize(yaml)[0]));

            var c2 = new System.Collections.Hashtable();
            c2.Add(10, 5);
            c2.Add("abc", 123);
            c2.Add(TestEnum.あいう, 5);
            yaml = serializer.Serialize(c2);
            Assert.AreEqual(
                BuildResult(
                    "!System.Collections.Hashtable",
                    "IDictionary.Entries: ",
                    "  10: 5",
                    "  abc: 123",
                    "  !YamlSerializerTest.TestEnum あいう: 5"
                    ),
                    yaml);
            Assert.AreEqual(yaml, serializer.Serialize(serializer.Deserialize(yaml)[0]));
        }

        [Test]
        public void TestCustomActivator()
        {
            var config = new YamlConfig();
            config.Register(new LegacyTypeConverterFactory());
            config.LookupAssemblies.Add(typeof(System.Drawing.SolidBrush).Assembly);
            config.LookupAssemblies.Add(typeof(YamlSerializerTest).Assembly);

            var serializer = new Serializer(config);
            var yaml =
              @"%YAML 1.2
---
!System.Drawing.SolidBrush
Color: Red
...
";

            SolidBrush b = null;
            try {
                b = (SolidBrush)serializer.Deserialize(yaml)[0];
            } catch ( MissingMethodException ) {
                // SolidBrush has no default constructor!
            }

            config.AddActivator<SolidBrush>(() => new SolidBrush(Color.Black));
            serializer = new Serializer(config);

            // Now the serializer knows how to activate an instance of SolidBrush.
            b = (SolidBrush)serializer.Deserialize(yaml)[0];

            Assert.AreEqual(b.Color, Color.Red);
        
        }

        [Test]
        public void TestMappingToDictionary()
        {
            var obj= (Dictionary<object,object>)serializer.Deserialize(
                "{a: 1, 2: 1.0}"
                )[0];
            Assert.AreEqual(obj["a"], 1);
            Assert.AreEqual(obj[2], 1.0);
            var yaml = serializer.Serialize(obj);
            Assert.AreEqual(
                BuildResult(
                    "2: !!float 1",
                    "a: 1"
                ), yaml
                );
        }

        [Test]
        public void TestOmitRootNodesTag()
        {
            var obj = new TestClass();
            obj.list.Add(new ChildClass());
            var serializer = new Serializer();
            var yaml= serializer.Serialize(obj);
            Assert.AreEqual(
                BuildResult(
                    "!YamlSerializerTest.TestClass",
                    "list: ",
                    "  Capacity: 4",
                    "  ICollection.Items: ",
                    "    - !YamlSerializerTest.ChildClass",
                    "      list: ",
                    "        Capacity: 0"
                ), yaml
            );

            var config = new YamlConfig();
            config.OmitTagForRootNode = true;
            serializer = new Serializer(config);
            yaml = serializer.Serialize(obj);
            Assert.AreEqual(
                BuildResult(
                    "list: ",
                    "  Capacity: 4",
                    "  ICollection.Items: ",
                    "    - !YamlSerializerTest.ChildClass",
                    "      list: ",
                    "        Capacity: 0"
                ), yaml
            );
        }
    }

}
