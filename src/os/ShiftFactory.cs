namespace IotHub
{
    using System;

    public class ShiftFactory
    {
        private int prev;
        private int index;

        private ShiftFactory() {}

        public static ShiftFactory CreateByIndex(int bitIndex) => new ShiftFactory {index = bitIndex};

        public int Shift()
        {
            prev = index;
            index -= 4;
            if (index < 0) index = 0;
            return prev;
        }

        public static implicit operator Func<int>(ShiftFactory factory) => factory.Shift;
    }
}