namespace IotHub
{
    using System;
    using Cosmos.System.FileSystem;
    using static System.Console;

    public class Kernel : Cosmos.System.Kernel
    {
        private State state;
        private Bus bus;

        protected override void BeforeRun()
        {
            bus = new Bus(this);
            state = new State(bus);
            var fs = new CosmosVFS();
        }

        protected override void Run()
        {
            Step();
        }          
        

        public int halt(int reason, string text = "")
        {
            if (state.halt != 0) return reason;
            Error(Environment.NewLine);
            state.halt = 1;
            Error($"{reason} {text}");
            //var l1 = _bus.State;
            //Error($"L1 Cache, PC: 0x{l1.pc:X8}, OpCode: {l1.iid} [{l1.iid.getInstruction()}]");
            //Error($"\t0x{l1.r1:X} 0x{l1.r2:X} 0x{l1.r3:X} 0x{l1.u1:X} 0x{l1.u2:X} 0x{l1.x1:X} 0x{l1.x2:X}");
            return reason;
        }

        private void Error(string str)
        {
            ForegroundColor = ConsoleColor.Red;
            WriteLine(str);
            ForegroundColor = ConsoleColor.White;
        }

        public void Step()
        {
            try
            {
                state.Accept(state.fetch());
                state.Eval();
            }
            catch (Exception e)
            {
                halt(0xFFFF, e.Message.ToLowerInvariant());
                WriteLine(e.ToString());
            }
        }
    }
}
