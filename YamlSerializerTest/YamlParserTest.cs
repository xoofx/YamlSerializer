using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using System.Resources;
using System.Yaml;
using System.Yaml.Serialization;
using YamlSerializerTest.Properties;

namespace YamlSerializerTest
{
    public abstract class YamlTestFixture: YamlNodeManipulator
    {
        public void AssertAreEqual(YamlNode expected, YamlNode result)
        {
            Assert.AreEqual(expected.GetType(), result.GetType());
            Assert.AreEqual(expected.Tag, result.Tag,
                "Tag was different between expected ({0}) and result ({1}).", expected, result);
            if ( expected is YamlScalar ) {
                var expectedScalar = (YamlScalar)expected;
                var resultScalar = (YamlScalar)result;
                Assert.AreEqual(expectedScalar.Value, resultScalar.Value);
            }
            if ( expected is YamlSequence ) {
                var expectedSequence = (YamlSequence)expected;
                var resultSequence = (YamlSequence)result;
                Assert.AreEqual(expectedSequence.Count, resultSequence.Count);
                for ( int i = 0; i < expectedSequence.Count; i++ )
                    AssertAreEqual(expectedSequence[i], resultSequence[i]);
            }
            if ( expected is YamlMapping ) {
                var expectedMapping = (YamlMapping)expected;
                var resultMapping = (YamlMapping)result;
                Assert.AreEqual(expectedMapping.Count, resultMapping.Count);
                foreach ( var entry in expectedMapping ) {
                    Assert.IsTrue(resultMapping.ContainsKey(entry.Key),
                        "Mapping node {0} does not contain a key {1}.",
                            resultMapping, entry.Key);
                    AssertAreEqual(entry.Value, resultMapping[entry.Key]);
                }
            }
        }
        public void AssertResultsWithWarnings(List<YamlNode> result, int nwarnings, params YamlNode[] expected)
        {
            AssertResultsWithoutCheckWarning(result, expected);
            var s = "";
            foreach ( var w in parser.Warnings )
                s += "   " + w + "\r\n";
            Assert.AreEqual(nwarnings, parser.Warnings.Count,
                "Number of warnings was different\r\n{0}", s);
        }
        public void AssertResultsWithWarnings(List<YamlNode> result, string warning, params YamlNode[] expected)
        {
            AssertResultsWithWarnings(result, new string[] { warning }, expected);
        }
        public void AssertResultsWithWarnings(List<YamlNode> result, string[] warnings, params YamlNode[] expected)
        {
            AssertResultsWithoutCheckWarning(result, expected);
            var s = "Expected:\r\n";
            foreach ( var w in warnings )
                s += "   " + w + "\r\n";
            s += "But was:\r\n";
            foreach ( var w in parser.Warnings )
                s += "   " + w + "\r\n";
            Assert.AreEqual(warnings.Length, parser.Warnings.Count,
                "Number of warnings was different\r\n{0}", s);
            for ( int i = 0; i < warnings.Length; i++ )
                Assert.AreEqual(warnings[i], parser.Warnings[i],
                    string.Format("Warning message #{0} was different", i));
        }
        public void AssertResultsWithoutCheckWarning(List<YamlNode> result, params YamlNode[] expected)
        {
            Assert.AreEqual(expected.Length, result.Count,
                "Number of result nodes is different");
            for ( int i = 0; i < result.Count; i++ )
                AssertAreEqual(expected[i], result[i]);
        }
        public void AssertResults(List<YamlNode> result, params YamlNode[] expected)
        {
            AssertResultsWithoutCheckWarning(result, expected);
            Assert.IsEmpty(parser.Warnings, "Parser gave warning(s):");
        }
        public void AssertParseError(Action action, string expectingErrorMessage)
        {
            try {
                action();
                Assert.Fail("ParseErrorException expected but not occurred.");
            } catch ( ParseErrorException e ) {
                Assert.AreEqual(expectingErrorMessage, e.Message.Split('\n')[1],
                    "ParseErrorException message was different.");
            }
        }
        public void AssertParseError(Action action)
        {
            try {
                action();
                Assert.Fail("ParseErrorException expected but not occurred.");
            } catch ( ParseErrorException ) {
            }
        }

        internal YamlParser parser = new YamlParser();
    }

    // http://www.yaml.org/spec/1.2/spec.html
    namespace YamlVersion1_2_20090721
    {
        [TestFixture]
        public class Chapter2_1: YamlTestFixture
        {
            // Example 2.1.  Sequence of Scalars ball players) 
            [Test] public void TestExample2_1()
            {
                AssertResults(
                    parser.Parse(Resources.Example2_1),
                    seq(
                        str("Mark McGwire"),
                        str("Sammy Sosa"),
                        str("Ken Griffey")
                    )
                );
            }

            // Example 2.2.  Mapping Scalars to Scalars (player statistics) 
            [Test] public void TestExample2_2()
            {
                AssertResults(
                    parser.Parse(Resources.Example2_2),
                    map(
                        str("hr"),  str("!!int", "65"),
                        str("avg"), str("!!float", "0.278"),
                        str("rbi"), str("!!int", "147")
                    )
                );
            }

            // Example 2.3.  Mapping Scalars to Sequences (ball clubs in each league) 
            [Test] public void TestExample2_3()
            {
                AssertResults(
                    parser.Parse(Resources.Example2_3),
                    map(
                        str("american"),
                        seq(
                            str("Boston Red Sox"),
                            str("Detroit Tigers"),
                            str("New York Yankees")
                            ),
                        str("national"),
                        seq(
                            str("New York Mets"),
                            str("Chicago Cubs"),
                            str("Atlanta Braves")
                            )
                    )
                );
            }

            // Example 2.4.  Sequence of Mappings (players’ statistics) 
            [Test] public void TestExample2_4()
            {
                AssertResults(
                    parser.Parse(Resources.Example2_4),
                    seq(
                        map(
                            str("name"), str("Mark McGwire"),
                            str("hr"), str("!!int", "65"),
                            str("avg"), str("!!float", "0.278")
                            ),
                        map(
                            str("name"), str("Sammy Sosa"),
                            str("hr"), str("!!int", "63"),
                            str("avg"), str("!!float", "0.288")
                        )
                    )
                );
            }

            // Example 2.5. Sequence of Sequences
            [Test] public void TestExample2_5()
            {
                AssertResults(
                    parser.Parse(Resources.Example2_5),
                    seq(
                        seq(str("name"), str("hr"), str("avg")),
                        seq(str("Mark McGwire"), str("!!int", "65"), str("!!float", "0.278")),
                        seq(str("Sammy Sosa"), str("!!int", "63"), str("!!float", "0.288"))
                    )
                );
            }

            // Example 2.6. Mapping of Mappings
            [Test] public void TestExample2_6()
            {
                AssertResults(
                    parser.Parse(Resources.Example2_6),
                    map(
                        str("Mark McGwire"), map(
                            str("hr"), str("!!int", "65"),
                            str("avg"), str("!!float", "0.278")
                        ),
                        str("Sammy Sosa"), map(
                            str("hr"), str("!!int", "63"),
                            str("avg"), str("!!float", "0.288")
                        )
                    )
                );
            }

        }

        [TestFixture]
        public class Chapter2_2: YamlTestFixture
        {
            // Example 2.7.  Two Documents in a Stream (each with a leading comment) 
            [Test] public void TestExample2_7()
            {
                AssertResults(
                    parser.Parse(Resources.Example2_7),
                    seq(
                        str("Mark McGwire"),
                        str("Sammy Sosa"),
                        str("Ken Griffey")
                    ),
                    seq(
                        str("Chicago Cubs"),
                        str("St Louis Cardinals")
                    )
                );

            }

            // Example 2.8.  Play by Play Feed from a Game 
            [Test] public void TestExample2_8()
            {
                AssertResults(
                    parser.Parse(Resources.Example2_8),
                    map(
                        str("time"), str("20:03:20"),
                        str("player"), str("Sammy Sosa"),
                        str("action"), str("strike (miss)")
                    ),
                    map(
                        str("time"), str("20:03:47"),
                        str("player"), str("Sammy Sosa"),
                        str("action"), str("grand slam")
                    )
                );

            }

            // Example 2.9.  Single Document with Two Comments 
            [Test] public void TestExample2_9()
            {
                AssertResults(
                    parser.Parse(Resources.Example2_9),
                    map(
                        str("hr"), seq(
                            str("Mark McGwire"), str("Sammy Sosa")
                            ),
                        str("rbi"), seq(
                            str("Sammy Sosa"), str("Ken Griffey")
                            )
                    )
                );

            }

            // Example 2.10.  Node for "Sammy Sosa" appears twice in this document 
            [Test] public void TestExample2_10()
            {
                AssertResults(
                    parser.Parse(Resources.Example2_10),
                    map(
                        str("hr"), seq(
                            str("Mark McGwire"), str("Sammy Sosa")
                        ),
                        str("rbi"), seq(
                            str("Sammy Sosa"), str("Ken Griffey")
                        )
                    )
                );

                System.Diagnostics.Debugger.Log(1, "info", Resources.Example2_10);
                var result = (YamlMapping)( parser.Parse(Resources.Example2_10)[0] );
                Assert.AreSame(
                    ( (YamlSequence)result[str("hr")] )[1],
                    ( (YamlSequence)result[str("rbi")] )[0]
                );

            }

            // Example 2.11. Mapping between Sequences
            [Test] public void TestExample2_11()
            {
                AssertResults(
                    parser.Parse(Resources.Example2_11),
                    map(
                        seq(
                            str("Detroit Tigers"), str("Chicago cubs")
                            ),
                        seq(
                            str("!!timestamp", "2001-07-23")
                            ),
                        seq(
                            str("New York Yankees"), str("Atlanta Braves")
                            ),
                        seq(
                            str("!!timestamp", "2001-07-02"), 
                            str("!!timestamp", "2001-08-12"),
                            str("!!timestamp", "2001-08-14")
                            )
                    )
                );
            }

            // Example 2.12. Compact Nested Mapping
            [Test] public void TestExample2_12()
            {
                AssertResults(
                    parser.Parse(Resources.Example2_12),
                    seq(
                        map(
                            str("item"), str("Super Hoop"),
                            str("quantity"), str("!!int", "1")
                            ),
                        map(
                            str("item"), str("Basketball"),
                            str("quantity"), str("!!int", "4")
                            ),
                        map(
                            str("item"), str("Big Shoes"),
                            str("quantity"), str("!!int", "1")
                            )
                    )
                );
            }
        }

        [TestFixture]
        public class Chapter2_3: YamlTestFixture
        {

            // Example 2.13.  In literals, newlines are preserved 
            [Test] public void TestExample2_13()
            {
                AssertResults(
                    parser.Parse(Resources.Example2_13),
                    str("\\//||\\/||\n// ||  ||__")
                );

            }

            // Example 2.14.  In the folded scalars, newlines become spaces 
            [Test] public void TestExample2_14()
            {
                AssertResults(
                    parser.Parse(Resources.Example2_14),
                    str("Mark McGwire's year was crippled by a knee injury.\n")
                );

            }

            // Example 2.15.  Folded newlines are preserved for "more indented" and blank lines 
            [Test] public void TestExample2_15()
            {
                AssertResults(
                    parser.Parse(Resources.Example2_15),
                    str(Resources.Example2_15Result.Replace("\r\n","\n"))
                );

            }

            // Example 2.16.  Indentation determines scope
            [Test] public void TestExample2_16()
            {
                AssertResults(
                    parser.Parse(Resources.Example2_16),
                    map(
                        str("name"),
                                str("Mark McGwire"),
                        str("accomplishment"),
                                str("Mark set a major league home run record in 1998.\n"),
                        str("stats"),
                                str("65 Home Runs\n0.278 Batting Average\n")
                    )
                );

            }

            // Example 2.17. Quoted Scalars
            [Test] public void TestExample2_17()
            {
                AssertResults(
                    parser.Parse(Resources.Example2_17),
                    map(
                        str("unicode"), str("Sosa did fine.\u263A"),
                        str("control"), str("\b1998\t1999\t2000\n"),
                        str("hex esc"), str("\x0d\x0a is \r\n"),
                        str("single"), str(@"""Howdy!"" he cried."),
                        str("quoted"), str(@" # Not a 'comment'."),
                        str("tie-fighter"), str(@"|\-*-/|")
                    )
                );

            }

            // Example 2.18. Multi-line Flow Scalars
            [Test] public void TestExample2_18()
            {
                AssertResults(
                    parser.Parse(Resources.Example2_18),
                    map(
                        str("plain"), str("This unquoted scalar spans many lines."),
                        str("quoted"), str("So does this quoted scalar.\n")
                    )
                );
            }
        }

        [TestFixture]
        public class Chapter5: YamlTestFixture
        {
            // Example 5.1. Byte Order Mark
            [Test] public void TestExample5_1()
            {
                AssertResults(
                    parser.Parse("\ufeff# Comment only.")
                    /* nothing */
                );

                // Valid Byte Order Mark
                Assert.DoesNotThrow(() =>
                    parser.Parse(
                        "\ufeff- Valid use of BOM\n" + // stream beginning
                        "\ufeff\ufeff...\n" +          // between "\n" and "..."
                        "\ufeff\ufeffsome text\n" +    // after "...\n"
                        "\ufeff\ufeff---\n" +          // between "\n" and "---"
                        "- Inside a stream but not in a document.\n")
                );

            }

            // Example 5.2. Invalid Byte Order Mark
            [Test] public void TestExample5_2()
            {
                AssertParseError(() =>
                    parser.Parse(
                        "- Invalid use of BOM\n" +
                        "\ufeff\n" +
                        "- Inside a document.\n"),
                    "A BOM (\\ufeff) must not appear inside a document."
                );

                // Other Invalid Char \u####
                AssertParseError(() =>
                    parser.Parse(
                        "- Invalid char appears\n" +
                        "\ufefe\n" +
                        "- Inside a document.\n"),
                    "Extra content was found. Maybe indentation was incorrect."
                );

                // Other Invalid Char \x##
                try {
                    parser.Parse(
                        "- Invalid char appears\n" +
                        "\0\n" +
                        "- Inside a document.\n");
                    Assert.Fail("ParseErrorException expected but not occurred.");
                } catch ( ParseErrorException e ) {
                    Assert.AreEqual("An irregal character '\\x00' appeared.", e.Message.Split('\n')[1]);
                }

            }

            // Example 5.3. Block Structure Indicators
            [Test] public void TestExample5_3()
            {
                AssertResults(
                    parser.Parse(Resources.Example5_3),
                    map(
                        str("sequence"), seq(
                            str("one"), str("two")
                        ),
                        str("mapping"), map(
                            str("sky"), str("blue"),
                            str("sea"), str("green")
                        )
                    )
                );

            }

            // Example 5.4. Flow Collection Indicators
            [Test] public void TestExample5_4()
            {
                AssertResults(
                    parser.Parse(Resources.Example5_4),
                    map(
                        str("sequence"), seq(
                            str("one"), str("two")
                        ),
                        str("mapping"), map(
                            str("sky"), str("blue"),
                            str("sea"), str("green")
                        )
                    )
                );

            }

            // Example 5.5. Comment Indicator
            [Test] public void TestExample5_5()
            {
                AssertResults(
                    parser.Parse(Resources.Example5_5)
                    /* nothing */
                );

            }

            // Example 5.6. Node Property Indicators
            [Test] public void TestExample5_6()
            {
                YamlNode n;
                AssertResults(
                    parser.Parse(Resources.Example5_6),
                    map(
                        str("anchored"), n = str("!local", "value"),
                        str("alias"), n
                    )
                );

            }

            // Example 5.7. Block Scalar Indicators
            [Test] public void TestExample5_7()
            {
                AssertResults(
                    parser.Parse(Resources.Example5_7),
                    map(
                        str("literal"), str("some\ntext\n"),
                        str("folded"), str("some text\n")
                    )
                );

            }

            // Example 5.8. Quoted Scalar Indicators
            [Test] public void TestExample5_8()
            {
                AssertResults(
                    parser.Parse(Resources.Example5_8),
                    map(
                        str("single"), str("text"),
                        str("double"), str("text")
                    )
                );

            }

            // Example 5.9. Directive Indicator
            [Test] public void TestExample5_9()
            {
                AssertResults(
                    parser.Parse(Resources.Example5_9),
                    str("text")
                );
            }

            // Example 5.10. Invalid use of Reserved Indicators
            [Test] public void TestExample5_10()
            {
                AssertParseError(() =>
                    parser.Parse(Resources.Example5_10),
                    "Reserved indicators '@' and '`' can't start a plain scalar."
                );

            }

            // Example 5.11. Line Break Characters
            [Test] public void TestExample5_11()
            {
                AssertResults(
                    parser.Parse(Resources.Example5_11),
                    str("Line break (no glyph)\nLine break (glyphed)\n")
                );

                // '\x0c' can not appear in YAML 1.2 document 
                // "YAML 1.2 processors parsing a version 1.1 document should therefore 
                // treat these line breaks as non-break characters, with an appropriate 
                // warning." seems invalid.
                AssertParseError(() =>
                    parser.Parse("\x0c"),
                    "An irregal character '\\x0c' appeared."
                );

                // warnings for YAML 1.1 break chars
                AssertResultsWithWarnings(
                    parser.Parse("\x85"),
                    new string[]{
                    "Warning: \\x85 is treated as non-break character unlike YAML 1.1 at line 1 column 1.",
                },
                    str("\x85")
                );

                AssertResultsWithWarnings(
                    parser.Parse("a\x85"), 1,
                    str("a\x85")
                );

                AssertResultsWithWarnings(
                    parser.Parse("|\n\x85"), 1,
                    str("\x85")
                );

                // warn one time for one char even when it repeatedly appears
                AssertResultsWithWarnings(
                    parser.Parse("a\x0085b\x85\u2028\x85"),
                    new string[]{
                    "Warning: \\x85 is treated as non-break character unlike YAML 1.1 at line 1 column 2.",
                    "Warning: \\u2028 is treated as non-break character unlike YAML 1.1 at line 1 column 5.",
                },
                    str("a\x0085b\x85\u2028\x85")
                );    // note that "a\x85b" represents "a\x085b" in C#

            }

            // Example 5.12. Tabs and Spaces
            [Test] public void TestExample5_12()
            {
                AssertResults(
                    parser.Parse(Resources.Example5_12),
                    map(
                        str("quoted"), str("Quoted \t"),
                        str("block"), str(
                            "void main() {\n" +
                            "\tprintf(\"Hello, world!\\n\");\n" +
                            "}"
                        )
                    )
                );

            }

            // Example 5.13. Escaped Characters
            [Test] public void TestExample5_13()
            {
                AssertResults(
                    parser.Parse(Resources.Example5_13),
                    str("Fun with \x5C \x22 \x07 \x08 \x1B \x0C " +
                        "\x0A \x0D \x09 \x0B \x00 \x20 \xA0 \x85 " +
                        "\u2028 \u2029 A A A")
                );
            }

            // Example 5.14. Invalid Escaped Characters
            [Test]
            public void TestExample5_14()
            {
                AssertParseError(() =>
                    parser.Parse(@"""\c"""),
                    "\\c is not a valid escape sequence.");
                AssertParseError(() =>
                    parser.Parse(@"""\xq-"""),
                    "\\xq- is not a valid escape sequence.");
                AssertParseError(() =>
                    parser.Parse(@"""\x"""),
                    "\\x is not a valid escape sequence.");
                AssertParseError(() =>
                    parser.Parse(@"""\U123"""),
                    "\\U123 is not a valid escape sequence.");
            }
        }

        [TestFixture]
        public class Chapter6: YamlTestFixture
        {
            [Test] // Example 6.1. Indentation Spaces
            public void TestExample6_1()
            {
                AssertResults(
                    parser.Parse(Resources.Example6_1),
                    map(
                        str("Not indented"),
                        map(
                            str("By one space"), str("By four\n  spaces\n"),
                            str("Flow style"), seq(
                                str("By two"),
                                str("Also by two"),
                                str("Still by two")
                            )
                        )
                    )
                );

            }

            // Example 6.2. Indentation Indicators
            [Test]
            public void TestExample6_2()
            {
                AssertResults(
                    parser.Parse(Resources.Example6_2),
                    map(
                        str("a"), seq(
                            str("b"),
                            seq(str("c"), str("d"))
                        )
                    )
                );

            }

            // Example 6.3. Separation Spaces
            [Test]
            public void TestExample6_3()
            {
                AssertResults(
                    parser.Parse(Resources.Example6_3),
                    seq(
                        map(str("foo"), str("bar")),
                        seq(str("baz"), str("baz"))
                    )
                );

            }

            // Example 6.4. Line Prefixes
            [Test]
            public void TestExample6_4()
            {
                AssertResults(
                    parser.Parse(Resources.Example6_4),
                    map(
                        str("plain"), str("text lines"),
                        str("quoted"), str("text lines"),
                        str("block"), str("text\n \tlines\n")
                    //                         ^---- line-break shuoud be kept
                    )
                );

            }

            // Example 6.5. Empty Lines
            [Test]
            public void TestExample6_5()
            {
                AssertResults(
                    parser.Parse(Resources.Example6_5),
                    map(
                        str("Folding"), str("Empty line\nas a line feed"),
                        str("Chomping"), str("Clipped empty lines\n")
                    )
                );

            }

            // Example 6.6. Line Folding
            [Test]
            public void TestExample6_6()
            {
                AssertResults(
                    parser.Parse(Resources.Example6_6),
                    str("trimmed\n\n\nas space")
                );

            }

            // Example 6.7. Block Folding
            [Test]
            public void TestExample6_7()
            {
                AssertResults(
                    parser.Parse(Resources.Example6_7),
                    str("foo \n\n\t bar\n\nbaz\n")
                );

            }

            // Example 6.8. Flow Folding
            [Test]
            public void TestExample6_8()
            {
                AssertResults(
                    parser.Parse(Resources.Example6_8),
                    str(" foo\nbar\nbaz ")
                );

            }

            // Example 6.9. Separated Comment
            [Test]
            public void TestExample6_9()
            {
                AssertResults(
                    parser.Parse(Resources.Example6_9),
                    map(str("key"), str("value"))
                );

            }

            // Example 6.10. Comment Lines
            [Test]
            public void TestExample6_10()
            {
                AssertResults(
                    parser.Parse(Resources.Example6_10)
                    /* This stream contains no documents, only comments. */
                );

            }

            // Example 6.11. Multi-Line Comments
            [Test]
            public void TestExample6_11()
            {
                AssertResults(
                    parser.Parse(Resources.Example6_11),
                    map(str("key"), str("value"))
                );

            }

            // Example 6.12. Separation Spaces
            [Test]
            public void TestExample6_12()
            {
                AssertResults(
                    parser.Parse(Resources.Example6_12),
                    map(
                        map(
                            str("first"), str("Sammy"),
                            str("last"), str("Sosa")
                        ),
                        map(
                            str("hr"), str("!!int", "65"),
                            str("avg"), str("!!float", "0.278")
                        )
                    )
                );
            }

            // Example 6.13. Reserved Directives
            [Test]
            public void TestExample6_13()
            {
                AssertResultsWithWarnings(
                    parser.Parse(Resources.Example6_13),
                    new string[]{
                    "Warning: Custom directive %FOO was ignored at line 1 column 35."
                },
                    str("foo")
                );

            }

            // Example 6.14. “YAML” directive
            [Test]
            public void TestExample6_14()
            {
                AssertResultsWithWarnings(
                    parser.Parse(Resources.Example6_14),
                    new string[]{
                    "Warning: YAML version %1.3 was specified but ignored at line 1 column 10."
                },
                    str("foo")
                );

                AssertResults(
                    parser.Parse("%YAML 1.2\n---\ntext"), // 1.2 is ok
                    str("text")
                );
            }

            // Example 6.15. Invalid Repeated YAML directive
            [Test]
            public void TestExample6_15()
            {
                AssertParseError(() =>
                    parser.Parse(Resources.Example6_15),
                    "The YAML directive must only be given at most once per document."
                );

                // It seems repeated YAML directives are allowed if a stream contains
                // multiple documents but I couldn't figure out how it can be done.
                /*
                AssertResultsWithWarnings(
                    parser.Parse("%YAML 1.2\n---\n...\n%YAML 1.1\n---\nfoo\n"),
                    2,
                    str(""),
                    str("foo")
                );
                */

            }

            // Example 6.16. "TAG" directive
            [Test]
            public void TestExample6_16()
            {
                AssertResults(
                    parser.Parse(Resources.Example6_16),
                    str("foo")
                );

            }

            // Example 6.17. Invalid Repeated TAG directive
            [Test]
            public void TestExample6_17()
            {
                AssertParseError(() =>
                    parser.Parse(Resources.Example6_17),
                    "Primary tag prefix is already defined as '!foo'."
                );

            }

            // Example 6.18. Primary Tag Handle
            [Test]
            public void TestExample6_18()
            {
                AssertResults(
                    parser.Parse(Resources.Example6_18),
                    str("!foo", "bar"),
                    str("tag:example.com,2000:app/foo", "bar")
                );

            }

            // Example 6.19. Secondary Tag Handle
            [Test]
            public void TestExample6_19()
            {
                AssertResults(
                    parser.Parse(Resources.Example6_19),
                    str("tag:example.com,2000:app/int", "1 - 3")
                );

            }

            // Example 6.20. Tag Handles
            [Test]
            public void TestExample6_20()
            {
                AssertResults(
                    parser.Parse(Resources.Example6_20),
                    str("tag:example.com,2000:app/foo", "bar")
                );

            }

            // Example 6.21. Local Tag Prefix
            [Test]
            public void TestExample6_21()
            {
                // I don't know what to do for this
                AssertResults(
                    parser.Parse(Resources.Example6_21),
                    str("!my-light", "fluorescent"),
                    str("!my-light", "green")
                );
            }

            // Example 6.22. Global Tag Prefix
            [Test]
            public void TestExample6_22()
            {
                AssertResults(
                    parser.Parse(Resources.Example6_22),
                    seq(str("tag:example.com,2000:app/foo", "bar"))
                );
            }

            // Example 6.23. Node Properties
            [Test]
            public void TestExample6_23()
            {
                YamlNode n;
                AssertResults(
                    parser.Parse(Resources.Example6_23),
                    map(
                        n = str("foo"), str("bar"),
                        str("baz"), n
                    )
                );

                var result = parser.Parse(Resources.Example6_23)[0];
                var m = result as YamlMapping;
                var foo = m[str("baz")];
                Assert.IsTrue(m.Keys.Any(key => key == foo && m[foo].Equals(str("bar"))));
            }

            // Example 6.24. Verbatim Tags
            [Test]
            public void TestExample6_24()
            {
                AssertResults(
                    parser.Parse(Resources.Example6_24),
                    map(
                        str("tag:yaml.org,2002:str", "foo"),
                        str("!bar", "baz")
                    )
                );
            }

            // Example 6.25. Invalid Verbatim Tags
            [Test]
            public void TestExample6_25()
            {
                AssertParseError(()=>
                    parser.Parse(Resources.Example6_25a),
                    "Empty local tag was found."
                );

                AssertResultsWithWarnings(
                    parser.Parse(Resources.Example6_25b),
                    "Warning: Invalid global tag name '$:?' (c.f. RFC 4151) found at line 1 column 9.",
                    seq(str("$:?", "bar"))
                );
            }

            // Example 6.26. Tag Shorthands
            [Test]
            public void TestExample6_26()
            {
                AssertResults(
                    parser.Parse(Resources.Example6_26),
                    seq(
                        str("!local", "foo"),
                        str("tag:yaml.org,2002:str", "bar"),
                        str("tag:example.com,2000:app/tag!", "baz")
                    )
                );
            }

            // Example 6.27. Invalid Tag Shorthands
            [Test]
            public void TestExample6_27()
            {
                AssertParseError(() =>
                    parser.Parse(Resources.Example6_27a),
                    "The !e! handle has no suffix."
                );
                AssertParseError(() =>
                    parser.Parse(Resources.Example6_27b),
                    "Tag handle !h! is not registered."
                );
                AssertParseError(() =>
                    parser.Parse("%TAG !e!\n"),
                    "Invalid TAG directive found."
                );
            }


            [Test] // Example 6.28. Non-Specific Tags
            public void TestExample6_28()
            {
                AssertResults(
                    parser.Parse(Resources.Example6_28),
                    seq(
                        str("!!str", "12"),
                        str("!!int", "12"),
                        str("!!str", "12")
                    )
                );
            }

            [Test] // Example 6.29. Node Anchors
            public void TestExample6_29()
            {
                AssertResults(
                    parser.Parse(Resources.Example6_29),
                    map(
                        str("First occurrence"), str("Value"),
                        str("Second occurrence"), str("Value")
                    )
                );

                var result = (YamlMapping)parser.Parse(Resources.Example6_29)[0];
                Assert.AreEqual(result[str("First occurrence")], result[str("Second occurrence")]);
            }
        }

        [TestFixture]
        public class Chapter7: YamlTestFixture
        {
            [Test] // Example 7.1. Alias Nodes
            public void TestExample7_1()
            {                         
                AssertResults(
                    parser.Parse(Resources.Example7_1),
                    map(
                        str("First occurrence"), str("Foo"),
                        str("Second occurrence"), str("Foo"),
                        str("Override anchor"), str("Bar"),
                        str("Reuse anchor"), str("Bar")
                    )
                );

                var result = (YamlMapping)parser.Parse(Resources.Example7_1)[0];
                Assert.AreEqual(
                    result[str("First occurrence")],
                    result[str("Second occurrence")]);
                Assert.AreEqual(
                    result[str("Override anchor")],
                    result[str("Reuse anchor")]);
            }

            [Test] // Example 7.2. Empty Content
            public void TestExample7_2()
            {
                AssertResults(
                    parser.Parse(Resources.Example7_2),
                    map(
                        str("foo"), str(""),
                        str(""), str("bar")
                    )
                );
            }

            [Test] // Example 7.3. Completely Empty Flow Nodes
            public void TestExample7_3()
            {                                               
                AssertResults(
                    parser.Parse(Resources.Example7_3),
                    map(
                        str("foo"), str("!!null", ""),
                        str("!!null", ""), str("bar")
                    )
                );
            }

            [Test] // Example 7.4. Double Quoted Implicit Keys
            public void TestExample7_4()
            {
                AssertResults(
                    parser.Parse(Resources.Example7_4),
                    map(
                        str("implicit key"), map(
                            str("also implicit"), str("value"),
                            str("not a implicit key"), str("another value")
                        )
                    )
                );
            }

            [Test] // Example 7.5. Double Quoted Line Breaks
            public void TestExample7_5()
            {
                AssertResults(
                    parser.Parse(Resources.Example7_5),
                    str("folded to a space,\nto a line feed, or \t \tnon-content")
                );
            }

            [Test] // Example 7.6. Double Quoted Lines
            public void TestExample7_6()
            {   
                AssertResults(
                    parser.Parse(Resources.Example7_6),
                    str(" 1st non-empty,\n2nd non-empty, 3rd non-empty ")
                );
            }

            [Test] // Example 7.7. Single Quoted Characters
            public void TestExample7_7()
            {
                AssertResults(
                    parser.Parse(Resources.Example7_7),
                    str("here's to \"quotes\"")
                );
            }

            [Test] // Example 7.8. Single Quoted Scalars
            public void TestExample7_8()
            {
                AssertResults(
                    parser.Parse(Resources.Example7_8),
                    map(
                        str("implicit key"), map(
                            str("also implicit"), str("value"),
                            str("not a implicit key"), str("another value")
                        )
                    )
                );
            }

            [Test] // Example 7.9. Single Quoted Lines
            public void TestExample7_9()
            {   
                AssertResults(
                    parser.Parse(Resources.Example7_9),
                    str(" 1st non-empty,\n2nd non-empty, 3rd non-empty ")
                );
            }

            [Test] // Example 7.10. Plain Characters
            public void TestExample7_10()
            {   
                AssertResults(
                    parser.Parse(Resources.Example7_10),
                    seq(
                        str("::vector"),
                        str(": - ()"),
                        str("Up, up, and away!"),
                        str("!!int", "-123"),
                        str("http://example.com/foo#bar"),
                        seq(
                            str("::vector"),
                            str(": - ()"),
                            str("Up, up, and away!"),
                            str("!!int", "-123"),
                            str("http://example.com/foo#bar")
                        )
                    )
                );
            }

            [Test] // Example 7.11. Plain Scalars
            public void TestExample7_11()
            {   
                AssertResults(
                    parser.Parse(Resources.Example7_11),
                    map(
                        str("implicit key"), map(
                            str("also implicit"), str("value"),
                            str("not a implicit key"), str("another value")
                        )
                    )
                );
            }

            [Test] // Example 7.12. Plain Lines
            public void TestExample7_12()
            {   
                AssertResults(
                    parser.Parse(Resources.Example7_12),
                    str("1st non-empty,\n2nd non-empty, 3rd non-empty")
                );
            }

            [Test] // Example 7.13. Flow Sequence
            public void TestExample7_13()
            {   
                AssertResults(
                    parser.Parse(Resources.Example7_13),
                    seq(
                        seq(str("one"), str("two")),
                        seq(str("three"), str("four"))
                    )                            
                );
            }

            [Test] // Example 7.14. Flow Sequence Entries
            public void TestExample7_14()
            {   
                AssertResults(
                    parser.Parse(Resources.Example7_14),
                    seq(
                        str("double quoted"),
                        str("single quoted"),
                        str("plain text"),
                        seq(
                            str("nested")
                        ),
                        map(
                            str("single"), str("pair")
                        )
                    )
                );
            }

            [Test] // Example 7.15. Flow Mappings
            public void TestExample7_15()
            {
                AssertResults(
                    parser.Parse(Resources.Example7_15),
                    seq(
                        map(
                            str("one"), str("two"),
                            str("three"), str("four")
                        ),
                        map(
                            str("five"), str("six"),
                            str("seven"), str("eight")
                        )
                    )
                );
            }

            [Test] // Example 7.16. Flow Mapping Entries
            public void TestExample7_16()
            {
                AssertResults(
                    parser.Parse(Resources.Example7_16),
                    map(
                        str("explicit"), str("entry"),
                        str("implicit"), str("entry"),
                        str("!!null", ""), str("!!null", "")
                    )
                );
            }

            [Test] // Example 7.17. Flow Mapping Separate Values
            public void TestExample7_17()
            {   
                AssertResults(
                    parser.Parse(Resources.Example7_17),
                    map(
                        str("unquoted"), str("separate"),
                        str("http://foo.com"), str("!!null", ""),
                        str("omitted"), str("!!null", ""), 
                        str("!!null", ""), str("omitted")
                    )
                );
            }

            [Test] // Example 7.18. Flow Mapping Adjacent Values
            public void TestExample7_18()
            {
                AssertResults(
                    parser.Parse(Resources.Example7_18),
                    map(
                        str("adjacent"), str("value"),
                        str("readable"), str("value"),
                        str("empty"), str("!!null", "")
                    )
                );
            }

            [Test] // Example 7.19. Single Pair Flow Mappings
            public void TestExample7_19()
            {
                AssertResults(
                    parser.Parse(Resources.Example7_19),
                    seq(
                        map(str("foo"), str("bar"))
                    )
                );
            }

            [Test] // Example 7.20. Single Pair Explicit Entry
            public void TestExample7_20()
            {
                AssertResults(
                    parser.Parse(Resources.Example7_20),
                    seq(
                        map(str("foo bar"), str("baz"))
                    )
                );
            }

            [Test] // Example 7.21. Single Pair Implicit Entries
            public void TestExample7_21()
            {
                AssertResults(
                    parser.Parse(Resources.Example7_21),
                    seq(
                        seq(map(str("YAML"), str("separate"))),
                        seq(map(str("!!null", ""), str("empty key entry"))),
                        seq(map(map(str("JSON"), str("like")), str("adjacent")))
                    )
                );
            }

            [Test] // Example 7.22. Invalid Implicit Keys
            public void TestExample7_22()
            {
                AssertParseError(() =>
                    parser.Parse(Resources.Example7_22a),
                    "Closing brace ] was not found."
                );

                AssertParseError(() =>
                    parser.Parse(Resources.Example7_22b),
                    "The implicit key was too long."
                );
            }

            [Test] // Example 7.23. Flow Content
            public void TestExample7_23()
            {
                AssertResults(
                    parser.Parse(Resources.Example7_23),
                    seq(
                        seq(str("a"), str("b")),
                        map(str("a"), str("b")),
                        str("a"),
                        str("b"),
                        str("c")
                    )
                );
            }

            [Test] // Example 7.24. Flow Nodes
            public void TestExample7_24()
            {
                AssertResults(
                    parser.Parse(Resources.Example7_24),
                    seq(
                        str("a"),
                        str("b"),
                        str("c"),
                        str("c"),
                        str("a"),
                        str("b"),
                        str("")
                    )
                );

                var result = (YamlSequence)parser.Parse(Resources.Example7_24)[0];
                Assert.AreEqual(result[2], result[3]);
            }

        }

        [TestFixture]
        public class Chapter8: YamlTestFixture
        {
            [Test] // Example 8.1. Block Scalar Header
            public void TestExample8_1()
            {
                AssertResults(
                    parser.Parse(Resources.Example8_1),
                    seq(
                        str("literal\n"),
                        str(" folded\n"),
                        str("keep\n\n"),
                        str(" strip")
                    )
                );
            }

            [Test] // Example 8.2. Block Indentation Indicator
            public void TestExample8_2()
            {   // Todo: example is wrong. Since the line " \t\n" is a spaced line, line break should be kept.
                //       this is different defect than that was reported by Brad on 2009-07-25
                AssertResults(
                    parser.Parse(Resources.Example8_2),
                    seq(
                        str("detected\n"),
                        str("\n\n# detected\n"),
                        str(" explicit\n"),
                        str("\t\ndetected\n")
                    )
                );
            }

            [Test] // Example 8.3. Invalid Block Scalar Indentation Indicators
            public void TestExample8_3()
            {
                AssertParseError(()=>
                    parser.Parse(Resources.Example8_3a),
                    "Too many indentation was found."
                );

                AssertParseError(() =>
                    parser.Parse(Resources.Example8_3b),
                    "Extra line was found. Maybe indentation was incorrect."
                );

                AssertParseError(() =>
                    parser.Parse(Resources.Example8_3c),
                    "Extra line was found. Maybe indentation was incorrect."
                );
            }

            [Test] // Example 8.4. Chomping Final Line Break
            public void TestExample8_4()
            {   
                AssertResults(
                    parser.Parse(Resources.Example8_4),
                    map(
                        str("strip"), str("text"),
                        str("clip"), str("text\n"),
                        str("keep"), str("text\n")
                    )
                );

                // Todo: should be error?
                AssertResultsWithWarnings(
                    parser.Parse("test: >+\n abc\n def\n"),
                    new string[]{ "Warning: Keep line breaks for folded text '>+' is invalid at line 2 column 1." },
                    map(str("test"),str("abc def\n"))
                );
            }

            [Test] // Example 8.5. Chomping Trailing Lines
            public void TestExample8_5()
            {   
                AssertResults(
                    parser.Parse(Resources.Example8_5),
                    map(
                        str("strip"), str("# text"),
                        str("clip"), str("# text\n"),
                        str("keep"), str("# text\n\n")
                    )
                );
            }

            [Test] // Example 8.6. Empty Scalar Chomping
            public void TestExample8_6()
            {
                AssertResults(
                    parser.Parse(Resources.Example8_6),
                    map(
                        str("strip"), str(""),
                        str("clip"), str(""),
                        str("keep"), str("\n")
                    )
                );
            }

            [Test] // Example 8.7. Literal Scalar
            public void TestExample8_7()
            {
                AssertResults(
                    parser.Parse(Resources.Example8_7),
                    str("literal\n\ttext\n")
                );
            }

            [Test] // Example 8.8. Literal Content
            public void TestExample8_8()
            {
                AssertResults(
                    parser.Parse(Resources.Example8_8),
                    str("\n\nliteral\n \n\ntext\n")
                );
            }

            [Test] // Example 8.9. Folded Scalar
            public void TestExample8_9()
            {
                AssertResults(
                    parser.Parse(Resources.Example8_9),
                    str("folded text\n")
                );
            }

            [Test] // Example 8.10. Folded Lines
            public void TestExample8_10()
            {
                AssertResults(
                    parser.Parse(Resources.Example8_10),
                    str("\nfolded line\nnext line\n  * bullet\n\n  * list\n  * lines\n\nlast line\n")
                );
            }

            [Test] // Example 8.14. Block Sequence
            public void TestExample8_14()
            {
                AssertResults(
                    parser.Parse(Resources.Example8_14),
                    map(
                        str("block sequence"), seq(
                            str("one"),
                            map(str("two"), str("three"))
                        )
                    )
                );
            }

            [Test] // Example 8.15. Block Sequence Entry Types
            public void TestExample8_15()
            {
                AssertResults(
                    parser.Parse(Resources.Example8_15),
                    seq(
                        str("!!null", ""),
                        str("block node\n"),
                        seq(
                            str("one"),
                            str("two")
                        ),
                        map(
                            str("one"),
                            str("two")
                        )
                    )
                );
            }

            [Test] // Example 8.16. Block Mappings
            public void TestExample8_16()
            {
                AssertResults(
                    parser.Parse(Resources.Example8_16),
                    map(
                        str("block mapping"), map(
                            str("key"), str("value")
                        )
                    )
                );
            }

            [Test] // Example 8.17. Explicit Block Mapping Entries
            public void TestExample8_17()
            {   // example was wrong
                AssertResults(
                    parser.Parse(Resources.Example8_17),
                    map(
                        str("explicit key"), str("!!null", ""),
                        str("block key\n"), seq(
                            str("one"), str("two")
                        )
                    )
                );
            }

            [Test] // Example 8.18. Implicit Block Mapping Entries
            public void TestExample8_18()
            {
                AssertResults(
                    parser.Parse(Resources.Example8_18),
                    map(
                        str("plain key"), str("in-line value"),
                        str("!!null", ""), str("!!null", ""),
                        str("quoted key"), seq(str("entry"))
                    )
                );
            }

            [Test] // Example 8.19. Compact Block Mappings
            public void TestExample8_19()
            {
                AssertResults(
                    parser.Parse(Resources.Example8_19),
                    seq(
                        map(
                            str("sun"), str("yellow")
                        ),
                        map(
                            map(str("earth"), str("blue")),
                            map(str("moon"), str("white"))
                        )
                    )
                );
            }

            [Test] // Example 8.20. Block Node Types
            public void TestExample8_20()
            {
                AssertResults(
                    parser.Parse(Resources.Example8_20),
                    seq(
                        str("flow in block"),
                        str("Block scalar\n"),
                        map(
                            str("foo"),
                            str("bar")
                        )
                    )
                );
            }

            [Test] // Example 8.21. Block Scalar Nodes
            public void TestExample8_21()
            {   
                AssertResults(
                    parser.Parse(Resources.Example8_21),
                    map(
                        str("literal"), str("value\n"),
                        str("folded"), str("!foo", "value")
                    )
                );
            }

            [Test] // Example 8.22. Block Collection Nodes
            public void TestExample8_22()
            {   
                AssertResults(
                    parser.Parse(Resources.Example8_22),
                    map(
                        str("sequence"), seq(
                            str("entry"),
                            seq( str("nested") )
                        ),
                        str("mapping"), map(
                            str("foo"), str("bar")
                        )
                    )    
                );
            }

        }

        [TestFixture]
        public class Chapter9: YamlTestFixture
        {
            [Test] // Example 9.1. Document Prefix
            public void TestExample9_1()
            {
                AssertResults(
                    parser.Parse("\ufeff" + Resources.Example9_1),
                    str("Document")
                );
            }

            [Test] // Example 9.2. Document Markers
            public void TestExample9_2()
            {
                AssertResults(
                    parser.Parse(Resources.Example9_2),
                    str("Document")
                );
            }

            [Test] // Example 9.3. Bare Documents
            public void TestExample9_3()
            {
                AssertResults(
                    parser.Parse(Resources.Example9_3),
                    str("Bare document"),
                    str("%!PS-Adobe-2.0 # Not the first line\n")
                );
            }

            [Test] // Example 9.4. Explicit Documents
            public void TestExample9_4()
            {
                AssertResults(
                    parser.Parse(Resources.Example9_4),
                    map(str("matches %"), str("!!int", "20")),
                    str("!!null", "")
                );
            }

            [Test] // Example 9.5. Directives Documents
            public void TestExample9_5()
            {   
                AssertResultsWithWarnings(
                    parser.Parse(Resources.Example9_5),
                    1,
                    str("%!PS-Adobe-2.0\n"),
                    str("!!null", "")
                );

                AssertResults(
                    parser.Parse("a\n---"),
                    str("a"),
                    str("!!null", "")
                );

                AssertResults(
                    parser.Parse(":\n---"),
                    map(str("!!null", ""), str("!!null", "")),
                    str("!!null", "")
                );

                AssertResults(
                    parser.Parse("-\n---"),
                    seq(str("!!null", "")),
                    str("!!null", "")
                );

                AssertParseError(() =>
                    parser.Parse("[\n---\n]")
                );

                AssertResults(
                    parser.Parse("[\n---a\n]"),
                    seq(str("---a"))
                );

                AssertParseError(() =>
                    parser.Parse("{\n---\n:}")
                );

                AssertParseError(() =>
                    parser.Parse("{:\n---\n}")
                );

                AssertParseError(() =>
                    parser.Parse("\"\n---\n\"")
                );

                AssertParseError(() =>
                    parser.Parse("'\n---\n'")
                );
            }

            [Test] // Example 9.6. Stream
            public void TestExample9_6()
            {   
                AssertResults(
                    parser.Parse(Resources.Example9_6),
                    str("Document"),
                    str("!!null", ""),
                    map(str("matches %"), str("!!int", "20"))
                );
            }
        }

        // this does not seem to work for defining reduction rules
        public class Rule
        {
            Func<bool> condition;
            Rule next;

            static object PrepareRewind()
            {
                return null;
            }

            static void Rewind(object rewind)
            {
            }

            public bool Evaluate()
            {
                if ( next == null )
                    return condition();
                var rewind = PrepareRewind();
                var result = condition() && next.EvaluateInternal();
                if ( !result )
                    Rewind(rewind);
                return result;
            }

            public bool EvaluateInternal()
            {
                if ( next == null )
                    return condition();
                return condition() && next.Evaluate();
            }

            public Rule Duplicate()
            {
                var result= new Rule(condition);
                result.next = next;
                return result;
            }

            public static Rule operator +(Rule a, Rule b)
            {
                var result = a.Duplicate();
                if ( result.next == null ) {
                    result.next = b;
                } else {
                    result.next = result.next + b;
                }
                return result;
            }

            public static Rule operator *(Rule a, Repeat repeat)
            {
                switch ( repeat ) {
                case Repeat.ZeroOrOne:
                    return (Rule)( () => {
                        a.Evaluate();
                        return true;
                    } );
                case Repeat.OneOrMany:
                    return (Rule)( () => {
                        if ( !a.Evaluate() )
                            return false;
                        while ( a.Evaluate() )
                            ;
                        return true;
                    } );
                case Repeat.Any:
                    return (Rule)( () => {
                        while ( a.Evaluate() )
                            ;
                        return true;
                    } );
                }
                return null;
            }

            public static Rule operator *(Rule a, int n)
            {
                return (Rule)( () => {
                    for ( int i = 0; i < n; i++ )
                        if ( !a.Evaluate() )
                            return false;
                    return true;
                } );
            }

            public static Rule operator |(Rule a, Rule b)
            {
                return new Rule( () => a.Evaluate() || b.Evaluate() );
            }

            public static implicit operator Rule (Func<bool> condition)
            {
                return new Rule(condition);
            }

            public Rule(Func<bool> condition)
            {
                this.condition = condition;
            }

        }

        public enum Repeat: uint {
            ZeroOrOne,
            OneOrMany,
            Any
        }

        [TestFixture]
        public class Extra: YamlTestFixture
        {
            [Test]
            public void TestRuleObject()
            {
                Rule rule = (Rule)( () => true );
                Rule rule2 = rule + rule + rule | rule * Repeat.Any;
                Rule rule3 = ( ( rule2 + rule ) | rule ) * Repeat.OneOrMany;
                Rule rule4 = ( rule3 * 4 ) * Repeat.ZeroOrOne;
            }

            [Test]
            public void TestBinarySearch()
            {
                var list = new List<int>();
                list.Add(0);
                list.Add(10);
                list.Add(20);
                Assert.AreEqual( 0, list.BinarySearch(  0 ));
                Assert.AreEqual(-2, list.BinarySearch(  5 ));
                Assert.AreEqual( 1, list.BinarySearch( 10 ));
                Assert.AreEqual(-3, list.BinarySearch( 15 ));
                Assert.AreEqual( 2, list.BinarySearch( 20 ));
                Assert.AreEqual(-4, list.BinarySearch( 25 ));
            }

            [Test] 
            public void Test1()
            {   
                AssertResults(
                    parser.Parse(@"\"),
                    str(@"\")
                );

                AssertResults(
                    parser.Parse(@"a ,[]{}#&*:!|>'""%@`d"),
                    str(@"a ,[]{}#&*:!|>'""%@`d")
                );

                AssertResults(
                    parser.Parse(@"- a ,[]{}#&*:!|>'""%@`d"),
                    seq(str(@"a ,[]{}#&*:!|>'""%@`d"))
                );

                AssertResults(
                    parser.Parse(@"a ,[]{}#&*:!|>'""%@`d: a"),
                    map(str(@"a ,[]{}#&*:!|>'""%@`d"), str("a"))
                );

                AssertResults(
                    parser.Parse("? a ,[]{}#&*:!|>'\"%@`d\n: a"),
                    map(str(@"a ,[]{}#&*:!|>'""%@`d"), str("a"))
                );

                AssertResults(
                    parser.Parse("- [a, a,\r  a, a]"),
                    seq(seq(str("a"), str("a"), str("a"), str("a")))
                );

                AssertResults(
                    parser.Parse("{}"),
                    map()
                );

                AssertResults(
                    parser.Parse("[]"),
                    seq()
                );

            }

            [Test]
            public void TestLineBreaks()
            {
                // \r\n in content \n in presentation
                AssertResults(
                    parser.Parse("\"abc\\r\\n\\\ndef\""),
                    "abc\r\ndef"
                );

                // \r\n in content \r\n in presentation
                AssertResults(
                    parser.Parse("\"abc\\r\\n\\\r\ndef\""),
                    "abc\r\ndef"
                );

                // \r\n in content \r in presentation
                AssertResults(
                    parser.Parse("\"abc\\r\\n\\\rdef\""),
                    "abc\r\ndef"
                );

                // \n in content \n in presentation
                AssertResults(
                    parser.Parse("\"abc\\n\\\ndef\""),
                    "abc\ndef"
                );

                // \n in content \r\n in presentation
                AssertResults(
                    parser.Parse("\"abc\\n\\\r\ndef\""),
                    "abc\ndef"
                );

                // \n in content \r\n in presentation
                AssertResults(
                    parser.Parse("\"abc\\n\\\rdef\""),
                    "abc\ndef"
                );

                // \r in content \n in presentation
                AssertResults(
                    parser.Parse("\"abc\\r\\\ndef\""),
                    "abc\rdef"
                );

                // \r in content \r\n in presentation
                AssertResults(
                    parser.Parse("\"abc\\r\\\r\ndef\""),
                    "abc\rdef"
                );

                // \r in content \r\n in presentation
                AssertResults(
                    parser.Parse("\"abc\\r\\\rdef\""),
                    "abc\rdef"
                );

            }

            [Test]
            public void TestNsPlainChar()
            {
                AssertResults(
                    parser.Parse("{::::}"),
                    map(":::", str("!!null", ""))
                );
            }
        }
    }
}
