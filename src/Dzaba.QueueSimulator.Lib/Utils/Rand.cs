using System;

namespace Dzaba.QueueSimulator.Lib.Utils;

public interface IRand
{
    int Next();
    int Next(int max);
    int Next(int min, int max);
    long NextLong();
    long NextLong(long max);
    long NextLong(long min, long max);
    double NextDouble();
}

internal sealed class Rand : IRand
{
    private readonly Random random = Random.Shared;

    public int Next()
    {
        return random.Next();
    }

    public int Next(int max)
    {
        return random.Next(max);
    }

    public int Next(int min, int max)
    {
        return random.Next(min, max);
    }

    public double NextDouble()
    {
        return random.NextDouble();
    }

    public long NextLong()
    {
        return random.NextInt64();
    }

    public long NextLong(long max)
    {
        return random.NextInt64(max);
    }

    public long NextLong(long min, long max)
    {
        return random.NextInt64(min, max);
    }
}
