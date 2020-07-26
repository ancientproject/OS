namespace IotHub.linq
{
    using System;

    public static class AllEx
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
    }
}