using System;

namespace YamlSerializer
{
    /// <summary>
    /// <para>When <see cref="Parser&lt;State&gt;"/> reports syntax error by exception, this class is thrown.</para>
    /// 
    /// <para>Sytax errors can also be reported by simply returing false with giving some warnings.</para>
    /// </summary>
    internal class ParseErrorException: Exception
    {
        /// <summary>
        /// Initialize an instance of <see cref="ParseErrorException"/>
        /// </summary>
        /// <param name="message">Error message.</param>
        public ParseErrorException(string message) : base(message) { }
    }
}