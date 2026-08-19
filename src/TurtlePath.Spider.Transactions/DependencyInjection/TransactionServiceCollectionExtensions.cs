using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using TurtlePath;
using TurtlePath.Spider.Transactions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registers the TurtlePath transaction boundary in a Spider pipeline.
/// </summary>
[ExcludeFromCodeCoverage]
public static class TransactionServiceCollectionExtensions
{
    /// <summary>
    /// Adds the transaction boundary directly to a service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddTurtlePathSpiderTransactions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        Register(services, configuration);
        return services;
    }

    /// <summary>
    /// Adds transaction boundary options, profile discovery, request discovery, and the Spider boundary.
    /// </summary>
    /// <param name="builder">The TurtlePath registration builder.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The same TurtlePath builder.</returns>
    public static ITurtlePathBuilder UseSpiderTransactions(
        this ITurtlePathBuilder builder,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var services = builder.Services;
        Register(services, configuration);
        return builder;
    }

    private static void Register(IServiceCollection services, IConfiguration configuration)
    {
        if (configuration != null)
            services.Configure<TransactionBoundaryOptions>(configuration.GetSection("TransactionBoundary"));
        else
            services.AddOptions<TransactionBoundaryOptions>();
        services.PostConfigure<TransactionBoundaryOptions>(options =>
        {
            var assemblies = GetApplicationAssemblies();
            foreach (var assembly in assemblies)
                options.DiscoverRequestsFrom(assembly);

            foreach (var profile in DiscoverProfiles(assemblies))
                profile.Configure(options);
        });

        services.AddSingleton<ITransactionBoundaryRequestFilter>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<TransactionBoundaryOptions>>();
            var filter = new TransactionBoundaryRequestFilter(options);
            filter.Discover(options.Value.RequestAssemblies.ToArray());
            return filter;
        });

        services.AddSpider(spider =>
        {
            spider.AddExecutionBoundary<TransactionExecutionBoundary>();
        });
    }

    private static IEnumerable<ITransactionBoundaryProfile> DiscoverProfiles(IEnumerable<Assembly> assemblies)
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

    private static IReadOnlyList<Assembly> GetApplicationAssemblies()
    {
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly => assembly is not null && !IsTestAssembly(assembly))
            .Distinct()
            .ToArray();
    }

    private static bool IsTestAssembly(Assembly assembly)
    {
        var name = assembly.GetName().Name ?? string.Empty;
        return name.EndsWith(".Tests", StringComparison.OrdinalIgnoreCase) ||
               name.EndsWith(".Test", StringComparison.OrdinalIgnoreCase) ||
               name.Contains(".TestHost", StringComparison.OrdinalIgnoreCase);
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
