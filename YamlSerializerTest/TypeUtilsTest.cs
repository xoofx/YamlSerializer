using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using System.Yaml;
using System.ComponentModel;

namespace YamlSerializerTest
{

    [TestFixture]
    class TypeUtilsTest
    {
#pragma warning disable 169, 649
        enum TestEnum { abc };
        struct TestStruct1
        {
            int a;
            int b;
            double c;
            TestEnum d;
        }
        struct TestStruct2 { string a; }
        struct TestStruct3 { IntPtr a; }
        struct TestStruct4 { TestStruct1 a; }
        struct TestStruct5 { TestStruct2 a; }
#pragma warning restore 169, 649

        [Test]
        public void TestIsPureValueType()
        {
            Assert.IsTrue(TypeUtils.IsPureValueType(typeof(TestStruct1)));
            Assert.IsFalse(TypeUtils.IsPureValueType(typeof(TestStruct2)));
            Assert.IsFalse(TypeUtils.IsPureValueType(typeof(TestStruct3)));
            Assert.IsTrue(TypeUtils.IsPureValueType(typeof(TestStruct4)));
            Assert.IsFalse(TypeUtils.IsPureValueType(typeof(TestStruct5)));
        }

        [Test]
        public void TestAreEqual()
        {
            Assert.IsFalse((object)0 == (object)0); // !!
            Assert.IsTrue(TypeUtils.AreEqual((object)0, (object)0));
            Assert.IsTrue(TypeUtils.AreEqual(null, null));
            Assert.IsFalse(TypeUtils.AreEqual(null, (object)0));
            Assert.IsFalse(TypeUtils.AreEqual((object)0, null));
            Assert.IsFalse(TypeUtils.AreEqual((object)0, (object)0.0)); // !!
        }

        [Test]
        public void TestIsNumeric()
        {
            Assert.IsFalse(TypeUtils.IsNumeric(TestEnum.abc));
            Assert.IsTrue(TypeUtils.IsNumeric(0));
            Assert.IsTrue(TypeUtils.IsNumeric(0.0));
            Assert.IsTrue(TypeUtils.IsNumeric((decimal)0)); // !
            Assert.IsTrue(TypeUtils.IsNumeric((byte)0));
            Assert.IsTrue(TypeUtils.IsNumeric((sbyte)0));
            Assert.IsFalse(TypeUtils.IsNumeric(null));
            Assert.IsTrue(TypeUtils.IsNumeric(double.NaN));
            Assert.IsTrue(TypeUtils.IsNumeric(double.NegativeInfinity));
            Assert.IsTrue(TypeUtils.IsNumeric(double.PositiveInfinity));
            Assert.IsFalse(TypeUtils.IsNumeric((char)0)); // !
        }

        [Test]
        public void TestCastToNumericType()
        {
            Assert.AreEqual((object)0, (object)0.0);

            Assert.IsFalse( (object)0 == (object)0 );
            Assert.IsFalse(( (object)0 ).Equals((object)0.0));
            Assert.IsFalse(((object)0.0).Equals((object)0));

            Assert.IsTrue(TypeUtils.CastToNumericType(1, typeof(double)).Equals((double)1));
            Assert.IsTrue(TypeUtils.CastToNumericType(1, typeof(float)).Equals((float)1));
            Assert.IsTrue(TypeUtils.CastToNumericType(1, typeof(decimal)).Equals((decimal)1));
            Assert.IsTrue(TypeUtils.CastToNumericType(1, typeof(byte)).Equals((byte)1));
            Assert.IsTrue(TypeUtils.CastToNumericType(1, typeof(ushort)).Equals((ushort)1));
        }

        [Test]
        public void TestIsPublic()
        {
            Assert.IsTrue(TypeUtils.IsPublic(typeof(Public)));
            Assert.IsTrue(TypeUtils.IsPublic(typeof(Public.Public_)));
            Assert.IsFalse(TypeUtils.IsPublic(typeof(Public.Internal)));
            Assert.IsFalse(TypeUtils.IsPublic(typeof(Public.Internal.Public)));
            Assert.IsFalse(TypeUtils.IsPublic(typeof(Internal)));
            Assert.IsFalse(TypeUtils.IsPublic(typeof(Internal.Public)));
        }

        class TestClass
        {
            public string text;
            public override bool Equals(object obj)
            {
                return ( obj is TestClass ) && ( (TestClass)obj ).text == text;
            }
            public override int GetHashCode()
            {
                return text.GetHashCode() * 13;
            }
            public int ObjectGetHashCode()
            {
                return base.GetHashCode();
            }
            public TestClass(string text)
            {
                this.text = text;
            }
        }

        [Test]
        public void TestEqualityComparerByRef()
        {
            var a = new TestClass("a");
            var b = new TestClass("a");
            var list = new List<TestClass>();

            var DefaultComparer = EqualityComparer<TestClass>.Default;
            var ComparerByRef = TypeUtils.EqualityComparerByRef<TestClass>.Default;

            list.Add(a);
            Assert.IsTrue(list.Contains(b));
            Assert.IsFalse(list.Contains(b, ComparerByRef));
            
            Assert.IsTrue(DefaultComparer.Equals(a, a));    // same object
            Assert.IsTrue(ComparerByRef.Equals(a, a));      // same object

            Assert.IsTrue(DefaultComparer.Equals(a, b));    // different object with same value
            Assert.IsFalse(ComparerByRef.Equals(a, b));     // different object with same value

            var dictionary = new Dictionary<TestClass, bool>(ComparerByRef);
            dictionary[a] = true;
            Assert.IsFalse(dictionary.ContainsKey(b));

            Assert.AreEqual(b.ObjectGetHashCode(), ComparerByRef.GetHashCode(b));
        }

    }
}
