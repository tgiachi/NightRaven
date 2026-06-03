using NightRaven.Network.Client;

namespace NightRaven.Network.Events;

/// <summary>
/// Event payload containing an exception raised by server or client network loops.
/// </summary>
public sealed class NightRavenTCPExceptionEventArgs : EventArgs
{
    public NightRavenTCPExceptionEventArgs(Exception exception, NightRavenTCPClient? client = null)
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
    public NightRavenTCPClient? Client { get; }
}
