namespace IotHub
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Memory : Device
    {
        private readonly Kernel _kernel;
        internal readonly ulong[] mem;

        public Memory(int startAddress, int endAddress, Bus bus) : base(0x0, "<ddr>")
        {
            _kernel = bus.kernel;
            // 512kb max
            if (endAddress >= 0x100000)
                _kernel.halt(0xBD);
            mem = new ulong[endAddress - startAddress + 1];
        }


        public override void write(long address, ulong data)
        {
            if (address > 0x900 && read(0x899) < (ulong)address) 
                write(0x899, address);

            if (address >= mem.Length)
            {
                _kernel.halt(0xBD);
                return;
            }
            mem[address] = data;
        }
        public override ulong read(long address)
        {
            if (address < mem.Length) 
                return mem[address];
            _kernel.halt(0xBD);
            return 0;
        }

        public void load(byte[] binary, int memOffset, int maxLen)
        {
            if (binary.Length % sizeof(long) != 0)
                _kernel.halt(0xD6);
            var bin = binary.Batch(sizeof(long)).Select(x => BitConverter.ToInt64(x.ToArray(), 0)).Reverse().ToArray();
            Array.Copy(bin, 0, mem, memOffset, maxLen);
        }
    }


    public static class ex
    {
        public static TSource[][] Batch<TSource>(
            this TSource[] source, int size)
        {
            if(source.Length % size != 0)
                throw new Exception("Batch failed. not symmetrical.");

            TSource[][] bucketBucket = new TSource[source.Length / size][];
            TSource[] bucket = null;
            var count = 0;
            var bcount = 0;

            foreach (var item in source)
            {
                if (bucket == null)
                    bucket = new TSource[size];

                bucket[count++] = item;
                if (count != size)
                    continue;
                bucketBucket[bcount++] = bucket;

                bucket = null;
                count = 0;
            }

            if (bucket != null && count > 0)
                bucketBucket[bcount++] = bucket;

            return bucketBucket;
        }
    }
}