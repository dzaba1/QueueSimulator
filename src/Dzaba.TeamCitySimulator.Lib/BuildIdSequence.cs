namespace Dzaba.TeamCitySimulator.Lib;

internal sealed class BuildIdSequence
{
    private long current = 0;

    public long Next()
    {
        current++;
        return current;
    }
}
