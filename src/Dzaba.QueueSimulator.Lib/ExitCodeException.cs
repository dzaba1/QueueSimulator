using System;

namespace Dzaba.QueueSimulator.Lib;

[Serializable]
public class ExitCodeException : Exception
{
    public ExitCode ExitCode { get; }

    public ExitCodeException(ExitCode exitCode)
    {
        ExitCode = exitCode;
    }

    public ExitCodeException(ExitCode exitCode, string message) : base(message)
    {
        ExitCode = exitCode;
    }

    public ExitCodeException(ExitCode exitCode, string message, Exception inner) : base(message, inner)
    {
        ExitCode = exitCode;
    }
}
