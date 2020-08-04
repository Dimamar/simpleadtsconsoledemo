using System.Threading;

namespace SimpleADTSConsole
{
    public interface IAdtsScript
    {
        string Name { get; }

        double Progress { get; set; }

        string Step { get; set; }

        void Start(IADTSConsoleModel adts, CancellationToken cancel);
    }
}