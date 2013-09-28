using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using NUnit.Framework;
using YamlSerializer;
using YamlSerializer.Serialization;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace YamlSerializerTest
{

    [TestFixture]
    public class YamlConstructorTest
    {
        Serializer serializer;
        public YamlConstructorTest()
        {
            serializer= new Serializer();
        }

        [Test]
        public void TestReflection()
        {
            var arrayType = typeof(int[]);
            var array1= (int[])arrayType.GetConstructor(new Type[] { typeof(int) }).Invoke(new object[] { 4 });
            Assert.AreEqual(4, array1.Length);
        }

        YamlConstructor constructor = new YamlConstructor();
        YamlParser parser = new YamlParser();
        void AssertSuccessRestored(object obj)
        {
            

            var yaml = serializer.Serialize(obj);
            var nodes = parser.Parse(yaml);
            Assert.AreEqual(1, nodes.Count);
            var yamlConfig = new YamlConfig();
            yamlConfig.LookupAssemblies.Add(typeof(YamlConstructorTest).Assembly);
            var restored = constructor.NodeToObject(nodes[0], new SerializerContext(yamlConfig));
            var yaml2 = serializer.Serialize(restored);
            Assert.AreEqual(yaml, yaml2);
        }

        [Flags]
        public enum TestEnum { abc = 1, あいう = 2 }

        [YamlTypeConverter(typeof(TestStructYamlTypeConverter))]
        struct TestStructWithTypeConverter
        {
            public int a, b;
        }

        class TestStructYamlTypeConverter: IYamlTypeConverter
        {
            public object ConvertFrom(CultureInfo culture, string value)
            {
                string[] v = ( (string)value ).Split(new char[] { ',' });
                var result = new TestStructWithTypeConverter();
                result.a = int.Parse(v[0]);
                result.b = int.Parse(v[1]);
                return result;
            }

            public string ConvertTo(CultureInfo culture, object value)
            {
                return ( (TestStructWithTypeConverter)value ).a + "," + ( (TestStructWithTypeConverter)value ).b;
            }
        }

        [Test]
        public void TestScalar()
        {
            AssertSuccessRestored(null);
            AssertSuccessRestored(true);
            AssertSuccessRestored(1);
            AssertSuccessRestored((byte)1);
            AssertSuccessRestored((double)1);
            AssertSuccessRestored((decimal)1);
            AssertSuccessRestored('1');
            AssertSuccessRestored("1");
            AssertSuccessRestored(TestEnum.abc);
            AssertSuccessRestored(TestEnum.あいう);
            AssertSuccessRestored(TestEnum.abc | TestEnum.あいう);
            var s = new TestStructWithTypeConverter();
            s.a = 1;
            AssertSuccessRestored(s);
            s.b = 100;
            AssertSuccessRestored(s);
            var array = new short[1001];
            AssertSuccessRestored(array);
            array[1000] = 100;
            AssertSuccessRestored(array);
            array[0] = 100;
            AssertSuccessRestored(array);
        }

        public class TestStruct
        {
            public int a;
            public double b;
            public string c;
        }

        public class TestStruct2
        {
            public List<TestStruct> Items { get; set; }
            public TestStruct2()
            {
                Items= new List<TestStruct> ();
            }
        }

        [Test]
        public void TestArray()
        {
            var array1 = new int[] { 1, 2, 3, 4 };
            AssertSuccessRestored(array1);

            var array2 = new int[,] { {1, 2}, {3, 4} };
            AssertSuccessRestored(array2);

            var array3 = new TestStructWithTypeConverter[5];
            array3[3].a = 3;
            AssertSuccessRestored(array3);
        }

        [Test]
        public void TestMapping()
        {
            var s1 = new TestStruct();
            AssertSuccessRestored(s1);

            var s2 = new TestStruct2();
            AssertSuccessRestored(s2);
            s2.Items.Add(new TestStruct());
            AssertSuccessRestored(s2);
            s2.Items[0].a = 1;
            AssertSuccessRestored(s2);
            s2.Items[0].c = "a";
            AssertSuccessRestored(s2);
        }

        [Test]
        public void TagResolver()
        {
            var serialiser = new Serializer();
            var m = ( new Regex(@"([-+]?)([0-9]+)") ).Match("0123");
            
            Assert.AreEqual(123, serialiser.Deserialize("123")[0]);
            Assert.AreEqual(123, serializer.Deserialize("12_3")[0]);
            Assert.AreEqual(-123, serializer.Deserialize("-123")[0]);
            Assert.AreEqual(-123, serializer.Deserialize("-12_3")[0]);
            Assert.AreEqual(Convert.ToInt32("123", 8), serializer.Deserialize("0123")[0]);
            Assert.AreEqual(Convert.ToInt32("123", 8), serializer.Deserialize("012_3")[0]);
            Assert.AreEqual(-Convert.ToInt32("123", 8), serializer.Deserialize("-0123")[0]);
            Assert.AreEqual(-Convert.ToInt32("123", 8), serializer.Deserialize("-012_3")[0]);
            Assert.AreEqual(Convert.ToInt32("123", 8), serializer.Deserialize("0o123")[0]);
            Assert.AreEqual(Convert.ToInt32("123", 8), serializer.Deserialize("0o12_3")[0]);
            Assert.AreEqual(-Convert.ToInt32("123", 8), serializer.Deserialize("-0o123")[0]);
            Assert.AreEqual(-Convert.ToInt32("123", 8), serializer.Deserialize("-0o12_3")[0]);
            Assert.AreEqual(14, serializer.Deserialize("0b1110")[0]);
            Assert.AreEqual(14, serializer.Deserialize("0b11_10")[0]);
            Assert.AreEqual(-14, serializer.Deserialize("-0b1110")[0]);
            Assert.AreEqual(-14, serializer.Deserialize("-0b1_110")[0]);
            Assert.AreEqual(0xF110, serializer.Deserialize("0xF110")[0]);
            Assert.AreEqual(0x11A0, serializer.Deserialize("0x11_A0")[0]);
            Assert.AreEqual(-0x11A0, serializer.Deserialize("-0x11A0")[0]);
            Assert.AreEqual(-0x1F10, serializer.Deserialize("-0x1_F10")[0]);

            Assert.AreEqual(0.1, serializer.Deserialize("0.1")[0]);
            Assert.AreEqual(.01, serializer.Deserialize(".01")[0]);
            Assert.AreEqual(0.1e2, serializer.Deserialize("0.1E2")[0]);
            Assert.AreEqual(.1e2, serializer.Deserialize(".1E2")[0]);
            Assert.AreEqual(0.1e2, serializer.Deserialize("0.1e2")[0]);
            Assert.AreEqual(.1e2, serializer.Deserialize(".1e2")[0]);
            Assert.AreEqual(-0.1e2, serializer.Deserialize("-0.1E2")[0]);
            Assert.AreEqual(-.1e2, serializer.Deserialize("-.1E2")[0]);
            Assert.AreEqual(-0.1e2, serializer.Deserialize("-0.1e2")[0]);
            Assert.AreEqual(-.1e2, serializer.Deserialize("-.1e2")[0]);

            Assert.AreEqual(0.1e-2, serializer.Deserialize("0.1E-2")[0]);
            Assert.AreEqual(.1e-2, serializer.Deserialize(".1E-2")[0]);
            Assert.AreEqual(0.1e-2, serializer.Deserialize("0.1e-2")[0]);
            Assert.AreEqual(.1e-2, serializer.Deserialize(".1e-2")[0]);
            Assert.AreEqual(-0.1e-2, serializer.Deserialize("-0.1E-2")[0]);
            Assert.AreEqual(-.1e-2, serializer.Deserialize("-.1E-2")[0]);
            Assert.AreEqual(-0.1e-2, serializer.Deserialize("-0.1e-2")[0]);
            Assert.AreEqual(-.1e-2, serializer.Deserialize("-.1e-2")[0]);

            Assert.AreEqual(0.1e+2, serializer.Deserialize("0.1E+2")[0]);
            Assert.AreEqual(.1e+2, serializer.Deserialize(".1E+2")[0]);
            Assert.AreEqual(0.1e+2, serializer.Deserialize("0.1e+2")[0]);
            Assert.AreEqual(.1e+2, serializer.Deserialize(".1e+2")[0]);
            Assert.AreEqual(-0.1e+2, serializer.Deserialize("-0.1E+2")[0]);
            Assert.AreEqual(-.1e+2, serializer.Deserialize("-.1E+2")[0]);
            Assert.AreEqual(-0.1e+2, serializer.Deserialize("-0.1e+2")[0]);
            Assert.AreEqual(-.1e+2, serializer.Deserialize("-.1e+2")[0]);

            Assert.AreEqual(100.1e-2, serializer.Deserialize("10_0.1E-2")[0]);
            Assert.AreEqual(10.1e-2, serializer.Deserialize("10_.1E-2")[0]);
            Assert.AreEqual(100.1e-2, serializer.Deserialize("10_0.1e-2")[0]);
            Assert.AreEqual(10.1e-2, serializer.Deserialize("10_.1e-2")[0]);
            Assert.AreEqual(-100.1e-2, serializer.Deserialize("-10_0.1E-2")[0]);
            Assert.AreEqual(-10.1e-2, serializer.Deserialize("-10_.1E-2")[0]);
            Assert.AreEqual(-100.1e-2, serializer.Deserialize("-10_0.1e-2")[0]);
            Assert.AreEqual(-10.1e-2, serializer.Deserialize("-10_.1e-2")[0]);

            Assert.AreEqual(100.1e-2, serializer.Deserialize("10_0._1E-2")[0]);
            Assert.AreEqual(10.1e-2, serializer.Deserialize("10_._1E-2")[0]);
            Assert.AreEqual(100.1e-2, serializer.Deserialize("10_0._1e-2")[0]);
            Assert.AreEqual(10.1e-2, serializer.Deserialize("10_.1_e-2")[0]);
            Assert.AreEqual(-100.1e-2, serializer.Deserialize("-10_0.1_E-2")[0]);
            Assert.AreEqual(-10.1e-2, serializer.Deserialize("-10_.1_E-2")[0]);
            Assert.AreEqual(-100.1e-2, serializer.Deserialize("-10_0._1e-2")[0]);
            Assert.AreEqual(-10.1e-2, serializer.Deserialize("-10_._1e-2")[0]);

            Assert.AreEqual("-012.1", serializer.Deserialize("-012.1")[0]); // not float

            Assert.IsTrue(double.IsNaN((double)serializer.Deserialize(".nan")[0]));
            Assert.IsTrue(double.IsNaN((double)serializer.Deserialize(".NaN")[0]));
            Assert.IsTrue(double.IsNaN((double)serializer.Deserialize(".NAN")[0]));

            Assert.IsTrue(double.IsPositiveInfinity((double)serializer.Deserialize(".inf")[0]));
            Assert.IsTrue(double.IsPositiveInfinity((double)serializer.Deserialize(".Inf")[0]));
            Assert.IsTrue(double.IsPositiveInfinity((double)serializer.Deserialize(".INF")[0]));
            Assert.IsTrue(double.IsPositiveInfinity((double)serializer.Deserialize("+.inf")[0]));
            Assert.IsTrue(double.IsPositiveInfinity((double)serializer.Deserialize("+.Inf")[0]));
            Assert.IsTrue(double.IsPositiveInfinity((double)serializer.Deserialize("+.INF")[0]));
            Assert.IsTrue(double.IsNegativeInfinity((double)serializer.Deserialize("-.inf")[0]));
            Assert.IsTrue(double.IsNegativeInfinity((double)serializer.Deserialize("-.Inf")[0]));
            Assert.IsTrue(double.IsNegativeInfinity((double)serializer.Deserialize("-.INF")[0]));

            var time = DateTime.Now;
            var utctime = time.ToUniversalTime();
            Assert.AreEqual(time, utctime.ToLocalTime());

            Assert.AreEqual(new DateTime(1999, 12, 31, 0, 0, 0), serializer.Deserialize("1999-12-31")[0]);
            Assert.AreEqual(new DateTime(1999, 12, 31, 12, 00, 00), serializer.Deserialize("1999-12-31 12:00:00")[0]);
            Assert.AreEqual(new DateTime(1999, 12, 31, 12, 00, 00, 010), serializer.Deserialize("1999-12-31 12:00:00.010")[0]);
            Assert.AreEqual(new DateTime(1999, 12, 31, 12, 00, 00, 010), serializer.Deserialize("1999-12-31T12:00:00.010")[0]);
            Assert.AreEqual(new DateTime(1999, 12, 31, 21, 00, 00, 010, DateTimeKind.Utc).ToLocalTime(), serializer.Deserialize("1999-12-31 12:00:00.010-9")[0]);
            Assert.AreEqual(new DateTime(1999, 12, 31, 12, 00, 00, 010, DateTimeKind.Utc).ToLocalTime(), serializer.Deserialize("1999-12-31 12:00:00.010Z")[0]);
            Assert.AreEqual(new DateTime(1999, 12, 31, 21, 00, 00, 010, DateTimeKind.Utc).ToLocalTime(), serializer.Deserialize("1999-12-31 12:00:00.010 -9")[0]);
            Assert.AreEqual(new DateTime(1999, 12, 31, 21, 20, 00, 010, DateTimeKind.Utc).ToLocalTime(), serializer.Deserialize("1999-12-31 12:00:00.010 -9:20")[0]);
            Assert.AreEqual(new DateTime(1999, 12, 31, 3, 00, 00, 010, DateTimeKind.Utc).ToLocalTime(), serializer.Deserialize("1999-12-31 12:00:00.010 +9")[0]);
            Assert.AreEqual(new DateTime(1999, 12, 31, 2, 40, 00, 010, DateTimeKind.Utc).ToLocalTime(), serializer.Deserialize("1999-12-31 12:00:00.010 +9:20")[0]);
            Assert.AreEqual(new DateTime(1999, 12, 30, 23, 00, 00, 010, DateTimeKind.Utc).ToLocalTime(), serializer.Deserialize("1999-12-31 1:00:00.010 +2")[0]);
            Assert.AreEqual(new DateTime(2000, 1, 1, 1, 00, 00, 000, DateTimeKind.Utc).ToLocalTime(), serializer.Deserialize("1999-12-31 23:00:00 -2")[0]);

            Assert.AreEqual("1999/12/30 23:00:00", (new DateTime(1999, 12, 30, 23, 00, 00, 010)).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture));
            YamlScalar node;
            YamlNode.DefaultConfig.TagResolver.Encode(time, out node);
            var recovered = DateTime.Parse(node.Value);
            Assert.IsTrue(time - recovered < new TimeSpan(0,0,0,0,1));
            recovered = DateTime.Parse("1999-12-31T00:00:01Z");
            recovered = DateTime.Parse("1999-12-31T00:00:01+9");
            recovered = DateTime.Parse("1999-12-31T00:00:01+9:00");
            recovered = DateTime.Parse("1999-12-31T00:00:01+09");
            recovered = DateTime.Parse("1999-12-31T00:00:01 +09");
            recovered = DateTime.Parse("1999-12-31T00:00:01.123 +09");
            recovered = DateTime.Parse("1999-12-31T00:00:01.123 +3");
            Assert.IsTrue(time - (DateTime)serializer.Deserialize(serializer.Serialize(time))[0] < new TimeSpan(0, 0, 0, 0, 1));
        }

        [Test]
        public void CustomActivator()
        {
            var brush = new YamlMapping("Color", "Blue");
            brush.Tag = "!System.Drawing.SolidBrush";
            Assert.Throws<MissingMethodException>(()=>constructor.NodeToObject(brush, new SerializerContext(YamlNode.DefaultConfig)));
            var config = new YamlConfig();
            config.AddActivator<System.Drawing.SolidBrush>(() => new System.Drawing.SolidBrush(System.Drawing.Color.Black));
            Assert.AreEqual(System.Drawing.Color.Blue, ((System.Drawing.SolidBrush)constructor.NodeToObject(brush, new SerializerContext(config))).Color);
        }

        [Test]
        public void IntAndFloat()
        {
            Assert.AreEqual((float)100000, serializer.Deserialize("!System.Single 100_000")[0]);
            Assert.AreEqual((float)100000, serializer.Deserialize("!System.Decimal 100_000")[0]);
            Assert.AreEqual((Byte)10, serializer.Deserialize("!System.Byte 1_0")[0]);
            Assert.AreEqual((Byte)10, serializer.Deserialize("!System.SByte 1_0")[0]);
            Assert.AreEqual((Byte)10, serializer.Deserialize("!System.Int16 1_0")[0]);
            Assert.AreEqual((Byte)10, serializer.Deserialize("!System.UInt16 1_0")[0]);
            Assert.AreEqual((Byte)10, serializer.Deserialize("!System.Int32 1_0")[0]);
            Assert.AreEqual((Byte)10, serializer.Deserialize("!System.UInt32 1_0")[0]);
            Assert.AreEqual((Byte)10, serializer.Deserialize("!System.Int64 1_0")[0]);
            Assert.AreEqual((Byte)10, serializer.Deserialize("!System.UInt64 1_0")[0]);
        }
    }
}
