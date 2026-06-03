using NightHeaven.Network.Client;

namespace NightHeaven.Network.Events;

/// <summary>
/// Event payload containing an exception raised by server or client network loops.
/// </summary>
public sealed class NightHeavenTCPExceptionEventArgs : EventArgs
{
    public NightHeavenTCPExceptionEventArgs(Exception exception, NightHeavenTCPClient? client = null)
    {
        Exception = exception;
        Client = client;
    }

    /// <summary>
    /// Exception raised by the networking component.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Client related to the exception, when available.
    /// </summary>
    public NightHeavenTCPClient? Client { get; }
}
