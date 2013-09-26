namespace YamlSerializer
{
    /// <summary>
    /// Extend string object to have .DoubleQuoteEscape() / .DoubleQuoteUnescape().
    /// </summary>
    internal static class StringYamlDoubleQuoteEscapeExtention
    {
        /// <summary>
        /// Escape control codes with YAML double quoted string format.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string YamlDoubleQuoteEscape(this string s)
        {
            return YamlDoubleQuoteEscaping.Escape(s);
        }
        /// <summary>
        /// Unescape control codes escaped with YAML double quoted string format.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string YamlDoubleQuoteUnescape(this string s)
        {
            return YamlDoubleQuoteEscaping.Unescape(s);
        }
    }
}