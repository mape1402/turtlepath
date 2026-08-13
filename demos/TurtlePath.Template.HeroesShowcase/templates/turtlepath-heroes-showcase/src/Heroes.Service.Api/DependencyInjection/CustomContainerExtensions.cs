using System.Diagnostics.CodeAnalysis;
using Heroes.Service.Business.Heroes.Services.OperationsReport;
using Heroes.Service.Business.Incidents.Services.Assignment;
using Heroes.Service.Business.Incidents.Services.Backlog;
using Heroes.Service.Business.Incidents.Services.ThreatScoring;
using Heroes.Service.Business.Incidents.Services.Workflow;
using Heroes.Service.Business.Jobs;
using Heroes.Service.Business.Jobs.Services.Universe;
using Heroes.Service.Business.Services.Audit;
using Heroes.Service.Business.Teams.Services.Reputation;
using Heroes.Service.Persistence.Repositories.Heroes;
using TurtlePath.Jobs;

namespace Microsoft.Extensions.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    internal static class CustomContainerExtensions
    {
        internal static IServiceCollection AddCustomContainer(this IServiceCollection services, IConfiguration configuration)
        {
            // Keep project-specific dependencies here. Defaults stay framework-owned and easy to update.
            services.AddSingleton<IAuditTrail, InMemoryAuditTrail>();

            services.AddScoped<IHeroOperationsReportService, HeroOperationsReportService>();
            var connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string 'Default' is required.");

            services.AddScoped<IHeroOperationsReadRepository>(_ =>
                new AdoHeroOperationsReadRepository(connectionString));

            services.AddScoped<IThreatScoringService, ThreatScoringService>();
            services.AddScoped<IIncidentAssignmentService, IncidentAssignmentService>();
            services.AddScoped<IIncidentBacklogService, IncidentBacklogService>();
            services.AddScoped<IIncidentWorkflowService, IncidentWorkflowService>();

            services.AddScoped<ITeamReputationService, TeamReputationService>();

            services.AddScoped<IHeroesUniverseSeeder, HeroesUniverseSeeder>();

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
