using DryIoc;
using NightRaven.Abstractions.Data.Seed;
using NightRaven.Core.Extensions.Container;

namespace NightRaven.Abstractions.Extensions.DryIoc;

/// <summary>
/// DryIoc-native registration helpers for NightRaven seed declarations.
/// </summary>
public static class SeedContainerExtensions
{
    extension(IContainer container)
    {
        /// <summary>
        /// Adds a boot-time seed action.
        /// </summary>
        public IContainer AddSeed(SeedAction action)
        {
            container.AddToRegisterTypedList(action);

            return container;
        }
    }
}
