namespace Dzaba.TeamCitySimulator.Lib.Repositories;

internal sealed class LongSequence
{
    private long current = 0;

    public long Next()
    {
        current++;
        return current;
    }
}
