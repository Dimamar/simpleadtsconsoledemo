using System.ComponentModel;
using System.Threading;

namespace SimpleADTSConsole
{
    public interface IAdtsScript
    {
        string Name { get; }

        double Progress { get; set; }

        string Step { get; set; }

        void Start(ADTSConsoleModel adts, CancellationToken cancel);
    }
}