using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using System.Resources;
using System.Yaml;
using YamlSerializerTest.Properties;

namespace YamlSerializerTest
{
    [TestFixture]
    public class YamlPresenterTest: YamlNodeManipulator
    {
        public static string BuildResult(params string[] lines)
        {
            var result= "%YAML 1.2\r\n---\r\n";
            foreach ( var line in lines )
                result += line + "\r\n";
            result += "...\r\n";
            return result;
        }

        YamlPresenter YamlPresenter = new YamlPresenter();

        [Test]
        public void TestScalars()
        {
            Assert.AreEqual(
                BuildResult("\"1\": \"1\""),
                YamlPresenter.ToYaml(map(str("1"),str("1")))
                );
            Assert.AreEqual(
                BuildResult("abc"),
                YamlPresenter.ToYaml(str("abc"))
                );
            Assert.AreEqual(
                BuildResult("\"123\""),
                YamlPresenter.ToYaml(str("123"))
                );
            Assert.AreEqual(
                BuildResult("\"12.3\""),
                YamlPresenter.ToYaml(str("12.3"))
                );
            Assert.AreEqual(
                BuildResult("1.2.3"),
                YamlPresenter.ToYaml(str("1.2.3"))
                );
            Assert.AreEqual(
                BuildResult(@"""1\n\", @"2\n\", @"3"""),
                YamlPresenter.ToYaml(str("1\n2\n3"))
                );
            Assert.AreEqual(
                BuildResult(@"""1\r\", @"2\n\", @"3\r\n"""),
                YamlPresenter.ToYaml(str("1\r2\n3\r\n"))
                );
            Assert.AreEqual(
                BuildResult(@"""1\n\", @"2\r\", @"3\r\n\", @"\r\n"""),
                YamlPresenter.ToYaml(str("1\n2\r3\r\n\r\n"))
                );
            Assert.AreEqual(
                BuildResult(@"""\0\L\P"""),
                YamlPresenter.ToYaml(str("\0\u2028\u2029"))
                );
            Assert.AreEqual(
                BuildResult("\"abc\\n\""),
                YamlPresenter.ToYaml(str("abc\n"))
                );
            Assert.AreEqual(
                BuildResult("123"),
                YamlPresenter.ToYaml(str("!!int", "123"))
                );
            Assert.AreEqual(
                BuildResult("!!float 123"),
                YamlPresenter.ToYaml(str("!!float", "123"))
                );
            Assert.AreEqual(
                BuildResult("123.2"),
                YamlPresenter.ToYaml(str("!!float", "123.2"))
                );
            Assert.AreEqual(
                BuildResult("\"\""),
                YamlPresenter.ToYaml(str(""))
                );
            Assert.AreEqual(
                BuildResult("null"),
                YamlPresenter.ToYaml(str("!!null", "null"))
                );
            Assert.AreEqual(
                BuildResult("\"null\""),
                YamlPresenter.ToYaml(str("null"))
                );
            Assert.AreEqual(
                BuildResult("true"),
                YamlPresenter.ToYaml(str("!!bool", "true"))
                );
        }

        [Test]
        public void TestNullScalar()
        {
            YamlNode node= str("!!null", "null");
            Assert.AreEqual(
                BuildResult("null"),
                YamlPresenter.ToYaml(node)
                );
            Assert.AreEqual(YamlNode.FromYaml(node.ToYaml())[0], node);

            node = str("!!null", "");
            Assert.AreEqual(
                BuildResult(""),
                YamlPresenter.ToYaml(node)
                );
            Assert.AreEqual(YamlNode.FromYaml(node.ToYaml())[0], node);

            node = seq(str("!!null", ""));
            Assert.AreEqual(
                BuildResult("- "),
                YamlPresenter.ToYaml(node)
                );
            Assert.AreEqual(YamlNode.FromYaml(node.ToYaml())[0], node);

            node = map(str("!!null", ""), str("!!null", ""));
            Assert.AreEqual(
                BuildResult(": "),
                YamlPresenter.ToYaml(node)
                );
            Assert.AreEqual(YamlNode.FromYaml(node.ToYaml())[0], node);

            node = map(str("!!null", ""), str("!!null", ""), "abc", "");
            Assert.AreEqual(
                BuildResult(
                    "abc: \"\"",
                    ": "
                    ),
                YamlPresenter.ToYaml(node)
                );
            Assert.AreEqual(YamlNode.FromYaml(node.ToYaml())[0], node);

            node = seq(map(str("!!null", ""), str("!!null", ""), "abc", ""));
            Assert.AreEqual(
                BuildResult(
                    "- abc: \"\"",
                    "  : "
                    ),
                YamlPresenter.ToYaml(node)
                );
            Assert.AreEqual(YamlNode.FromYaml(node.ToYaml())[0], node);

        }

        [Test]
        public void TestScalars2()
        {
            Assert.AreEqual(
                BuildResult("\"- aa\""),
                YamlPresenter.ToYaml(str("- aa"))
                );
            Assert.AreEqual(
                BuildResult("-aa"),
                YamlPresenter.ToYaml(str("-aa"))
                );
            Assert.AreEqual(
                BuildResult("\"!aa\""),
                YamlPresenter.ToYaml(str("!aa"))
                );
            Assert.AreEqual(
                BuildResult("\"&aa\""),
                YamlPresenter.ToYaml(str("&aa"))
                );
            Assert.AreEqual(
                BuildResult("\"@aa\""),
                YamlPresenter.ToYaml(str("@aa"))
                );
            Assert.AreEqual(
                BuildResult("\"`aa\""),
                YamlPresenter.ToYaml(str("`aa"))
                );
            Assert.AreEqual(
                BuildResult("\"'aa\""),
                YamlPresenter.ToYaml(str("'aa"))
                );
            Assert.AreEqual(
                BuildResult("\"\\\"aa\""),
                YamlPresenter.ToYaml(str("\"aa"))
                );
            Assert.AreEqual(
                BuildResult("\"}aa\""),
                YamlPresenter.ToYaml(str("}aa"))
                );
            Assert.AreEqual(
                BuildResult("\"]aa\""),
                YamlPresenter.ToYaml(str("]aa"))
                );
            Assert.AreEqual(
                BuildResult("\"[aa\""),
                YamlPresenter.ToYaml(str("[aa"))
                );
            Assert.AreEqual(
                BuildResult("\"{aa\""),
                YamlPresenter.ToYaml(str("{aa"))
                );
            Assert.AreEqual(
                BuildResult("\" aa\""),
                YamlPresenter.ToYaml(str(" aa"))
                );
            Assert.AreEqual(
                BuildResult("\"aa \""),
                YamlPresenter.ToYaml(str("aa "))
                );
            Assert.AreEqual(
                BuildResult("\"|aa\""),
                YamlPresenter.ToYaml(str("|aa"))
                );
            Assert.AreEqual(
                BuildResult("\">aa\""),
                YamlPresenter.ToYaml(str(">aa"))
                );
            Assert.AreEqual(
                BuildResult("\",aa\""),
                YamlPresenter.ToYaml(str(",aa"))
                );
            Assert.AreEqual(
                BuildResult("\"#aa\""),
                YamlPresenter.ToYaml(str("#aa"))
                );
            Assert.AreEqual(
                BuildResult("\"aa #adsfad\""),
                YamlPresenter.ToYaml(str("aa #adsfad"))
                );
            Assert.AreEqual(
                BuildResult("\"aa: adsfad\""),
                YamlPresenter.ToYaml(str("aa: adsfad"))
                );
            Assert.AreEqual( // Todo: contextful judgement of CanBePlainText
                BuildResult(@"a ,[]{}#&*:!|>'""%@`d"),
                YamlPresenter.ToYaml(str(@"a ,[]{}#&*:!|>'""%@`d"))
                );
        }

        [Test]
        public void TestSequences()
        {
            Assert.AreEqual(
                BuildResult(
                    "- abc",
                    "- def",
                    @"- ""ghi\r\n\",
                    "  jkl\"",
                    "- \"123\"",
                    "- 123",
                    "- !!float 123",
                    "- ",
                    "- - a",
                    "  - b",                                                     
                    "  - a: b"
                    ),
                YamlPresenter.ToYaml(
                    seq(
                        str("abc"),
                        str("def"),
                        str("ghi\r\njkl"),
                        str("123"),
                        str("!!int", "123"),
                        str("!!float", "123"),
                        str("!!null", ""),
                        seq(
                            str("a"),
                            str("b"),
                            map(str("a"),str("b"))
                        )
                    )
                )
            );
        }

        [Test]
        public void TestMaps()
        {
            Assert.AreEqual(
                BuildResult(
                    "? a12345678901234567890123456789012345678901234567890123456789012345678901234567890", // too long to be implicit key
                    ": \"1\"",
                    "? !<!SomeType>",
                    "  - a",
                    "  - b",
                    "  - c",
                    ": !<!some>",
                    "  !<!ab> abc: b",
                    "\"@\": ",
                    "  c: ",
                    "    - b",
                    "    - c",
                    "  a: b",
                    "\"1\": 3.3",
                    "abc: def",
                    "? - a",
                    "  - b",
                    ": b"
                ),
                YamlPresenter.ToYaml(
                    map(
                        str("abc"), str("def"),
                        seq( str("a"), str("b") ), str("b"),
                        str("1"), str("!!float", "3.3"),
                        str("@"), map(
                                str("a"), str("b"),
                                str("c"), seq(str("b"), str("c"))
                            ),
                        seq_tag("!SomeType", str("a"), str("b"), str("c")), map_tag("!some", str("!ab", "abc"), str("b")),
                        str("a12345678901234567890123456789012345678901234567890123456789012345678901234567890"), str("1")
                    )
                )
            );
        }

        YamlScalar str(string tag, string value, string expect)
        {
            var s = str(tag, value);
            s.Properties["expectedTag"] = expect;
            return s;
        }

        [Test]
        public void TestScalarTags()
        {
            Assert.AreEqual(
                BuildResult("!<!Enum> one"),
                YamlPresenter.ToYaml(str("!Enum", "one"))
            );

            Assert.AreEqual(
                BuildResult("one"),
                YamlPresenter.ToYaml(str("!Enum", "one", "!Enum"))
            );

            Assert.AreEqual(
                BuildResult("1"),
                YamlPresenter.ToYaml(str("!!int", "1", "!!int"))
            );

            Assert.AreEqual(
                BuildResult("!<!actual> abc"),
                YamlPresenter.ToYaml(str("!actual", "abc", "!base"))
            );

            Assert.AreEqual(
                BuildResult("abc"),
                YamlPresenter.ToYaml(str("!actual", "abc", "!actual"))
            );

            Assert.AreEqual(
                BuildResult(""),
                YamlPresenter.ToYaml(str("!!null", "", "!actual"))
            );

        }

        [Test]
        public void TestNextAnchor()
        {
            Assert.AreEqual("A", YamlPresenter.NextAnchor(""));
            Assert.AreEqual("B", YamlPresenter.NextAnchor("A"));
            Assert.AreEqual("C", YamlPresenter.NextAnchor("B"));
            Assert.AreEqual("D", YamlPresenter.NextAnchor("C"));
            Assert.AreEqual("AA", YamlPresenter.NextAnchor("Z"));
            Assert.AreEqual("AB", YamlPresenter.NextAnchor("AA"));
            Assert.AreEqual("AC", YamlPresenter.NextAnchor("AB"));
            Assert.AreEqual("BA", YamlPresenter.NextAnchor("AZ"));
            Assert.AreEqual("CA", YamlPresenter.NextAnchor("BZ"));
            Assert.AreEqual("AAA", YamlPresenter.NextAnchor("ZZ"));
            Assert.AreEqual("BAA", YamlPresenter.NextAnchor("AZZ"));
        }

        [Test]
        public void TestAnchorAndAlias()
        {
            var s = str("A");
            var m = map(str("bb"), seq(str("a"), str("b")),
                        str("b"), str("BB"));
            m.Add(s, m);
            Assert.AreEqual(
                BuildResult(
                    "- &A A",
                    "- &B ",
                    "  *A: *B", // recursive
                    "  b: BB",
                    "  bb: ",
                    "    - a",
                    "    - b",
                    "- *A",
                    "- - A",
                    "  - *A",
                    "- *B"
                    ),
                seq(s, 
                    m,
                    s,
                    seq(str("A"), s),
                    m
                ).ToYaml()
            );

        }

        [Test]
        public void TestPerformanceOfMemoryStream()
        {
            const int n= 1000000;

            // how to use memory stream
            var ms = new System.IO.MemoryStream();
            var tw = new System.IO.StreamWriter(ms);
            tw.WriteLine("abc");
            tw.WriteLine("abc");
            tw.Close();
            ms.Close();
            Assert.AreEqual("abc\r\nabc\r\n", UTF8Encoding.UTF8.GetString(ms.ToArray()));

            // performance
            ms = new System.IO.MemoryStream();
            tw = new System.IO.StreamWriter(ms);

            var t = DateTime.Now;
            for ( int i = 0; i < n; i++ )
                tw.WriteLine("abc"); // not so slow
            System.Diagnostics.Debug.WriteLine(( DateTime.Now - t ).ToString());

            var sb = new StringBuilder();

            t = DateTime.Now;
            for ( int i = 0; i < n; i++ )
                sb.AppendLine("abc");
            System.Diagnostics.Debug.WriteLine(( DateTime.Now - t ).ToString());
        }
    }
}
