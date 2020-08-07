using System.Threading;
using SimpleADTSConsole.Scripts;

namespace SimpleADTSConsole
{
    public interface IAdtsScript
    {
        string Name { get; }

        double Progress { get; set; }

        string Step { get; set; }

        void Start(IADTSTransportModel adts, CancellationToken cancel);
    }
}