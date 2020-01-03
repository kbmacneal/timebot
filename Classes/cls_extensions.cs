using System;
using System.Collections.Generic;
using System.Linq;

namespace timebot.Classes.Extensions
{
    public static class IEnumerableExtensions
    {
        public static List<T> GetRandom<T> (this IEnumerable<T> list, int count)
        {
            var rtn = new List<T>();
            for (int i = 0; i < count; i++)
            {
                var index = Program.rand.Next (0, list.Count () - 1);
                rtn.Add(list.ElementAt(index));
            }

            return rtn;
        }
    }

}