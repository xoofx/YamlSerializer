using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using System.Yaml;

namespace YamlSerializerTest
{
    [TestFixture]
    public class YamlTagValidatorTest
    {
        YamlTagValidator validator = new YamlTagValidator();
        void AssertOK(string s)
        {
            Assert.IsTrue(validator.IsValid(s), s);
        }
        void AssertNG(string s)
        {
            Assert.IsFalse(validator.IsValid(s), s);
        }
        [Test]
        public void Test()
        {
            AssertOK("tag:yaml.org,2002:str");
            AssertNG("tag:-yaml.org,2002:str");
            AssertNG("tag:yaml-.org,2002:str");
            AssertOK("tag:ya-ml.org,2002:str");
            AssertNG("tag:yaml..org,2002:str");
            AssertNG("tog:yaml.org,2002:str");
            AssertOK("tag:yaml@yaml.org,2002:str");
            AssertOK("tag:.ya-ml_--@yaml.org,2002:str");
            AssertNG("tag:.ya-m#l_--@yaml.org,2002:str");
            AssertOK("tag:yaml.org,1980:str"); // not in reality (DNS started 1983)
            AssertNG("tag:yaml.org,1979:str");
            AssertOK("tag:yaml.org,2049:str");
            AssertNG("tag:yaml.org,2050:str"); // probably
            AssertOK("tag:yaml.org,2002-01:str");
            AssertNG("tag:yaml.org,2002-00:str");
            AssertNG("tag:yaml.org,2002-13:str");
            AssertOK("tag:yaml.org,2002-01-01:str");
            AssertNG("tag:yaml.org,2002-01-00:str");
            AssertOK("tag:yaml.org,2002-01-31:str");
            AssertNG("tag:yaml.org,2002-01-32:str");
            AssertOK("tag:yaml.org,2002-02-31:str"); // not in reality
            AssertOK("tag:yaml.org,2002:"); // rfc 4151 allows empty [specific]
            AssertOK("tag:yaml.org,2002:#"); // rfc 4151 allows empty [fragment]
            AssertOK("tag:yaml.org,2002:str%2c");
            AssertOK("tag:yaml.org,2002:str%ff"); // not in reality (bad coding)
            AssertOK("tag:yaml.org,2002:over/there?name=ferret#nose");
            AssertNG("tag:yaml.org%2c,2002:str");
        }

    }
}
