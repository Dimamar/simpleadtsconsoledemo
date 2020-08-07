namespace SimpleADTSConsole.Tools
{
    public interface IObservableUpdater<in T>
    {
        void OnNext(T data);
    }
}