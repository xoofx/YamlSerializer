using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using System.Yaml;
using System.ComponentModel;

using System.Reflection.Emit;

namespace YamlSerializerTest
{
    [TestFixture]
    public class YamlNodeTest: YamlNodeManipulator
    {
        [Test]
        public void TestGenericList()
        {
            var list = new List<object>();
            list.Add(str("a"));
            Assert.IsTrue(list.Contains(str("a"))); // searched by content, not by reference
        }

        [Test]
        public void TestSequenceContains()
        {
            var s = seq(str("a"));

            Assert.IsTrue(s.Contains(str("a")));

            Assert.IsFalse(s.Contains(str("b")));
        }

        [Test]
        public void TestSequenceRemove()
        {
            var s = seq(str("a"));

            Assert.IsTrue(s.Contains(str("a")));

            Assert.IsFalse(s.Contains(str("b")));
        }

        [Test]
        public void TestDictionaryByRef()
        {
            var dic = new Dictionary<YamlNode, bool>(TypeUtils.EqualityComparerByRef<YamlNode>.Default);
            var a = str("a");
            var b = str("a");
            dic.Add(a, true);
            Assert.IsTrue(dic.ContainsKey(a));
            Assert.IsFalse(dic.ContainsKey(b));
        }

        [Test]
        public void TestScalarEquality()
        {
            var a = str("a");
            var b = str("a");
            Assert.AreEqual(a, b);

            a.Value = "b";
            Assert.AreNotEqual(a, b);

            a.Value = "a";
            Assert.AreEqual(a, b);

            a.Tag = "!test";
            Assert.AreNotEqual(a, b);

            b.Tag = "!test";
            Assert.AreEqual(a, b);
        }

        class TestBase
        {
            string test = "base";
            public virtual string Test()
            {
                return test;
            }
        }
        class TestChild: TestBase
        {
            string test = "child";
            public override string Test()
            {
                return test;
            }
        }

        class DummyClass: TestBase
        {
            new public string Test()
            {
                return base.Test();
            }
        }

        delegate string Call(TestBase b);

        [Test]
        public void TestCallingParentsMethod()
        {
            var c = new TestChild();
            Assert.AreEqual("child", c.Test());
            var m = typeof(TestChild).GetMethod("Test").GetBaseDefinition();
            Assert.AreEqual(typeof(TestBase), m.DeclaringType);
            Assert.AreEqual(typeof(TestBase).GetMethod("Test"), m);
            Assert.AreEqual("child", m.Invoke(c, new object[0]));
            Assert.AreEqual("child", ( (TestBase)c ).Test());

            var dm = new System.Reflection.Emit.DynamicMethod(
                "", typeof(string), new Type[] { typeof(TestBase) }, typeof(YamlNodeTest));
            System.Reflection.Emit.ILGenerator ilgen = dm.GetILGenerator();

            ilgen.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
            ilgen.Emit(System.Reflection.Emit.OpCodes.Call, typeof(TestBase).GetMethod("Test"));
            ilgen.Emit(System.Reflection.Emit.OpCodes.Ret);
            var callTest = (Call)dm.CreateDelegate(typeof(Call));

            Assert.AreEqual("base", callTest.Invoke(c));
        }

        class TestClass
        {
            public override int GetHashCode()
            {
                return 0;
            }

            public int GetObjectHashCode()
            {
                return base.GetHashCode();
            }
        }

        [Test]
        public void TestCallingObjectGetHashCode()
        {
            var dm = new DynamicMethod(
                "GetHashCodeByRef",     // name of dynamic method
                typeof(int),            // type of return value
                new Type[] { 
                    typeof(object)      // type of "this"
                },
                typeof(YamlNodeTest));  // owner

            var ilg = dm.GetILGenerator();

            ilg.Emit(OpCodes.Ldarg_0);  // push "this" to the stack
            ilg.Emit(OpCodes.Call, typeof(object).GetMethod("GetHashCode"));
            ilg.Emit(OpCodes.Ret);      // return
            var getHashCode = (Func<object, int>)dm.CreateDelegate(typeof(Func<object, int>));

            var obj = new TestClass();
            Assert.AreEqual(obj.GetObjectHashCode(), getHashCode(obj));
        }


        [Test]
        public void TestRecursiveNodesEquality1()
        {
            var a = seq(str("a"));
            a.Add(a);
            var b = seq(str("a"));
            b.Add(b);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
            // Assert.AreEqual(a, b); // this will crash NUnit
            Assert.IsTrue(a.Equals(b)); // this does not crash NUnit
        }

        [Test]
        public void TestRecursiveNodesEquality2()
        {
            var a = map();
            a.Add(a, a);
            var b = map();
            b.Add(b, b);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
            // Assert.AreEqual(a, b); // this will crash NUnit
            Assert.IsTrue(a.Equals(b)); // this does not crash NUnit
        }

        [Test]
        public void TestNodeGraphEquality1()
        {
            var a = str("a");
            var b = str("a");

            var s1 = seq(a, b);
            var s2 = seq(a, a);
            var s3 = seq(b, b);

            Assert.IsFalse(s1.Equals(s2));
            Assert.IsFalse(s1.Equals(s3));
            Assert.IsTrue(s2.Equals(s3));
        }

        [Test]
        public void TestNodeGraphEquality2()
        {
            var a1 = str("a");
            var a2 = str("a");
            var a3 = str("!dummy", "a");
            var b = str("b");

            Assert.IsTrue(a1.Equals(a2));   // different objects that have same content

            Assert.IsFalse(a1.Equals(a3));  // Tag is different
            Assert.IsFalse(a1.Equals(b));   // Value is different

            var s1 = map(a1, seq(a1, a2));
            var s2 = map(a1, seq(a2, a1));
            var s3 = map(a2, seq(a1, a2));

            Assert.IsFalse(s1.Equals(s2)); // node graph topology is different
            Assert.IsFalse(s1.Equals(s3)); // node graph topology is different
            Assert.IsTrue(s2.Equals(s3));  // different objects that have same content and topology
        }

        public static string MultiLineText(params string[] lines)
        {
            var result = "";
            foreach ( var line in lines )
                result += line + "\r\n";
            return result;
        }

        [Test]
        public void Example2_27_YAML1_2()
        {
            var invoice = new YamlMapping(
                "invoice", 34843,
                "date", new DateTime(2001, 01, 23),
                "bill-to", new YamlMapping(
                    "given", "Chris",
                    "family", "Dumars",
                    "address", new YamlMapping(
                        "lines", "458 Walkman Dr.\nSuite #292\n",
                        "city", "Royal Oak",
                        "state", "MI",
                        "postal", 48046
                        )
                    ),
                "product", new YamlSequence(
                    new YamlMapping(
                        "sku", "BL394D",
                        "quantity", 4,
                        "description", "Basketball",
                        "price", 450.00
                        ),
                    new YamlMapping(
                        "sku", "BL4438H",
                        "quantity", 1,
                        "description", "Super Hoop",
                        "price", 2392.00
                        )
                    ),
                "tax", 251.42,
                "total", 4443.52,
                "comments", "Late afternoon is best. Backup contact is Nancy Billsmer @ 338-4338."
                );
            invoice["ship-to"] = invoice["bill-to"];
            invoice.Tag = "tag:clarkevans.com,2002:invoice";
            var yaml = invoice.ToYaml();


            Assert.AreEqual(
                MultiLineText(
                    @"%YAML 1.2",
                    @"---",
                    @"!<tag:clarkevans.com,2002:invoice>",
                    @"ship-to: &A ",
                    @"  address: ",
                    @"    city: Royal Oak",
                    @"    postal: 48046",
                    @"    lines: ""458 Walkman Dr.\n\",
                    @"      Suite #292\n""",
                    @"    state: MI",
                    @"  given: Chris",
                    @"  family: Dumars",
                    @"product: ",
                    @"  - quantity: 4",
                    @"    sku: BL394D",
                    @"    price: !!float 450",
                    @"    description: Basketball",
                    @"  - quantity: 1",
                    @"    sku: BL4438H",
                    @"    price: !!float 2392",
                    @"    description: Super Hoop",
                    @"date: 2001-01-23",
                    @"tax: 251.42",
                    @"bill-to: *A",
                    @"comments: Late afternoon is best. Backup contact is Nancy Billsmer @ 338-4338.",
                    @"total: 4443.52",
                    @"invoice: 34843",
                    @"..."
                    ),
                yaml);
            Assert.AreEqual(27, invoice["invoice"].Raw);
            Assert.AreEqual(10, invoice["invoice"].Column);
            Assert.AreEqual(22, invoice["date"].Raw);
            Assert.AreEqual(7, invoice["date"].Column);
            Assert.AreEqual(4, invoice["bill-to"].Raw);
            Assert.AreEqual(10, invoice["bill-to"].Column);
            Assert.AreEqual(11, ( (YamlMapping)invoice["bill-to"] )["given"].Raw);
            Assert.AreEqual(10, ( (YamlMapping)invoice["bill-to"] )["given"].Column);
            Assert.AreEqual(4, invoice["ship-to"].Raw);
            Assert.AreEqual(10, invoice["ship-to"].Column);
        }

        [Test]
        public void MergeKey()
        {
            var map = new YamlMapping("existing", "value");
            var mergeKey = new YamlScalar("!!merge", "<<");

            map.Add(mergeKey, new YamlMapping("existing", "new value"));
            Assert.AreEqual(1, map.Count);
            Assert.IsTrue(map.ContainsKey("existing"));
            Assert.AreEqual((YamlNode)"value", map["existing"]);

            map.Add(mergeKey, new YamlMapping("not existing", "new value"));
            Assert.AreEqual(2, map.Count);
            Assert.IsTrue(map.ContainsKey("existing"));
            Assert.AreEqual((YamlNode)"value", map["existing"]);
            Assert.IsTrue(map.ContainsKey("not existing"));
            Assert.AreEqual((YamlNode)"new value", map["not existing"]);

            map.Add(mergeKey, new YamlMapping("key1", "value1", 2, 2, 3.0, 3.0));
            Assert.AreEqual(5, map.Count);
            Assert.IsTrue(map.ContainsKey("existing"));
            Assert.AreEqual((YamlNode)"value", map["existing"]);
            Assert.IsTrue(map.ContainsKey("not existing"));
            Assert.AreEqual((YamlNode)"new value", map["not existing"]);
            Assert.IsTrue(map.ContainsKey(2));
            Assert.AreEqual((YamlNode)2, map[2]);
            Assert.IsTrue(map.ContainsKey(3.0));
            Assert.AreEqual((YamlNode)3.0, map[3.0]);

            map = new YamlMapping(
                "existing", "value",
                mergeKey, new YamlMapping("not existing", "new value"));
            Assert.AreEqual(2, map.Count);
            Assert.IsTrue(map.ContainsKey("existing"));
            Assert.AreEqual((YamlNode)"value", map["existing"]);
            Assert.IsTrue(map.ContainsKey("not existing"));
            Assert.AreEqual((YamlNode)"new value", map["not existing"]);

            map = (YamlMapping)YamlNode.FromYaml(
                "key1: value1\r\n" +
                "key2: value2\r\n" +
                "<<: \r\n" +
                "  key2: value2 modified\r\n" +
                "  key3: value3\r\n" +
                "  <<: \r\n" +
                "    key4: value4\r\n" +
                "    <<: value5\r\n"+
                "key6: <<\r\n")[0];
            Assert.AreEqual(
                "%YAML 1.2\r\n" +
                "---\r\n" +
                "<<: value5\r\n" +
                "key6: <<\r\n" +
                "key3: value3\r\n" +
                "key2: value2\r\n" +
                "key4: value4\r\n" +
                "key1: value1\r\n" +
                "...\r\n",
                map.ToYaml()
                );
            Assert.IsTrue(map.ContainsKey(mergeKey));
            Assert.AreEqual(mergeKey, map["key6"]);

            map.Add(mergeKey, map); // recursive
            Assert.AreEqual(    // nothing has been changed
                "%YAML 1.2\r\n" +
                "---\r\n" +
                "<<: value5\r\n" +
                "key6: <<\r\n" +
                "key3: value3\r\n" +
                "key2: value2\r\n" +
                "key4: value4\r\n" +
                "key1: value1\r\n" +
                "...\r\n",
                map.ToYaml()
                );
            Assert.IsTrue(map.ContainsKey(mergeKey));
            Assert.AreEqual(mergeKey, map["key6"]);

        }

        [Test]
        public void TestDictionary()
        {
            var dict = new Dictionary<YamlNode, int>();
            var key = (YamlScalar)"abc";
            dict[key] = 4;
            Assert.Throws<ArgumentException>(() => dict.Add(key, 5));
            key.Value = "def";
            Assert.IsFalse(dict.ContainsKey(key));
            Assert.IsFalse(dict.Remove(key));
            Assert.IsFalse(dict.Contains(new KeyValuePair<YamlNode, int>(key, 4)));
            var entry = dict.First(e => e.Key == key);
            Assert.IsFalse(dict.Contains(entry));
            Assert.IsFalse(((ICollection<KeyValuePair<YamlNode,int>>)dict).Remove(entry));
            Assert.IsTrue(( (ICollection<YamlNode>)dict.Keys ).IsReadOnly);
        }

        [Test]
        public void TestChangingMappingKey()
        {
            var key = (YamlScalar)"def";
            var map = new YamlMapping(key, 3);
            key.Value = "ghi";
            Assert.IsTrue(map.ContainsKey(key));
            Assert.IsTrue(map.Remove(key));

            map[map] = 4;
            map.Tag = "!test";
            Assert.IsTrue(map.ContainsKey(map));
            Assert.AreEqual((YamlNode)4, map[map]);

            var map2 = new YamlMapping(key, 3);
            map2[map2] = 2;
            map2[map] = 4;
            Assert.IsTrue(map2.ContainsKey(map2));
            Assert.AreEqual((YamlNode)2, map2[map2]);
            Assert.AreEqual((YamlNode)4, map2[map]);
            map[map2] = 2;
            Assert.IsTrue(map2.ContainsKey(map2));
            Assert.AreEqual((YamlNode)2, map2[map2]);
            Assert.AreEqual((YamlNode)4, map2[map]);
            Assert.IsTrue(map.ContainsKey(map));
            Assert.AreEqual((YamlNode)4, map[map]);

            map.Tag = YamlNode.DefaultTagPrefix + "map";
            Assert.IsTrue(map2.ContainsKey(map2));
            Assert.AreEqual((YamlNode)2, map2[map2]);
            Assert.AreEqual((YamlNode)4, map2[map]);
            Assert.IsTrue(map.ContainsKey(map));
            Assert.AreEqual((YamlNode)4, map[map]);
            Assert.IsTrue(map.ContainsKey(map2));
            Assert.AreEqual((YamlNode)2, map[map2]);

            var seq = new YamlSequence(map);
            map.Add(seq, 4);
            Assert.IsTrue(map2.ContainsKey(map2));
            Assert.AreEqual((YamlNode)2, map2[map2]);
            Assert.AreEqual((YamlNode)4, map2[map]);
            Assert.IsTrue(map.ContainsKey(map));
            Assert.AreEqual((YamlNode)4, map[map]);
            Assert.IsTrue(map.ContainsKey(map2));
            Assert.AreEqual((YamlNode)2, map[map2]);
            seq.Add(map);
            Assert.IsTrue(map2.ContainsKey(map2));
            Assert.AreEqual((YamlNode)2, map2[map2]);
            Assert.AreEqual((YamlNode)4, map2[map]);
            Assert.IsTrue(map.ContainsKey(map));
            Assert.AreEqual((YamlNode)4, map[map]);
            Assert.IsTrue(map.ContainsKey(map2));
            Assert.AreEqual((YamlNode)2, map[map2]);
        }

        [Test]
        public void TestScalarEqualityWithDifferentExpression()
        {
            var seq = (YamlSequence)YamlNode.FromYaml(
                "- 0x01\n" +
                "- 0b01\n" +
                "- 1\n" +
                "- 01\n" +
                "- 0o1\n" 
                )[0];
            Assert.AreEqual(seq[0], seq[1]);
            Assert.AreEqual(seq[0], seq[2]);
            Assert.AreEqual(seq[0], seq[3]);
            Assert.AreEqual(seq[0], seq[4]);

            seq = (YamlSequence)YamlNode.FromYaml(
                "- !!float 10\n" +
                "- +10.0\n" +
                "- 10.0\n" +
                "- 1e1\n" +
                "- 0.1e2\n"+
                "- 0.11e2\n"
                )[0];
            Assert.AreEqual(seq[0], seq[1]);
            Assert.AreEqual(seq[0], seq[2]);
            Assert.AreEqual(seq[0], seq[3]);
            Assert.AreEqual(seq[0], seq[4]);
            Assert.AreNotEqual(seq[0], seq[5]);

            seq = (YamlSequence)YamlNode.FromYaml(
                "- y\n" +
                "- Y\n" +
                "- yes\n" +
                "- Yes\n" +
                "- YES\n" +
                "- true\n" +
                "- True\n" +
                "- TRUE\n" +
                "- on\n" +
                "- On\n" +
                "- ON\n"
                )[0];
            Assert.AreEqual(seq[0], seq[1]);
            Assert.AreEqual(seq[0], seq[2]);
            Assert.AreEqual(seq[0], seq[3]);
            Assert.AreEqual(seq[0], seq[4]);
            Assert.AreEqual(seq[0], seq[5]);
            Assert.AreEqual(seq[0], seq[6]);
            Assert.AreEqual(seq[0], seq[7]);
            Assert.AreEqual(seq[0], seq[8]);
            Assert.AreEqual(seq[0], seq[9]);
            Assert.AreEqual(seq[0], seq[10]);

            seq = (YamlSequence)YamlNode.FromYaml(
                "- n\n" +
                "- N\n" +
                "- no\n" +
                "- No\n" +
                "- NO\n" +
                "- false\n" +
                "- False\n" +
                "- FALSE\n" +
                "- off\n" +
                "- Off\n" +
                "- OFF\n"
                )[0];
            Assert.AreEqual(seq[0], seq[1]);
            Assert.AreEqual(seq[0], seq[2]);
            Assert.AreEqual(seq[0], seq[3]);
            Assert.AreEqual(seq[0], seq[4]);
            Assert.AreEqual(seq[0], seq[5]);
            Assert.AreEqual(seq[0], seq[6]);
            Assert.AreEqual(seq[0], seq[7]);
            Assert.AreEqual(seq[0], seq[8]);
            Assert.AreEqual(seq[0], seq[9]);
            Assert.AreEqual(seq[0], seq[10]);

            seq = (YamlSequence)YamlNode.FromYaml(
                "- \n" +
                "- ~\n" +
                "- null\n" +
                "- Null\n" +
                "- NULL\n"
                )[0];
            Assert.AreEqual(seq[0], seq[1]);
            Assert.AreEqual(seq[0], seq[2]);
            Assert.AreEqual(seq[0], seq[3]);
            Assert.AreEqual(seq[0], seq[4]);

            seq = (YamlSequence)YamlNode.FromYaml(
                "- 2000-01-01 00:00:00Z\n" +
                "- 2000-01-01 09:00:00 +9\n" +
                "- 1999-12-31 23:00:00 -1\n" +
                "- 2000-01-01 00:00:00\n"
                )[0];
            Assert.AreEqual(seq[0], seq[1]);
            Assert.AreEqual(seq[0], seq[2]);
            Assert.AreNotEqual(seq[0], seq[3]); // how it should be
            
        }
    }
}
