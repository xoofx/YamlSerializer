namespace YamlSerializer
{
    internal static class StringUtils
    {
        public static bool Contains(this string str, char character)
        {
            for (int i = 0; i < str.Length; i++)
                if (str[i] == character)
                    return true;

            return false;
        }
    }
}