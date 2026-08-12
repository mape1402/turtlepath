using TurtlePath.Template.Api.Boundaries;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    internal static class TransactionBoundaryExtensions
    {
        internal static IServiceCollection AddTransactionBoundaryDefaults(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<TransactionBoundaryOptions>(configuration.GetSection("TransactionBoundary"));
            services.PostConfigure<TransactionBoundaryOptions>(options =>
            {
                options.DiscoverRequestsFrom(typeof(TurtlePath.Template.Business.Constants).Assembly);
                options.DiscoverRequestsFrom(typeof(TransactionBoundaryExtensions).Assembly);

                foreach (var profile in DiscoverProfiles(typeof(TurtlePath.Template.Business.Constants).Assembly, typeof(TransactionBoundaryExtensions).Assembly))
                    profile.Configure(options);
            });

            services.AddSingleton<ITransactionBoundaryRequestFilter>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<TransactionBoundaryOptions>>();
                var filter = new TransactionBoundaryRequestFilter(options);

                filter.Discover(options.Value.RequestAssemblies.ToArray());

                return filter;
            });

            return services;
        }

        private static IEnumerable<ITransactionBoundaryProfile> DiscoverProfiles(params Assembly[] assemblies)
        {
            foreach (var type in assemblies
                .Where(assembly => assembly != null)
                .SelectMany(GetLoadableTypes)
                .Where(type => type is { IsClass: true, IsAbstract: false } &&
                               typeof(ITransactionBoundaryProfile).IsAssignableFrom(type)))
            {
                if (Activator.CreateInstance(type) is ITransactionBoundaryProfile profile)
                    yield return profile;
            }
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                return exception.Types.Where(type => type != null);
            }
        }
    }
}
