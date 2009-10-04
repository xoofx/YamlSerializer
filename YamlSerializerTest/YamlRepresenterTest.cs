using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using System.Yaml;
using System.Yaml.Serialization;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace YamlSerializerTest
{
    [TestFixture]
    public class YamlRepresenterTest: YamlTestFixture
    {

        [Flags]
        enum TestEnum: int { abc = 1, あいう = 2 };

        [Test]
        public void TestTypeIsPrimitive()
        {
            Assert.IsTrue(typeof(bool).IsPrimitive);
            Assert.IsTrue(typeof(byte).IsPrimitive);
            Assert.IsTrue(typeof(sbyte).IsPrimitive);
            Assert.IsTrue(typeof(char).IsPrimitive);
            Assert.IsTrue(typeof(double).IsPrimitive);
            Assert.IsTrue(typeof(float).IsPrimitive);
            Assert.IsTrue(typeof(int).IsPrimitive);
            Assert.IsTrue(typeof(uint).IsPrimitive);
            Assert.IsTrue(typeof(long).IsPrimitive);
            Assert.IsTrue(typeof(ulong).IsPrimitive);
            Assert.IsTrue(typeof(short).IsPrimitive);
            Assert.IsTrue(typeof(ushort).IsPrimitive);

            Assert.IsFalse(typeof(TestEnum).IsPrimitive);
            Assert.IsFalse(typeof(decimal).IsPrimitive);
            Assert.IsFalse(typeof(string).IsPrimitive);

            Assert.IsFalse(typeof(object).IsPrimitive);
        }

        public static string BuildResult(params string[] lines)
        {
            var result = "%YAML 1.2\r\n---\r\n";
            foreach ( var line in lines )
                result += line + "\r\n";
            result += "...\r\n";
            return result;
        }

        YamlSerializer YamlSerializer = new YamlSerializer();

        [Test]
        public void TestPrimitiveScalars()
        {
            Assert.AreEqual(
                BuildResult("True"),
                YamlSerializer.Serialize(true));
            Assert.AreEqual(
                BuildResult("False"),
                YamlSerializer.Serialize(false));
            Assert.AreEqual(
                BuildResult("!System.Byte 1"),
                YamlSerializer.Serialize((byte)1));
            Assert.AreEqual(
                BuildResult("!System.SByte 1"),
                YamlSerializer.Serialize((sbyte)1));
            Assert.AreEqual(
                BuildResult("!System.Char a"),
                YamlSerializer.Serialize('a'));
            Assert.AreEqual(
                BuildResult("!System.Char \"\\n\""),
                YamlSerializer.Serialize('\n'));
            Assert.AreEqual(
                BuildResult("!System.Char \"\\r\""),
                YamlSerializer.Serialize('\r'));
            Assert.AreEqual(
                BuildResult("!System.Char \"\\0\""),
                YamlSerializer.Serialize('\0'));
            Assert.AreEqual(
                BuildResult("!System.Char \"\\x03\""),
                YamlSerializer.Serialize('\x03'));
            Assert.AreEqual(
                BuildResult("!System.Byte 1"),
                YamlSerializer.Serialize((byte)1));
            Assert.AreEqual(
                BuildResult("!System.SByte 1"),
                YamlSerializer.Serialize((sbyte)1));
            Assert.AreEqual(
                BuildResult("!!float 1"),
                YamlSerializer.Serialize((double)1));
            Assert.AreEqual(
                BuildResult("1.1"),
                YamlSerializer.Serialize((double)1.1));
            Assert.AreEqual(
                BuildResult("-1.1"),
                YamlSerializer.Serialize((double)-1.1));
            Assert.AreEqual(
                BuildResult("-1.1E-10"),
                YamlSerializer.Serialize((double)-1.1e-10));
            Assert.AreEqual(
                BuildResult("!System.Single 1"),
                YamlSerializer.Serialize((float)1));
            Assert.AreEqual(
                BuildResult("!System.Single 1.1"),
                YamlSerializer.Serialize((float)1.1));
            Assert.AreEqual(
                BuildResult("1"),
                YamlSerializer.Serialize(1));
            Assert.AreEqual(
                BuildResult("-1"),
                YamlSerializer.Serialize(-1));
            Assert.AreEqual(
                BuildResult("!System.UInt32 1"),
                YamlSerializer.Serialize((uint)1));
            Assert.AreEqual(
                BuildResult("!System.Int64 1"),
                YamlSerializer.Serialize((long)1));
            Assert.AreEqual(
                BuildResult("!System.UInt64 1"),
                YamlSerializer.Serialize((ulong)1));
            Assert.AreEqual(
                BuildResult("!System.Int16 1"),
                YamlSerializer.Serialize((short)1));
            Assert.AreEqual(
                BuildResult("!System.UInt16 1"),
                YamlSerializer.Serialize((ushort)1));
        }

        [Test]
        public void TestScalarsNotPrimitive()
        {
            Assert.AreEqual(
                BuildResult("null"),
                YamlSerializer.Serialize(null));
            Assert.AreEqual(
                BuildResult("!System.Decimal 1"),
                YamlSerializer.Serialize((decimal)1));
            Assert.AreEqual(
                BuildResult("!YamlSerializerTest.YamlRepresenterTest%2BTestEnum abc"),
                YamlSerializer.Serialize(TestEnum.abc));
            Assert.AreEqual(
                BuildResult("!YamlSerializerTest.YamlRepresenterTest%2BTestEnum あいう"),
                YamlSerializer.Serialize(TestEnum.あいう));
            Assert.AreEqual(
                BuildResult("!System.Object {}"),
                YamlSerializer.Serialize(new object()));
            Assert.AreEqual(
                BuildResult("!YamlSerializerTest.YamlRepresenterTest%2BTestEnum abc, あいう"),
                YamlSerializer.Serialize(TestEnum.abc | TestEnum.あいう));
            var converter = new EasyTypeConverter();
            Assert.AreEqual(
                TestEnum.abc | TestEnum.あいう,
                converter.ConvertFromString("abc, あいう", typeof(TestEnum)));
        }

        [Test]
        public void TestStringScalars()
        {
            Assert.AreEqual(
                BuildResult("\"null\""),
                YamlSerializer.Serialize("null"));
            Assert.AreEqual(
                BuildResult("\"1\""),
                YamlSerializer.Serialize("1"));
            Assert.AreEqual(
                BuildResult("\"-1\""),
                YamlSerializer.Serialize("-1"));
            Assert.AreEqual(
                BuildResult("\"-1.0E12\""),
                YamlSerializer.Serialize("-1.0E12"));
            Assert.AreEqual(
                BuildResult("\"True\""),
                YamlSerializer.Serialize("True"));
            Assert.AreEqual(
                BuildResult(@"""\0\a\b"+"\t"+@"\v\f\e\""/\\\N\_\L\P"""),
                YamlSerializer.Serialize("\x00\x07\x08\x09\x0b\x0c\x1b\x22\x2f\x5c\x85\xa0\u2028\u2029"));
            Assert.AreEqual(
                BuildResult("\"abc\\n\""),
                YamlSerializer.Serialize("abc\n"));
            Assert.AreEqual(
                BuildResult(@"""abc\n\",@"abc"""),
                YamlSerializer.Serialize("abc\nabc"));
        }

        [Test]
        public void TestArray1()
        {
            var o = new object();
            var array1 = new object[] {
                true,
                false,
                (byte)1,
                (sbyte)1,
                'a',
                '\n',
                '\r',
                '\0',
                '\x03',
                (byte)1,
                (sbyte)1,
                (double)1,
                (double)1.1,
                (double)-1.1,
                (double)-1.1e-10,
                (float)1,
                (float)1.1,
                1,
                -1,
                (uint)1,
                (long)1,
                (ulong)1,
                (short)1,
                (ushort)1,
                null,
                (decimal)1,
                TestEnum.abc,
                TestEnum.あいう,
                "null",
                "1",
                "-1",
                "-1.0E12",
                "True",
                "\x00\x07\x08\x09\x0b\x0c\x1b\x22\x2f\x5c\x85\xa0\u2028\u2029",
                "abc\n",
                "abc\n abc",
                "null", // string value is not anchored & aliased.
                o,
                o
            };

            Assert.AreEqual(
                BuildResult(
                    "- True",
                    "- False",
                    "- !System.Byte 1",
                    "- !System.SByte 1",
                    "- !System.Char a",
                    "- !System.Char \"\\n\"",
                    "- !System.Char \"\\r\"",
                    "- !System.Char \"\\0\"",
                    "- !System.Char \"\\x03\"",
                    "- !System.Byte 1",
                    "- !System.SByte 1",
                    "- !!float 1",
                    "- 1.1",
                    "- -1.1",
                    "- -1.1E-10",
                    "- !System.Single 1",
                    "- !System.Single 1.1",
                    "- 1",
                    "- -1",
                    "- !System.UInt32 1",
                    "- !System.Int64 1",
                    "- !System.UInt64 1",
                    "- !System.Int16 1",
                    "- !System.UInt16 1",
                    "- null",
                    "- !System.Decimal 1",
                    "- !YamlSerializerTest.YamlRepresenterTest%2BTestEnum abc",
                    "- !YamlSerializerTest.YamlRepresenterTest%2BTestEnum あいう",
                    "- \"null\"",
                    "- \"1\"",
                    "- \"-1\"",
                    "- \"-1.0E12\"",
                    "- \"True\"",
                    @"- ""\0\a\b"+"\t"+@"\v\f\e\""/\\\N\_\L\P""",
                    "- \"abc\\n\"",
                    @"- ""abc\n\",
                    @"  \ abc""",
                    "- \"null\"",
                    "- &A !System.Object {}",
                    "- *A"
                ),
                YamlSerializer.Serialize(array1)
            );
        }

        [Test]
        public void TestArray2()
        {
            var array1 = new short[] {1, 2, 3, 4};
            Assert.AreEqual(
                BuildResult("!<!System.Int16[]> [1, 2, 3, 4]"),
                YamlSerializer.Serialize(array1)
            );

            var array2 = new decimal[] { 1, 2, 3, 4 };
            Assert.AreEqual(
                BuildResult("!<!System.Decimal[]> [1, 2, 3, 4]"),
                YamlSerializer.Serialize(array2)
            );

            var array3 = new string[] { "a", "b", "c", "d" };
            Assert.AreEqual(
                BuildResult(
                    "!<!System.String[]>",
                    "- a",
                    "- b",
                    "- c",
                    "- d"
                ),
                YamlSerializer.Serialize(array3)
            );

            var array4 = new short[2,2] { {1, 2}, {3, 4} };
            Assert.AreEqual(
                BuildResult("!<!System.Int16[,]> [[1, 2], [3, 4]]"),
                YamlSerializer.Serialize(array4)
            );

            Assert.AreEqual(2, array4.Rank);
            Assert.AreEqual(8, sizeof(double));
            var p= System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(array4, 0);
            Assert.AreEqual(1, System.Runtime.InteropServices.Marshal.ReadInt16(p));
            p = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(array4, 1);
            Assert.AreEqual(2, System.Runtime.InteropServices.Marshal.ReadInt16(p));
            p = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(array4, 2);
            Assert.AreEqual(3, System.Runtime.InteropServices.Marshal.ReadInt16(p));
            p = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(array4, 3);
            Assert.AreEqual(4, System.Runtime.InteropServices.Marshal.ReadInt16(p));
            var array5 = new short[1];
            System.Runtime.InteropServices.Marshal.Copy(p, array5, 0, 1);
            Assert.AreEqual(4, array5[0]);
            var array6 = new byte[2];
            System.Runtime.InteropServices.Marshal.Copy(p, array6, 0, 1);
            Assert.AreEqual(4, array6[0]);
            Assert.AreEqual(0, array6[1]);

            var array8 = new short[100];
            Assert.AreEqual(
                BuildResult(
                    "- !<!System.Int16[]> [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, ",
                    "  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, ",
                    "  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, ",
                    "  0, 0, 0, 0, 0, 0, 0, 0]"
                    ),
                YamlSerializer.Serialize(new object[] { array8 }) 
            );

            array8[0]=1000;
            Assert.AreEqual(
                BuildResult(
                    "- !<!System.Int16[]> [1000, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, ",
                    "  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, ",
                    "  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, ",
                    "  0, 0, 0, 0, 0, 0, 0, 0, 0]"
                    ),
                YamlSerializer.Serialize(new object[] { array8 })
            );

            var array9 = new short[10, 2];
            Assert.AreEqual(
                BuildResult(
                    "- !<!System.Int16[,]> [[0, 0], [0, 0], [0, 0], [0, 0], [0, 0], [0, 0], [0, 0], [0, 0], ",
                    "  [0, 0], [0, 0]]"
                    ),
                YamlSerializer.Serialize(new object[] { array9 })
            );

            var array10 = new short[5, 2, 2];
            Assert.AreEqual(
                BuildResult(
                    "- !<!System.Int16[,,]> [[[0, 0], [0, 0]], [[0, 0], [0, 0]], [[0, 0], [0, 0]], [[0, 0], ",
                    "  [0, 0]], [[0, 0], [0, 0]]]"
                    ),
                YamlSerializer.Serialize(new object[] { array10 })
            );
        }

        public struct TestStruct1
        {
            public int a;
            public double b;
            public string c;
            string d;
            [DefaultValue(0)]
            public int e;

            string PropD { get { return d; } set { d = value; } }
        }
        [Test]
        public void TestStruct()
        {
            var s1 = new TestStruct1();
            Assert.AreEqual(
                BuildResult(
                "!YamlSerializerTest.YamlRepresenterTest%2BTestStruct1",
                "c: null",
                "b: 0",
                "a: 0"
                ),
                YamlSerializer.Serialize(s1)
            );

            s1.a = 2;
            s1.b = 1.2;
            s1.c = "1";
            Assert.AreEqual(
                BuildResult(
                "!YamlSerializerTest.YamlRepresenterTest%2BTestStruct1",
                "c: \"1\"",
                "b: 1.2",
                "a: 2"
                ),
                YamlSerializer.Serialize(s1)
            );

            s1.e = 1;
            Assert.AreEqual(
                BuildResult(
                "!YamlSerializerTest.YamlRepresenterTest%2BTestStruct1",
                "e: 1",
                "c: \"1\"",
                "b: 1.2",
                "a: 2"
                ),
                YamlSerializer.Serialize(s1)
            );

        }

        public class TestClass1
        {
            [DefaultValue(0)]
            public int a { get; set; }
            [DefaultValue(0.0)] // (0.0).Equals(0) == true but ((object)0.0).Equals(0) == false
            public double b { get; set; }
            [DefaultValue(null)]
            public string c { get; set; }
        }

        public class TestClass2
        {
            public List<TestClass1> Items { get; set; }
            public TestClass2()
            {
                Items = new List<TestClass1>();
            }
        }

        [Test]
        public void TestClass()
        {
            var c2 = new TestClass2();
            Assert.AreEqual(
                BuildResult(
                "!YamlSerializerTest.YamlRepresenterTest%2BTestClass2",
                "Items: ",
                "  Capacity: 0"
                ),
                YamlSerializer.Serialize(c2)
            );

            c2.Items.Add(new TestClass1());
            Assert.AreEqual(
                BuildResult(
                "!YamlSerializerTest.YamlRepresenterTest%2BTestClass2",
                "Items: ",
                "  Capacity: 4",
                "  ICollection.Items: ",
                "    - {}"
                ),
                YamlSerializer.Serialize(c2)
            );

            c2.Items[0].a = 1;
            Assert.AreEqual(
                BuildResult(
                "!YamlSerializerTest.YamlRepresenterTest%2BTestClass2",
                "Items: ",
                "  Capacity: 4",
                "  ICollection.Items: ",
                "    - a: 1"
                ),
                YamlSerializer.Serialize(c2)
            );

            // Identical values in different numeric types can not be compared 
            // after being boxed.
            Assert.IsTrue(( 0.0 ).Equals(0)); // double.Equals(double) converts int to double before comparison
            Assert.IsFalse(0.0.Equals((object)0));
            Assert.IsFalse(( (object)0.0 ).Equals(0)); // !!
            Assert.IsFalse(( (object)(float)0.0 ).Equals(0.0)); // !!
            Assert.IsFalse( ( (object)0.0 ) == ((object)0)); // !!
            Assert.IsFalse(ValueType.Equals((object)0.0, (object)0)); // !!
            Assert.IsFalse(ValueType.Equals((object)0.0, (object)(float)0)); // !!
            Assert.IsFalse(( (ValueType)0.0 ) == ( (ValueType)0 )); // !!
            Assert.IsFalse(( (ValueType)0.0 ) == ( (ValueType)0 )); // !!
            Assert.IsFalse(Math.Equals((object)0, (object)0.0)); // !!!!
            Assert.IsFalse(TypeDescriptor.Equals((object)0.0,(object)0));
//            Assert.Throws<ArgumentException>(()=> 0.0.CompareTo( (object)0));
//            Assert.Throws<InvalidCastException>(()=> ((double)(object)0).CompareTo((double)(object)(float)0.0));
            Assert.IsTrue(0.0 == (double)(int)(object)0);
            Assert.IsTrue(0.0 == (double)(decimal)(object)(decimal)0);
            Assert.IsTrue(0 == (int)0.0);
            Assert.IsFalse(0 == (int)(double)(object)double.NaN); // !!
            object nan = double.NaN;
            object doubleNan = (double)nan;
//            Assert.Throws<InvalidCastException>(() => 0.CompareTo((int)doubleNan)); // !!
            Assert.IsFalse(double.NaN == double.NaN);



            Assert.IsTrue(typeof(bool).IsPrimitive);
            Assert.IsTrue(typeof(bool).IsValueType);
            Assert.IsFalse(typeof(bool).IsPointer);
            Assert.IsTrue(typeof(bool).IsSubclassOf(typeof(ValueType)));
            Assert.IsTrue(typeof(bool).IsSerializable);
            Assert.IsFalse(typeof(bool).IsClass);

            Assert.IsTrue(typeof(char).IsPrimitive);
            Assert.IsTrue(typeof(char).IsValueType);
            Assert.IsFalse(typeof(char).IsPointer);
            Assert.IsTrue(typeof(char).IsSubclassOf(typeof(ValueType)));
            Assert.IsTrue(typeof(char).IsSerializable);
            Assert.IsFalse(typeof(char).IsClass);

            Assert.IsTrue(typeof(int).IsPrimitive);
            Assert.IsTrue(typeof(int).IsValueType);
            Assert.IsFalse(typeof(int).IsPointer);
            Assert.IsTrue(typeof(int).IsSubclassOf(typeof(ValueType)));
            Assert.IsTrue(typeof(int).IsSerializable);
            Assert.IsFalse(typeof(int).IsClass);

            Assert.IsTrue(typeof(double).IsPrimitive);
            Assert.IsTrue(typeof(double).IsValueType);
            Assert.IsFalse(typeof(double).IsPointer);
            Assert.IsTrue(typeof(double).IsSubclassOf(typeof(ValueType)));
            Assert.IsTrue(typeof(double).IsSerializable);
            Assert.IsFalse(typeof(double).IsClass);

            Assert.IsFalse(typeof(void*).IsPrimitive);
            Assert.IsFalse(typeof(void*).IsValueType);
            Assert.IsTrue(typeof(void*).IsPointer);
            Assert.IsFalse(typeof(void*).IsSubclassOf(typeof(ValueType)));
            Assert.IsFalse(typeof(void*).IsSerializable);
            Assert.IsTrue(typeof(void*).IsClass);

            Assert.IsTrue(typeof(IntPtr).IsPrimitive);
            Assert.IsTrue(typeof(IntPtr).IsValueType);
            Assert.IsFalse(typeof(IntPtr).IsPointer); // !
            Assert.IsTrue(typeof(IntPtr).IsSubclassOf(typeof(ValueType)));
            Assert.IsTrue(typeof(IntPtr).IsSerializable); // !
            Assert.IsFalse(typeof(IntPtr).IsClass);

            Assert.IsFalse(typeof(Test1).IsPrimitive);
            Assert.IsTrue(typeof(Test1).IsValueType);
            Assert.IsFalse(typeof(Test1).IsPointer); 
            Assert.IsTrue(typeof(Test1).IsSubclassOf(typeof(ValueType)));
            Assert.IsFalse(typeof(Test1).IsSerializable); // !
            Assert.IsFalse(typeof(Test1).IsClass);

            Assert.IsFalse(typeof(string).IsPrimitive);
            Assert.IsFalse(typeof(string).IsValueType);
            Assert.IsFalse(typeof(string).IsPointer);
            Assert.IsFalse(typeof(string).IsSubclassOf(typeof(ValueType)));
            Assert.IsTrue(typeof(string).IsSerializable);
            Assert.IsTrue(typeof(string).IsClass);

            Assert.IsFalse(typeof(decimal).IsPrimitive);
            Assert.IsTrue(typeof(decimal).IsValueType);
            Assert.IsFalse(typeof(decimal).IsPointer);
            Assert.IsTrue(typeof(decimal).IsSubclassOf(typeof(ValueType)));
            Assert.IsTrue(typeof(decimal).IsSerializable);
            Assert.IsFalse(typeof(decimal).IsClass);

            Assert.IsFalse(typeof(Enum).IsPrimitive);
            Assert.IsFalse(typeof(Enum).IsValueType);
            Assert.IsFalse(typeof(Enum).IsPointer);
            Assert.IsTrue(typeof(Enum).IsSubclassOf(typeof(ValueType)));
            Assert.IsTrue(typeof(Enum).IsSerializable);
            Assert.IsFalse(typeof(Enum).IsClass);

            Assert.IsFalse(typeof(Test2).IsPrimitive);
            Assert.IsTrue(typeof(Test2).IsValueType);
            Assert.IsFalse(typeof(Test2).IsPointer);
            Assert.IsTrue(typeof(Test2).IsSubclassOf(typeof(ValueType)));
            Assert.IsTrue(typeof(Test2).IsSerializable);
            Assert.IsFalse(typeof(Test2).IsClass);

            Assert.IsFalse(typeof(Array).IsPrimitive);
            Assert.IsFalse(typeof(Array).IsValueType);
            Assert.IsFalse(typeof(Array).IsPointer);
            Assert.IsFalse(typeof(Array).IsSubclassOf(typeof(ValueType)));
            Assert.IsTrue(typeof(Array).IsSerializable);
            Assert.IsTrue(typeof(Array).IsClass);

            Assert.IsFalse(typeof(int[]).IsPrimitive);
            Assert.IsFalse(typeof(int[]).IsValueType);
            Assert.IsFalse(typeof(int[]).IsPointer);
            Assert.IsFalse(typeof(int[]).IsSubclassOf(typeof(ValueType)));
            Assert.IsTrue(typeof(int[]).IsSerializable);
            Assert.IsTrue(typeof(int[]).IsClass);

            Assert.IsFalse(typeof(Test3).IsPrimitive);
            Assert.IsFalse(typeof(Test3).IsValueType);
            Assert.IsFalse(typeof(Test3).IsPointer);
            Assert.IsFalse(typeof(Test3).IsSubclassOf(typeof(ValueType)));
            Assert.IsFalse(typeof(Test3).IsSerializable);
            Assert.IsTrue(typeof(Test3).IsClass);

            Assert.IsFalse(typeof(Test4).IsNotPublic);

            ShowTypeProperties(typeof(int));
            ShowTypeProperties(typeof(IntPtr));
            ShowTypeProperties(typeof(void*));
            ShowTypeProperties(typeof(decimal));
            ShowTypeProperties(typeof(Test1));
            ShowTypeProperties(typeof(Enum));
            ShowTypeProperties(typeof(Test2));
            ShowTypeProperties(typeof(string));
            ShowTypeProperties(typeof(Array));
            ShowTypeProperties(typeof(int[]));
            ShowTypeProperties(typeof(string[]));
            ShowTypeProperties(typeof(Test3));
            ShowTypeProperties(typeof(Test4));
        }

        public void ShowTypeProperties(Type type)
        {
            Debug.Write("|" + type.Name + "\t|");
            Debug.Write(type.IsAbstract + "|");
            Debug.Write(type.IsAnsiClass + "|"); // True
            Debug.Write(type.IsArray + "|");
            Debug.Write(type.IsAutoClass + "|"); // False
            Debug.Write(type.IsAutoLayout + "|");
            Debug.Write(type.IsByRef + "|"); // False
            Debug.Write(type.IsClass + "|");
            Debug.Write(type.IsCOMObject + "|"); // False
            Debug.Write(type.IsContextful + "|"); // False
            Debug.Write(type.IsEnum + "|");
            Debug.Write(type.IsExplicitLayout + "|"); // False
            Debug.Write(type.IsLayoutSequential + "|");
            Debug.Write(type.IsMarshalByRef + "|"); // False
            Debug.Write(type.IsNested + "|");
            Debug.Write(type.IsNestedAssembly + "|");
            Debug.Write(type.IsNestedFamANDAssem + "|");
            Debug.Write(type.IsNestedFamily + "|");
            Debug.Write(type.IsNestedFamORAssem + "|");
            Debug.Write(type.IsNestedPrivate + "|");
            Debug.Write(type.IsNestedPublic + "|");
            Debug.Write(type.IsNotPublic + "|");
            Debug.Write(type.IsPointer + "|");
            Debug.Write(type.IsPrimitive + "|");
            Debug.Write(type.IsPublic + "|");
            Debug.Write(type.IsSealed + "|");
            Debug.Write(type.IsSerializable + "|");
            Debug.Write(type.IsSpecialName + "|"); // False
            Debug.Write(type.IsUnicodeClass + "|"); // False
            Debug.Write(type.IsValueType + "|");
            Debug.Write(type.IsSubclassOf(typeof(ValueType)) + "|");
            Debug.Write(type.IsVisible + "|");
            Debug.WriteLine("");
        }

        public void ShowTypeProperties2(Type type)
        {
            Debug.Write("|" + type.FullName + "\t|");
            Debug.Write(type.IsNested + "|");
            Debug.Write(type.IsNestedAssembly + "|");
            Debug.Write(type.IsNestedFamANDAssem + "|");
            Debug.Write(type.IsNestedFamily + "|");
            Debug.Write(type.IsNestedFamORAssem + "|");
            Debug.Write(type.IsNestedPrivate + "|");
            Debug.Write(type.IsNestedPublic + "|");
            Debug.Write(type.IsNotPublic + "|");
            Debug.Write(type.IsPublic + "|");
            Debug.WriteLine("");
        }

        public struct Test1 { }
        public enum Test2 { abc }
        public class Test3 { }
        internal class Test4 { }

        [Test]
        public void PublicTest()
        {
            ShowTypeProperties2(typeof(Public));
            ShowTypeProperties2(typeof(Public.Public_));
            ShowTypeProperties2(Public.GetPrivate());
            ShowTypeProperties2(Public.GetPrivatePublic());
            ShowTypeProperties2(Public.GetProtected());
            ShowTypeProperties2(typeof(Public.Internal));
            ShowTypeProperties2(typeof(Public.ProtectedInternal));

            ShowTypeProperties2(typeof(Internal));
            ShowTypeProperties2(typeof(Internal.Public));
            ShowTypeProperties2(Internal.GetPrivate());
            ShowTypeProperties2(Internal.GetPrivatePublic());
            ShowTypeProperties2(Internal.GetProtected());
            ShowTypeProperties2(typeof(Internal.Internal_));
            ShowTypeProperties2(typeof(Internal.ProtectedInternal));

            ShowTypeProperties2(typeof(NotSpecified));
            ShowTypeProperties2(typeof(NotSpecified.Public));
            ShowTypeProperties2(NotSpecified.GetPrivate());
            ShowTypeProperties2(NotSpecified.GetPrivatePublic());
            ShowTypeProperties2(NotSpecified.GetProtected());
            ShowTypeProperties2(typeof(NotSpecified.Internal_));
            ShowTypeProperties2(typeof(NotSpecified.ProtectedInternal));
        }

        [Test]
        public void CultureTest()
        {
            var config = new YamlConfig();
            config.Culture = new System.Globalization.CultureInfo("da-DK");
            var serializer = new YamlSerializer(config);
            object obj = new System.Drawing.PointF(1.2f, 3.1f);
            var yaml = serializer.Serialize(obj);
            Assert.AreEqual(
                BuildResult(
                    "!System.Drawing.PointF",
                    "Y: 3,1",
                    "X: 1,2"
                    ),
                yaml
                );
            var restore = serializer.Deserialize(yaml)[0];
            Assert.AreEqual(obj, restore);

            obj = new System.Drawing.Point(1, 3);
            yaml = serializer.Serialize(obj);
            Assert.AreEqual(
                BuildResult(
                    "!System.Drawing.Point 1; 3"
                    ),
                yaml
                );
            restore = serializer.Deserialize(yaml)[0];
            Assert.AreEqual(obj, restore);

            YamlNode.DefaultConfig.Culture = System.Globalization.CultureInfo.CurrentCulture;
        }
    }

    public class Public
    {
        public static Type GetPrivate() { return typeof(Private); }
        public static Type GetPrivatePublic() { return typeof(Private.Public); }
        public static Type GetProtected() { return typeof(Protected); }
        public class Public_ { }
        private class Private { public class Public { } }
        protected class Protected { }
        internal class Internal { public class Public { } }
        protected internal class ProtectedInternal { }
    }
    internal class Internal
    {
        public static Type GetPrivate() { return typeof(Private); }
        public static Type GetProtected() { return typeof(Protected); }
        public static Type GetPrivatePublic() { return typeof(Private.Public); }
        public class Public { }
        private class Private { public class Public { } }
        protected class Protected { }
        internal class Internal_ { }
        protected internal class ProtectedInternal { }
    }
    class NotSpecified
    {
        public static Type GetPrivate() { return typeof(Private); }
        public static Type GetProtected() { return typeof(Protected); }
        public static Type GetPrivatePublic() { return typeof(Private.Public); }
        public class Public { }
        private class Private { public class Public { } }
        protected class Protected { }
        internal class Internal_ { }
        protected internal class ProtectedInternal { }
    }
}
