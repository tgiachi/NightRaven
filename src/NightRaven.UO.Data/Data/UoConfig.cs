using NightRaven.Abstractions.Interfaces.Config;
using NightRaven.Core.Extensions.Directories;

namespace NightRaven.UO.Data.Data;

/// <summary>
/// Configuration for the Ultima Online client data files (idx/mul/uop). Bound from the <c>uo</c>
/// TOML section. Validation fails boot when the directory or the minimum required files are absent.
/// </summary>
public sealed class UoConfig : IValidatableConfig
{
    /// <summary>
    /// Directory containing the UO client data files. Supports <c>~</c> and environment variables.
    /// Default: <c>~/uo</c>.
    /// </summary>
    public string ClientFilesDirectory { get; set; } = "~/uo";

    /// <inheritdoc />
    public IEnumerable<string> Validate()
    {
        var directory = ClientFilesDirectory?.ResolvePathAndEnvs();

        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
        {
            yield return $"client files directory '{ClientFilesDirectory}' does not exist";

            yield break;
        }

        if (!File.Exists(Path.Combine(directory, "tiledata.mul")))
        {
            yield return $"required file 'tiledata.mul' was not found in '{directory}'";
        }
    }
}
