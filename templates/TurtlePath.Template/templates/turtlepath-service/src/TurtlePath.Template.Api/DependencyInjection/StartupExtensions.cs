using Serilog;
using System.Diagnostics.CodeAnalysis;
#if (!JobHost)
using TurtlePath.Template.Api.DependencyInjection;
#endif
#if (JobHost)
using TurtlePath.Jobs;
#endif

namespace Microsoft.Extensions.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    /// <summary>
    /// Provides startup service configuration extensions.
    /// </summary>
    public static class StartupExtensions
    {
#if (!JobHost)
        /// <summary>
        /// Registers default services and middleware dependencies.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="environment">The hosting environment.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddDefaults(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            return services
                .AddMvcDefaults()
                .AddOpenApiDefaults()
                .AddHealthCheckDefaults(configuration)
                .AddPersistenceDefaults(configuration)
                .AddApplicationDefaults()
                // Event sourcing is ready but intentionally opt-in because the service must define streams and event tables.
                // Uncomment this together with .UseEventSourcingProfiles(...) in AddApplicationDefaults.
                // .AddEventSourcingDefaults()
                // Pigeon is ready but intentionally opt-in because Azure Service Bus requires a real connection string.
                // Uncomment this line when the service needs consumers, producers, or the EF Core outbox.
                // .AddMessagingDefaults(configuration)
                .AddPipelineDefaults(configuration)
                .AddCustomContainer();
        }

        /// <summary>
        /// Configures default middleware for the application.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="environment">The hosting environment.</param>
        /// <returns>The application builder.</returns>
        public static IApplicationBuilder UseDefaults(this IApplicationBuilder app, IWebHostEnvironment environment)
        {
            app.UseLoggingDefaults(environment);
            app.UseOpenApiDefaults(environment);
            app.UseRoutingDefaults();
            app.UseEndpointDefaults();

            return app;
        }

        private static IApplicationBuilder UseLoggingDefaults(this IApplicationBuilder app, IWebHostEnvironment environment)
        {
            if (environment.IsDevelopment())
                app.UseSerilogRequestLogging();

            return app;
        }

        private static IApplicationBuilder UseRoutingDefaults(this IApplicationBuilder app)
        {
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            return app;
        }

        private static IApplicationBuilder UseEndpointDefaults(this IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthCheckEndPoints();
                endpoints.MapControllers();
            });

            return app;
        }
#endif

#if (JobHost)
        /// <summary>
        /// Registers default services for one-shot job execution.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="environment">The host environment.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddJobDefaults(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            services
                .AddPersistenceDefaults(configuration)
                .AddApplicationDefaults()
                .AddPipelineDefaults(configuration)
                .AddJobExceptionHandlingDefaults()
                .AddCustomContainer();

            services.AddTurtlePathJobs(options =>
            {
                options.ExecutionMode = TurtlePathJobExecutionMode.Parallel;
                options.MaxDegreeOfParallelism = Environment.ProcessorCount;
                options.FailureBehavior = TurtlePathJobFailureBehavior.Rethrow;
            });

            return services;
        }
#endif
    }
}
