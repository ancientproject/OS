namespace IotHub
{
    using System;
    public interface IMemoryRange
    {
        void write(ulong address, long data);
        void write(long address, long data);
        void write(ulong address, ulong data);
        void write(long address, ulong data);
        ulong read(ulong address);
        ulong read(long address);
    }
    public interface IDevice : IMemoryRange
    {
        string name { get; }
        short startAddress { get; set; }

        long this[long address] { get; set; }

        void warmUp();
        void shutdown();
    }

    public abstract class Device : IDevice
    {
        public string name { get; private set; }
        public short startAddress { get; set; }


        protected Device(short address, string name)
        {
            startAddress = address;
            this.name = name;
        }

        public long this[long address]
        {
            get => (long)read(address);
            set => write(address, value);
        }

        #region read\write

        public void write(ulong address, long data)
            => write((long) address, data);

        public void write(long address, long data) =>
            write(address, (ulong) data);

        public void write(ulong address, ulong data)
            => write((long) address, data);

        public virtual void write(long address, ulong data)
        {

        }

        public ulong read(ulong address)
            => read((long) address);

        public virtual ulong read(long address) 
            => throw new Exception();

        public virtual void warmUp() { }

        public virtual void shutdown() { }

        #endregion


        #region def

        public int CompareTo(object obj)
        {
            if (!(obj is IDevice dev)) return 0;
            if (startAddress > dev.startAddress)
                return 1;
            return -1;
        }
        public override int GetHashCode()
        {
            var hash = unchecked(startAddress.GetHashCode() ^ 0x2A * name.GetHashCode() ^ 0x2A);
            return (hash & 0xFF) ^ ((hash >> 16) & 0xFF) ^ ((hash >> 8) & 0xFF) ^ ((hash >> 24) & 0xFF);
        }

        #endregion
    }
}