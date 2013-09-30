using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace YamlSerializer
{
    /// <summary>
    /// Validates a text as a global tag in YAML.
    /// 
    /// <a href="http://www.faqs.org/rfcs/rfc4151.html">RFC4151 - The 'tag' URI Scheme</a>>
    /// </summary>
    internal class YamlTagValidator: Parser<YamlTagValidator.Status>
    {
        /// <summary>
        /// Not used in this parser
        /// </summary>
        public struct Status { }

        public static YamlTagValidator Default
        {
            get { return default_; }
        }
        static YamlTagValidator default_ = new YamlTagValidator();

        /// <summary>
        /// Validates a text as a global tag in YAML.
        /// </summary>
        /// <param name="tag">A candidate for a global tag in YAML.</param>
        /// <returns>True if <paramref name="tag"/> is  a valid global tag.</returns>
        public bool IsValid(string tag)
        {
            text = tag;
            p = 0;
            return TagUri();
        }

        private bool TagUri()
        {
            return
                Accept("tag:") &&
                taggingEntity() &&
                Accept(':') &&
                specific() &&
                OptionalBlabla(
                    Accept('#') &&
                    fragment()
                ) &&
                EndOfString();
        }

        private bool taggingEntity()
        {
            return
                new RewindState(this).Result(
                    authorityName() &&
                    Accept(',') &&
                    date()
                );
        }

        private bool authorityName()
        {
            return
                emailAddress() ||
                DNSname();
        }

        private bool DNSname()
        {
            return
                DNScomp() &&
                Repeat(() =>
                    Accept('.') &&
                    DNScomp()
                );
        }

        private bool DNScomp()
        {
            return new RewindState(this).Result(
                OneAndRepeat(alphaNum) &&
                Repeat(() => new RewindState(this).Result(
                    Accept('-') &&
                    OneAndRepeat(alphaNum)
                ))
            );
        }

        private bool alphaNum()
        {
            return
                Accept(alphaNumCharset);
        }
        Func<char, bool> alphaNumCharset = Charset(c =>
                c < 0x100 && (
                    ( '0' <= c && c <= '9' ) ||
                    ( 'A' <= c && c <= 'Z' ) ||
                    ( 'a' <= c && c <= 'z' )
                )
            );

        private bool emailAddress()
        {
            return new RewindState(this).Result(
                OneAndRepeat(() => alphaNum() || Accept('-') || Accept('.') || Accept('_')) &&
                Accept('@') &&
                DNSname()
            );
        }

        private bool date()
        {
            return
                Accept(dateRegex);
        }
        Regex dateRegex = new Regex(@"(19[89][0-9]|20[0-4][0-9])(-(0[1-9]|1[0-2])(-(0[1-9]|[12][0-9]|3[01]))?)?");

        private bool num()
        {
            return
                Accept(numCharset);
        }
        Func<char, bool> numCharset = Charset(c =>
                c < 0x100 && 
                ( '0' <= c && c <= '9' ) 
            );

        private bool specific()
        {
            return
                Repeat(() => pchar() || Accept('/') || Accept('?'));
        }

        private bool fragment()
        {
            return
                Repeat(() => pchar() || Accept('/') || Accept('?'));
        }

        private bool EndOfString()
        {
            return text.Length == p;
        }

        bool pchar()
        {
            return
                Accept(pcharCharsetSub) ||
                new RewindState(this).Result(
                    Accept('%') &&
                    hexDig() &&
                    hexDig()
                );
        }
        Func<char, bool> pcharCharsetSub = Charset(c =>
                c < 0x100 && (
                    ( '0' <= c && c <= '9' ) ||
                    ( 'A' <= c && c <= 'Z' ) ||
                    ( 'a' <= c && c <= 'z' ) ||
                    "-._~!$&'()*+,;=:@".Contains(c)
                )
            );

        bool hexDig()
        {
            return Accept(hexDigCharset);
        }
        Func<char, bool> hexDigCharset = Charset(c =>
                c < 0x100 && (
                    ( '0' <= c && c <= '9' ) ||
                    ( 'A' <= c && c <= 'F' ) ||
                    ( 'a' <= c && c <= 'f' )
                )
            );

    }

}
