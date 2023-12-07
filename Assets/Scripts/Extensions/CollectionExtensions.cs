using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Extensions
{
    public static class CollectionExtensions
    {
        private static readonly Random Random = new();
        private static readonly object Sync = new();

        public static T GetRandom<T>(this IReadOnlyList<T> list)
        {
            lock (Sync)
            {
                return list[Random.Next(list.Count)];
            }
        }

        public static T GetRandom<T>(this IReadOnlyList<T> list, Func<T, bool> filter)
        {
            return list.Where(filter).ToList().GetRandom();
        }
    }
}