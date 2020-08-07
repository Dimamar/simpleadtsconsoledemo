namespace SimpleADTSConsole.Interfaces.IEEE488
{
    public interface IIee488
    {
        string ibrd(int boarddev);

        void ibwrt(int boarddev, string buf);

        void ibclr(int device);

        int ibdev(int boardindex, int pad, int sad, Timeout timeout, bool eot, short eos);
    }
}