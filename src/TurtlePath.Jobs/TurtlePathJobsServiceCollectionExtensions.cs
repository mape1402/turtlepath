using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using TurtlePath.ExceptionHandling.Workers;

namespace TurtlePath.Jobs
{
    /// <summary>
    /// Provides service registration extensions for TurtlePath jobs.
    /// </summary>
    public static class TurtlePathJobsServiceCollectionExtensions
    {
        /// <summary>
        /// Registers TurtlePath job infrastructure.
        /// </summary>
        public static ITurtlePathJobsBuilder AddTurtlePathJobs(
            this IServiceCollection services,
            Action<TurtlePathJobManagerOptions> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddOptions();
            services.AddTurtlePathWorkerExceptionHandling();

            if (configure != null)
                services.Configure(configure);

            services.TryAddSingleton<ITurtlePathJobExecutor, TurtlePathJobExecutor>();
            services.TryAddSingleton<ITurtlePathJobManager, TurtlePathJobManager>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, TurtlePathCronJobHostedService>());

            return new TurtlePathJobsBuilder(services);
        }

        /// <summary>
        /// Registers a one-shot job.
        /// </summary>
        public static ITurtlePathJobsBuilder AddJob<TJob>(
            this ITurtlePathJobsBuilder builder,
            string name = null)
            where TJob : class, ITurtlePathJob
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.TryAddScoped<TJob>();
            builder.Services.AddSingleton(new TurtlePathJobDefinition
            {
                JobType = typeof(TJob),
                Name = name ?? typeof(TJob).Name
            });

            return builder;
        }

        /// <summary>
        /// Registers a recurring cron-style job.
        /// </summary>
        public static ITurtlePathJobsBuilder AddCronJob<TJob>(
            this ITurtlePathJobsBuilder builder,
            Action<TurtlePathCronJobOptions> configure = null,
            string name = null)
            where TJob : class, ITurtlePathJob
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            var options = new TurtlePathCronJobOptions();
            configure?.Invoke(options);

            builder.Services.TryAddScoped<TJob>();
            builder.Services.AddSingleton(new TurtlePathCronJobDefinition
            {
                JobType = typeof(TJob),
                Name = name ?? typeof(TJob).Name,
                Options = options
            });

            return builder;
        }

        /// <summary>
        /// Runs all registered one-shot jobs.
        /// </summary>
        public static Task<TurtlePathJobManagerResult> RunTurtlePathJobsAsync(
            this IServiceProvider serviceProvider,
            CancellationToken cancellationToken = default)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            return serviceProvider.GetRequiredService<ITurtlePathJobManager>().RunAsync(cancellationToken);
        }

        /// <summary>
        /// Runs selected registered one-shot jobs.
        /// </summary>
        public static Task<TurtlePathJobManagerResult> RunTurtlePathJobsAsync(
            this IServiceProvider serviceProvider,
            IEnumerable<Type> jobTypes,
            CancellationToken cancellationToken = default)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            return serviceProvider.GetRequiredService<ITurtlePathJobManager>().RunAsync(jobTypes, cancellationToken);
        }
    }
}
