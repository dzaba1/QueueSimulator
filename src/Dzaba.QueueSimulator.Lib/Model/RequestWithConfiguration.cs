namespace Dzaba.QueueSimulator.Lib.Model;

internal sealed class RequestWithConfiguration
{
    public Request Request { get; init; }
    public RequestConfiguration RequestConfiguration { get; init; }
}
