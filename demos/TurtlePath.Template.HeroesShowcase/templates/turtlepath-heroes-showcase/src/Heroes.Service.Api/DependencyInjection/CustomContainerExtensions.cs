using System.Diagnostics.CodeAnalysis;
using Heroes.Service.Business.Jobs;
using Heroes.Service.Business.Services.Audit;
using Heroes.Service.Business.Services.Incident;
using Heroes.Service.Business.Services.ThreatScoring;
using TurtlePath.Hooks;
using TurtlePath.Jobs;

namespace Microsoft.Extensions.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    internal static class CustomContainerExtensions
    {
        internal static IServiceCollection AddCustomContainer(this IServiceCollection services)
        {
            // Keep project-specific dependencies here. Defaults stay framework-owned and easy to update.
            services.AddSingleton<IAuditTrail, InMemoryAuditTrail>();
            services.AddScoped<IThreatScoringService, ThreatScoringService>();
            services.AddScoped<IIncidentAssignmentService, IncidentAssignmentService>();
            services.AddHandlerHooksFromAssemblyContaining<AuditAfterSaveHook>();

            services.AddTurtlePathJobs(options =>
            {
                options.ExecutionMode = TurtlePathJobExecutionMode.Parallel;
                options.MaxDegreeOfParallelism = 3;
                options.Retries = 2;
                options.RetryDelay = TimeSpan.FromSeconds(5);
                options.FailureBehavior = TurtlePathJobFailureBehavior.Continue;
            })
            .AddJob<SeedHeroesUniverseJob>("seed-heroes-universe")
            .AddCronJob<AutoAssignOpenIncidentsJob>(options =>
            {
                options.EveryMinutes(5);
                options.RunOnStart = false;
                options.Retries = 2;
                options.FailureBehavior = TurtlePathJobFailureBehavior.Continue;
            }, "auto-assign-open-incidents")
            .AddCronJob<RecalculateTeamReputationJob>(options =>
            {
                options.EveryMinutes(15);
                options.RunOnStart = true;
                options.Retries = 1;
                options.FailureBehavior = TurtlePathJobFailureBehavior.Continue;
            }, "recalculate-team-reputation");

            return services;
        }
    }
}
