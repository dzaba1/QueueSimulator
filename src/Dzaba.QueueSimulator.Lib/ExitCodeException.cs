using System;
using System.Collections.Generic;
using System.Linq;

namespace Dzaba.QueueSimulator.Lib;

[Serializable]
public class ExitCodeException : Exception
{
    public KeyValuePair<ExitCode, string>[] Errors { get; }

    public ExitCodeException(IEnumerable<KeyValuePair<ExitCode, string>> errors)
    {
        ArgumentNullException.ThrowIfNull(errors, nameof(errors));

        Errors = errors.ToArray();
    }

    public ExitCodeException(string message, IEnumerable<KeyValuePair<ExitCode, string>> errors)
        : base(message)
    {
        ArgumentNullException.ThrowIfNull(errors, nameof(errors));

        Errors = errors.ToArray();
    }

    public ExitCodeException(string message, IEnumerable<KeyValuePair<ExitCode, string>> errors, Exception inner)
        : base(message, inner)
    {
        ArgumentNullException.ThrowIfNull(errors, nameof(errors));

        Errors = errors.ToArray();
    }
}
