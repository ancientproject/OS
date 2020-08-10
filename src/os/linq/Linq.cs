namespace IotHub.linq
{
    using System;
    using System.Runtime.CompilerServices;

    public static class LinqEx
    {
        public static bool All<T>(this T[] items, Func<T, bool> predicate)
        {
            if (items.Length == 0)
                return false;
            var flag = true;
            foreach (var item in items) 
                flag &= predicate(item);
            return flag;
        }
        public static T First<T>(this T[] items)
        {
            if (items.Length == 0)
                return default;
            return items[0];
        }
        public static R[] Select<T, R>(this T[] items, Func<T, R> predicate) 
            => items.Select((x, i) => predicate(x));
        public static R[] Select<T, R>(this T[] items, Func<T, int, R> predicate)
        {
            
            var newItems = new R[items.Length];
            for (var i = 0; i != items.Length; i++)
                newItems[i] = predicate(items[i], i);
            return newItems;
        }
        public static T[] Skip<T>(this T[] items, int count)
        {
            var newItems = new T[items.Length - count];
            for (var i = count; i != items.Length; i++) 
                newItems[i - count] = items[i];
            return newItems;
        }
    }
    public static class AdditionalEx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] ToArray<T>(this T[] items) => items;
    }
}