using System;
using System.Collections.Generic;
using System.Linq;

using System.Reflection;
using TestDriven.Framework;

namespace YamlSerializerTestRunner
{
    /// <summary>
    /// This application is just for running NUnit test in a debugger.
    /// Visual C# 2008 *Express Edition* does not allow me to atach its debugger
    /// to NUnit nor to enable TestDriven.
    /// </summary>
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            var runner = new NUnit.AddInRunner.NUnitTestRunner();
            var Listener = new  Listener();
            runner.RunAssembly(Listener,
                Assembly.GetAssembly(typeof(YamlSerializerTest.YamlPresenterTest)));
        }

        public class Listener: ITestListener
        {
            public void TestFinished(TestResult summary)
            { }
            public void TestResultsUrl(string resultsUrl)
            { }
            public void WriteLine(string text, Category category)
            { }
        }
    }
}
