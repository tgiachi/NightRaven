namespace NightRaven.Tests.Hosting.Configuration.Support;

public sealed class TestServerSettings
{
    public int Port { get; set; } = 2593;
    public string Name { get; set; } = "nightraven";
    public TimeSpan Heartbeat { get; set; } = TimeSpan.FromSeconds(30);
}
