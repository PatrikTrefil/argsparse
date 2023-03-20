using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Numactl;

static class ListExtensions
{
    public static bool ValuesAreInRange(this IList<int>? list, int? from, int? to)
    {
        return list == null || list.All(val => val.IsInRange(from, to));
    }
}

static class IntExtensions
{
    public static bool IsInRange(this int num, int? from, int? to)
    {
        return (from == null || from <= num) && (to == null || num < from);
    }
    public static bool IsInRange(this int? num, int? from, int? to)
    {
        return (num == null || IsInRange(num.Value, from, to));
    }
}