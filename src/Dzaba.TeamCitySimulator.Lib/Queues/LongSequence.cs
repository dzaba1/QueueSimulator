namespace Dzaba.TeamCitySimulator.Lib.Queues;

internal sealed class LongSequence
{
    private long current = 0;

    public long Next()
    {
        current++;
        return current;
    }
}
