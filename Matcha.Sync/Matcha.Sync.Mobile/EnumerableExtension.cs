using System;
using System.Collections.Generic;
using System.Text;

namespace Matcha.Sync.Mobile
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> enumberable, Action<T> action)
        {
            foreach (var item in enumberable)
            {
                action(item);
            }
        }
    }
}
