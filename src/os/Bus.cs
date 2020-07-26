namespace IotHub
{
    using System.Collections.Generic;
    using System.Linq;

    public class Bus
    {
        public State State { get; set; }
        public Kernel Kernel { get; set; }

        public Device[] Devices = new Device[16];

        private int len;
        public Bus(Kernel kernel)
        {
            State = new State(this);
            Kernel = kernel;


            //Add(new BIOS(this));
            Add(new Memory(0x0, 0x90000, this));
        }
        public void Add(Device device)
        {
            Devices[len++] = device;
        }

        public IDevice find(int address)
        {
            foreach (var device in Devices)
            {
                if (device.startAddress == address)
                    return device;
            }
            return null;
        }
    }
}