namespace IotHub
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using exceptions;
    using static System.Console;
    public class State
    {

        private readonly Bus bus;
        private readonly Stack stack;
        /// <summary>
        /// Section addressing information
        /// </summary>
        public List<(string block, uint address)> sectors = new List<(string block, uint address)>(16);
        public State(Bus bus)
        {
            this.bus = bus;
            this.stack = new Stack(bus);
        }
        /// <summary>
        /// Load to memory execution data
        /// </summary>
        /// <param name="name">name of execution memory</param>
        /// <param name="prog">execution memory</param>
        public void Load(string name, params ulong[] prog)
        {
            var pin = 0x600;
            var set = 0x599;
            if (AcceptOpCode(prog.First()) == 0x33)
            {
                Func<int> shift = ShiftFactory.CreateByIndex(sizeof(int) * 0b100 - 0b100).Shift;
                Accept(prog.First());
                prog = prog.Skip(1).ToArray();
                pin = (r1 << shift()) | (r2 << shift()) | (r3 << shift()) | (u1 << shift());
                set = pin - 0b1;
            }
            foreach (var (@ulong, index) in prog.Select((x, i) => (x, i)))
                bus.find(0x0).write(pin + index, @ulong);
            bus.find(0x0).write(set, prog.Length);
            sectors.Add((name, u32 & pin));
            if(pc == 0x0) pc = i64 | pin;
        }

        /// <summary>
        /// Deconstruct <see cref="UInt64"/> to registres
        /// </summary>
        /// <remarks>
        /// ===
        /// :: current instruction map (x8 bit instruction size, x40bit data)
        /// reserved    r   u  x
        ///   | opCode 123 123 12
        ///   |     |   |   |  |
        /// 0xFFFF_FFCC_AAA_BBB_DD
        /// ===
        /// :: future instruction map (x16 bit instruction size, x48bit data)
        ///          r     u    x f
        ///  opCode 1234  1234  1212
        ///   |  |
        /// 0xFFFF__AAAA__DDDD__EEEE
        /// ===
        /// </remarks>
        public void Accept(ulong container)
        {
            trace($"fetch 0x{container:X}");

            var shifter = ShiftFactory.CreateByIndex(52);

            pfx = (ushort)((container & 0x00F0000000000000) >> shifter.Shift());
            iid = (ushort)((container & 0x000F000000000000) >> shifter.Shift());
            r1  = (ushort)((container & 0x0000F00000000000) >> shifter.Shift());
            r2  = (ushort)((container & 0x00000F0000000000) >> shifter.Shift());
            r3  = (ushort)((container & 0x000000F000000000) >> shifter.Shift());
            u1  = (ushort)((container & 0x0000000F00000000) >> shifter.Shift());
            u2  = (ushort)((container & 0x00000000F0000000) >> shifter.Shift());
            x1  = (ushort)((container & 0x000000000F000000) >> shifter.Shift());
            x2  = (ushort)((container & 0x0000000000F00000) >> shifter.Shift());
            x3  = (ushort)((container & 0x00000000000F0000) >> shifter.Shift());
            x4  = (ushort)((container & 0x000000000000F000) >> shifter.Shift());
            o1  = (ushort)((container & 0x0000000000000F00) >> shifter.Shift());
            o2  = (ushort)((container & 0x00000000000000F0) >> shifter.Shift());
            o3  = (ushort)((container & 0x000000000000000F) >> shifter.Shift());
            iid = (ushort)((pfx << 4) | iid );
        }

        /// <summary>
        /// Deconstruct <see cref="UInt64"/> to OpCode
        /// </summary>
        /// <param name="container"></param>
        /// <returns>
        /// 8 bit <see cref="ushort"/>
        /// </returns>
        public ushort AcceptOpCode(ulong container)
        {
            var shifter = ShiftFactory.CreateByIndex(52);

            var pfx1 = (ushort)((container & 0x00F0000000000000) >> shifter.Shift());
            var pfx2 = (ushort)((container & 0x000F000000000000) >> shifter.Shift());
            return u16 & (pfx1 << 0x4 | pfx2);
        }
        public void Eval()
        {
            switch (iid)
            {
                case 0x0:
                    trace("call :: skip");
                    break;
                case 0x1 when x2 == 0x0:
                    trace($"call :: ldi 0x{u1:X}, 0x{u2:X} -> 0x{r1:X}");
                    _ = u2 switch
                    {
                        0x0 => mem[r1] = u1,
                        _ => mem[r1] = i64 | ((u2 << 4) | u1)
                    };
                    break;
                case 0xF when new[] { r1, r2, r3, u1, u2, x1 }.All(x => x == 0xF):
                    bus.kernel.halt(0xF);
                    break;

                case 0xD when r1 == 0xE && r2 == 0xA && r3 == 0xD:
                    bus.kernel.halt(0x0);
                    break;

                case 0xB when r1 == 0x0 && r2 == 0x0 && r3 == 0xB && u1 == 0x5:
                    bus.kernel.halt(0x1);
                    break;

                case 0x1 when x2 == 0xA:
                    trace($"call :: ldx 0x{u1:X}, 0x{u2:X} -> 0x{r1:X}-0x{r2:X}");
                    mem[((r1 << 4) | r2)] = i64 | ((u1 << 4) | u2);
                    break;

                case 0x3: /* @swap */
                    trace($"call :: swap, 0x{r1:X}, 0x{r2:X}");
                    mem[r1] ^= mem[r2];
                    mem[r2] = mem[r1] ^ mem[r2];
                    mem[r1] ^= mem[r2];
                    break;

                case 0xF when x2 == 0xE: // 0xF9988E0
                    trace($"call :: move, dev[0x{r1:X}] -> 0x{r2:X} -> 0x{u1:X}");
                    bus.find(r1 & 0xFF).write(r2 & 0xFF, i32 & mem[u1] & 0xFF);
                    break;

                case 0xF when x2 == 0xC: // 0xF00000C
                    trace($"call :: move, dev[0x{r1:X}] -> 0x{r2:X} -> [0x{u1:X}-0x{u2:X}]");
                    bus.find(r1 & 0xFF).write(r2 & 0xFF, (r3 << 12 | u1 << 8 | u2 << 4 | x1) & 0xFFFFFFF);
                    break;
            }
        }

        /// <summary>
        /// Fetch next execution memory
        /// </summary>
        /// <returns>
        /// Fragment of execution memory
        /// </returns>
        /// <exception cref="CorruptedMemoryException">
        /// Memory instruction at address access to memory could not be read.
        /// </exception>
        public ulong fetch()
        {
            try
            {
                if (halt != 0) return 0;
                lastAddr = curAddr;
                if (++step == 0x90000)
                    return (ulong)bus.kernel.halt(0x2);
                if (bus.find(0x0).read(0x599) != pc - 0x600)
                    return curAddr = bus.find(0x0).read(pc++);
                return (ulong)bus.kernel.halt(0x77);
            }
            catch
            {
                if (!km) 
                    Array.Fill(mem, 0xDEADUL, 0, 16);
                throw new CorruptedMemoryException($"Memory instruction at address 0x{curAddr:X4} access to memory 0x{pc:X4} could not be read.");
            }
        }

        private void trace(string str)
        {
            WriteLine(str);
        }

        private void warn(string str)
        {
            WriteLine($"-  {str}  -");
        }

        private void Error(string str)
        {
            ForegroundColor = ConsoleColor.Red;
            WriteLine(str);
            ForegroundColor = ConsoleColor.White;
        }


        public long SP { get; set; }
        
        public ulong pc { get; set; }

        /// <summary>
        /// base register
        /// </summary>
        public ushort r1 { get; set; }
        public ushort r2 { get; set; }
        public ushort r3 { get; set; }
        /// <summary>
        /// value register
        /// </summary>
        public ushort u1 { get; set; }
        public ushort u2 { get; set; }
        /// <summary>
        /// magic registers
        /// </summary>
        public ushort x1 { get; set; }
        public ushort x2 { get; set; }
        public ushort x3 { get; set; }
        public ushort x4 { get; set; }
        /// <summary>
        /// meta registers
        /// </summary>
        public ushort o1 { get; set; }
        public ushort o2 { get; set; }
        public ushort o3 { get; set; }
        /// <summary>
        /// id
        /// </summary>
        public ushort iid { get; set; }

        #region flags

        /// <summary>
        /// trace flag
        /// </summary>
        public bool tc 
        {
            get => mem[0x11] == 1;
            set => mem[0x11] = value ? 0x1UL : 0x0UL;
        }
        /// <summary>
        /// Error flag
        /// </summary>
        public bool ec
        {
            get => mem[0x12] == 1;
            set => mem[0x12] = value ? 0x1UL : 0x0UL;
        }

        /// <summary>
        /// Keep memory flag
        /// </summary>
        public bool km
        {
            get => mem[0x13] == 1;
            set => mem[0x13] = value ? 0x1UL : 0x0UL;
        }
        /// <summary>
        /// fast write flag
        /// </summary>
        public bool fw
        {
            get => mem[0x14] == 0x0;
            set => mem[0x14] = value ? 0x1UL : 0x0UL;
        }
        /// <summary>
        /// overflow flag
        /// </summary>
        public bool of
        {
            get => mem[0x15] == 1;
            set => mem[0x15] = value ? 0x1UL : 0x0UL;
        }
        /// <summary>
        /// negative flag
        /// </summary>
        public bool nf
        {
            get => mem[0x16] == 1;
            set => mem[0x16] = value ? 0x1UL : 0x0UL;
        }
        /// <summary>
        /// break flag (for next execute)
        /// </summary>
        public bool bf
        {
            get => mem[0x17] == 1;
            set => mem[0x17] = value ? 0x1UL : 0x0UL;
        }
        /// <summary>
        /// float flag
        /// </summary>
        public bool ff
        {
            get => mem[0x18] == 1;
            set => mem[0x18] = value ? 0x1UL : 0x0UL;
        }
        /// <summary>
        /// stack forward flag
        /// </summary>
        public bool sf
        {
            get => mem[0x19] == 1;
            set => mem[0x19] = value ? 0x1UL : 0x0UL;
        }
        /// <summary>
        /// control stack flag
        /// </summary>
        public bool northFlag
        {
            get => mem[0x20] == 1;
            set => mem[0x20] = value ? 0x1UL : 0x0UL;
        }
        /// <summary>
        /// control stack flag
        /// </summary>
        public bool eastFlag
        {
            get => mem[0x21] == 1;
            set => mem[0x21] = value ? 0x1UL : 0x0UL;
        }
        /// <summary>
        /// bios read-access
        /// </summary>
        public bool southFlag
        {
            get => mem[0x22] == 1;
            set => mem[0x22] = value ? 0x1UL : 0x0UL;
        }

        #endregion
        /// <summary>
        /// Current Address
        /// </summary>
        public ulong curAddr { get; set; } = 0xFFFF;
        /// <summary>
        /// Last executed address
        /// </summary>
        public ulong lastAddr { get; set; } = 0xFFFF;

        /// <summary>
        /// CPU Steps
        /// </summary>
        public virtual ulong step { get; set; }

        /// <summary>
        /// L1 Memory
        /// </summary>
        public ulong[] mem { get; } = new ulong[64];

        /// <summary>
        /// Halt flag
        /// </summary>
        public sbyte halt { get; set; } = 0;
        private ushort pfx { get; set; }


        #region casters

        /// <summary><see cref="byte"/> to <see cref="long"/></summary>
        public static readonly Unicast<byte  , ulong > u8  = new Unicast<byte  , ulong>();
        /// <summary><see cref="ushort"/> to <see cref="ulong"/></summary>
        public static readonly Unicast<ushort, ulong> u16 = new Unicast<ushort, ulong>();
        /// <summary><see cref="uint"/> to <see cref="long"/></summary>
        public static readonly Unicast<uint  , ulong > u32 = new Unicast<uint  , ulong>();
        /// <summary><see cref="int"/> to <see cref="long"/></summary>
        public static readonly Unicast<int   , ulong > i32 = new Unicast<int   , ulong>();
        /// <summary><see cref="long"/> to <see cref="ulong"/></summary>
        public static readonly Unicast<long  , ulong> i64 = new Unicast<long  , ulong>();

        /// <summary>bytecast <see cref="float"/> to <see cref="long"/></summary>
        public static readonly Bitcast<float, long > i64f32 = new Bitcast<float, long>();
        public static readonly Bitcast<float, ulong > u64f32 = new Bitcast<float, ulong>();
        /// <summary>bytecast <see cref="long"/> to <see cref="float"/></summary>
        public static readonly Bitcast<long , float> f32i64 = new Bitcast<long , float>();
        public static readonly Bitcast<ulong , float> f32u64 = new Bitcast<ulong , float>();
        /// <summary><see cref="int"/> to <see cref="short"/></summary>
        public static readonly Unicast<int  , short> i32i16 = new Unicast<int  , short>();

        #endregion
    }
}