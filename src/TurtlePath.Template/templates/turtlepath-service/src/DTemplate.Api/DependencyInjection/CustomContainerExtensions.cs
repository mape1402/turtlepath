using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    internal static class CustomContainerExtensions
    {
        internal static IServiceCollection AddCustomContainer(this IServiceCollection services)
        {
            // Register service-specific dependencies here.
            return services;
        }
    }
}
