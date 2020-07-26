namespace IotHub.linq
{
    using System;

    public static class FirstEx
    {
        public static T First<T>(this T[] items)
        {
            if (items.Length == 0)
                return default;
            return items[0];
        }
    }

    public static class SkipEx
    {
        public static T[] Skip<T>(this T[] items, int count)
        {
            var newItems = new T[items.Length - count];
            for (var i = count; i != items.Length; i++) 
                newItems[i - count] = items[i];
            return newItems;
        }
    }
}