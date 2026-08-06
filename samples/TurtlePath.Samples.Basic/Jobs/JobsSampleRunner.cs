using Microsoft.Extensions.DependencyInjection;
using TurtlePath.ExceptionHandling;
using TurtlePath.Jobs;

namespace TurtlePath.Samples.Basic.Jobs;

public static class JobsSampleRunner
{
    public static async Task<IReadOnlyList<string>> RunAsync()
    {
        var services = new ServiceCollection();
        var jobLog = new SampleJobLog();

        services.AddLogging();
        services.AddSingleton(jobLog);
        services.AddTurtlePathExceptionHandlingCore();

        services
            .AddTurtlePathJobs(options =>
            {
                options.ExecutionMode = TurtlePathJobExecutionMode.Parallel;
                options.MaxDegreeOfParallelism = 2;
                options.Retries = 1;
                options.FailureBehavior = TurtlePathJobFailureBehavior.Rethrow;
            })
            .AddJob<ImportCustomersJob>("import-customers")
            .AddJob<ImportInvoicesJob>("import-invoices")
            .AddCronJob<RefreshCatalogJob>(options =>
            {
                options.EveryMinutes(30);
                options.Retries = 3;
                options.FailureBehavior = TurtlePathJobFailureBehavior.Continue;
            }, "refresh-catalog")
            .AddCronJob<ImportCustomersJob>(options =>
            {
                options.EveryHours(6);
                options.FailureBehavior = TurtlePathJobFailureBehavior.StopHost;
            }, "scheduled-customer-import");

        using var provider = services.BuildServiceProvider();
        var result = await provider.RunTurtlePathJobsAsync();
        var cronJobs = provider.GetServices<TurtlePathCronJobDefinition>().ToArray();

        var lines = new List<string>
        {
            $"one-shot manager completed: succeeded={result.Succeeded}, jobs={result.Jobs.Count}",
            $"registered recurring cron jobs: {string.Join(", ", cronJobs.Select(job => $"{job.Name} every {job.Options.Interval}"))}"
        };

        lines.AddRange(jobLog.Entries.Select(entry => $"job log: {entry}"));

        return lines;
    }
}
