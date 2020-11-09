using System;
using System.Collections.Generic;
using System.Linq;

namespace BestDiceRollerBot
{
    public static class LINQExtension
    {
        public static IEnumerable<TRes> Unfold<TRes, T>(this T item, Func<T, Tuple<TRes, T>> generator)
        {
            var res = generator(item);
            if (res == null) yield break;
            yield return res.Item1;
            foreach(var sub in Unfold<TRes, T>(res.Item2, generator)) yield return sub;
        }
    }
}