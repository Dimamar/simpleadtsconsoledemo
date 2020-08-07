namespace SimpleADTSConsole
{
    public interface IStatus : IBusy
    {
        bool IsOpened { get; set; }
    }
}