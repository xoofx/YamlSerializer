using System;
using System.Collections.Generic;

namespace YamlSerializer
{
    internal static class ListExtensions
    {
        public static int FindIndex<T>(this List<T> list, Predicate<T> match)
        {
            return FindIndex(list, 0, list.Count, match);
        }

        public static int FindIndex<T>(this List<T> list, int startIndex, int count, Predicate<T> match)
        {
            int num = startIndex + count;
            for (int index = startIndex; index < num; ++index)
            {
                if (match(list[index]))
                    return index;
            }
            return -1;
        }
         
    }
}