namespace NightHeaven.Server.Data;

/// <summary>
/// Version information returned by the <c>GET /api/version</c> endpoint.
/// </summary>
public sealed record ServerVersionInfo(string Version, string Codename);
