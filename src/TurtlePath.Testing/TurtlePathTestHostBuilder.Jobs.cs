namespace TurtlePath.Testing
{
    using Microsoft.Extensions.DependencyInjection;
    using TurtlePath.Jobs;

    public sealed partial class TurtlePathTestHostBuilder
    {
        /// <summary>
        /// Registers TurtlePath job infrastructure for the test host.
        /// </summary>
        public TurtlePathTestHostBuilder UseJobs(Action<TurtlePathJobManagerOptions> configure = null)
            => ConfigureServices(services =>
            {
                services.AddLogging();
                services.AddTurtlePathJobs(configure);
            });

        /// <summary>
        /// Registers a one-shot TurtlePath job.
        /// </summary>
        public TurtlePathTestHostBuilder WithJob<TJob>(string name = null)
            where TJob : class, ITurtlePathJob
            => ConfigureServices(services =>
            {
                services.AddLogging();
                services.AddTurtlePathJobs().AddJob<TJob>(name);
            });

        /// <summary>
        /// Registers a recurring TurtlePath job.
        /// </summary>
        public TurtlePathTestHostBuilder WithCronJob<TJob>(
            Action<TurtlePathCronJobOptions> configure = null,
            string name = null)
            where TJob : class, ITurtlePathJob
            => ConfigureServices(services =>
            {
                services.AddLogging();
                services.AddTurtlePathJobs().AddCronJob<TJob>(configure, name);
            });
    }
}
