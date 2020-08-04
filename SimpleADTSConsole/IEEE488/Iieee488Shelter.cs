namespace WrapLib.IEEE488
{
    public interface IIeee488Shelter
    {
        string ibrd(int boarddev);

        void ibwrt(int boarddev, string buf);

        void ibclr(int device);

        int ibdev(int boardindex, int pad, int sad, Timeout timeout, bool eot, short eos);
    }
}