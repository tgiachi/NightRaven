namespace NightRaven.Server.Bootstrap;

public sealed class NightRavenBootstrapOptions
{
    public NightRavenBootstrapOptions(string[] args, string? rootDirectory, bool debug, bool showHeader)
    {
        Args = args;
        RootDirectory = rootDirectory;
        Debug = debug;
        ShowHeader = showHeader;
    }

    public string[] Args { get; }

    public bool Debug { get; }

    public string? RootDirectory { get; }

    public bool ShowHeader { get; }
}
