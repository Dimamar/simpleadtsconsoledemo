using System;
using System.Threading;
using WrapLib.IEEE488;
using Timeout = WrapLib.IEEE488.Timeout;

namespace SimpleADTSConsole.ForTest
{
    class MoqIeee488Shelter : IIeee488Shelter
    {
        public int TimeWait { get; set; }
        private Random _rnd;
        private int count = 0;
        private int num = 0;

        public MoqIeee488Shelter(int timeWait)
        {
            TimeWait = timeWait;
            _rnd = new Random();
        }

        public string ibrd(int boarddev)
        {
            Console.WriteLine($"call ibrd, {boarddev}");
            Thread.Sleep(TimeWait);
            if (count > 9)
            {
                num = _rnd.Next(0, 200);
                count = 0;
            }
            count++;
            return $"ibrd {num}";
        }

        public void ibwrt(int boarddev, string buf)
        {
            Console.WriteLine($"call ibwrt {boarddev}, {buf}");
            Thread.Sleep(TimeWait);
        }

        public void ibclr(int device)
        {
            Console.WriteLine($"call ibclr {device}");
            Thread.Sleep(TimeWait);
        }

        public int ibdev(int boardindex, int pad, int sad, Timeout timeout, bool eot, short eos)
        {
            Console.WriteLine($"call  ibdev {boardindex}, {pad}, {sad}");
            Thread.Sleep(TimeWait);
            return 0;
        }
    }
}
