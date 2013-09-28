namespace YamlSerializer
{
    /// <summary>
    /// Add .DoFunction method to string
    /// </summary>
    internal static class StringExtension
    {
        /// <summary>
        /// Short expression of string.Format(XXX, arg1, arg2, ...)
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string DoFormat(this string format, params object[] args)
        {
            return string.Format(format, args);
        }
    }
}