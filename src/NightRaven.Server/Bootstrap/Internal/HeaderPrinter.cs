using System.Reflection;
using NightRaven.Core.Utils;

namespace NightRaven.Server.Bootstrap.Internal;

internal static class HeaderPrinter
{
    public static void Print(NightRavenBootstrapContext context, bool showHeader)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (showHeader)
        {
            var headerContent = ResourceUtils.GetEmbeddedResourceString(
                Assembly.GetExecutingAssembly(),
                "Assets/header.txt"
            );

            Console.WriteLine(headerContent);
        }

        Console.WriteLine($"NightRaven UO Server v{VersionUtils.GetVersion()}");
        Console.WriteLine($"Root Directory: {context.Directories.Root}");
    }
}
