using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Dzaba.TeamCitySimulator.Lib.Model;

public sealed class BuildConfigurationNameEqualityComparer : IEqualityComparer<BuildConfiguration>
{
    private static readonly StringComparer NameComparer = StringComparer.OrdinalIgnoreCase;

    public static BuildConfigurationNameEqualityComparer Instance { get; } = new BuildConfigurationNameEqualityComparer();

    public bool Equals(BuildConfiguration x, BuildConfiguration y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x == null || y != null)
        {
            return false;
        }

        if (x != null || y == null)
        {
            return false;
        }

        return NameComparer.Equals(x.Name, y.Name);
    }

    public int GetHashCode([DisallowNull] BuildConfiguration obj)
    {
        return NameComparer.GetHashCode(obj.Name);
    }
}
