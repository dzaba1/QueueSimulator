using Dzaba.QueueSimulator.Lib.Model.Distribution;
using Dzaba.QueueSimulator.Lib.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;

namespace Dzaba.QueueSimulator.Lib.Model;

[DebuggerDisplay("{Name}")]
public sealed class RequestConfiguration
{
    [Required(AllowEmptyStrings = false)]
    public string Name { get; set; }

    public IDuration Duration { get; set; }

    public string[] CompatibleAgents { get; set; }

    public string[] RequestDependencies { get; set; }
    public bool IsComposite { get; set; }

    public IEnumerable<RequestConfiguration> ResolveDependencies(SimulationPayload simulationPayload, bool recursive)
    {
        ArgumentNullException.ThrowIfNull(simulationPayload, nameof(simulationPayload));

        var comparer = new StringPropertyEqualityComparer<RequestConfiguration>(r => r.Name, StringComparer.OrdinalIgnoreCase);

        return ResolveDependenciesInternal(simulationPayload, recursive)
            .Distinct(comparer);
    }

    private IEnumerable<RequestConfiguration> ResolveDependenciesInternal(SimulationPayload simulationPayload, bool recursive)
    {
        if (RequestDependencies == null)
        {
            yield break;
        }

        var current = RequestDependencies
            .Select(simulationPayload.RequestConfigurations.GetEntity);

        foreach (var dep in current)
        {
            yield return dep;

            if (recursive)
            {
                var subDeps = dep.ResolveDependenciesInternal(simulationPayload, true);
                foreach (var subDep in subDeps)
                {
                    yield return subDep;
                }
            }
        }
    }
}
