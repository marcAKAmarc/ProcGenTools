﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcGenTools.DataStructures
{
    static class Helpers
    {
        public static T NextChoice<T>(this Random random, List<T> items)
        {
            return items[random.Next(items.Count)];
        }
    }
}
