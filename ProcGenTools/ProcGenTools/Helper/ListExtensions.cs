﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcGenTools.Helper
{
    public static class ListExtensions
    {
        public static void Shuffle<T>(this IList<T> collection, Random random)
        {
            int n = collection.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = collection[k];
                collection[k] = collection[n];
                collection[n] = value;
            }
        }

        public static T GetRandomOrDefault<T>(this IEnumerable<T> collection, Random random)
        {
            if (collection.Count() == 0)
                return default(T);
            return collection.ElementAt(random.Next(0, collection.Count()));
        }
        
    }
}
