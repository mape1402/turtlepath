using Heroes.Service.Persistence;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides optional Event Sourcing registration for the demo host.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class EventSourcingExtensions
    {
        /// <summary>
        /// Registers the EF Core event store used when TurtlePath Event Sourcing profiles are enabled.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The same service collection.</returns>
        internal static IServiceCollection AddEventSourcingDefaults(this IServiceCollection services)
        {
            services.AddKrackendEntityFrameworkEventStore<AppDbContext>();

            return services;
        }
    }
}
