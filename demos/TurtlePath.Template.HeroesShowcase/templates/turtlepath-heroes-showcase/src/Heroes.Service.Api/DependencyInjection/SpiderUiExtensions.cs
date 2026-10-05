using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Spider.Pipelines.Architecture;
using Spider.Pipelines.RuntimeTracing;
using Spider.Pipelines.Web;
using Heroes.Service.Business;
using TurtlePath.Automations;
using TurtlePath.Commands;
using TurtlePath.Spider.Transactions;

namespace Microsoft.Extensions.DependencyInjection
{
    internal static class SpiderUiExtensions
    {
        private const string ArchitectureEndpoint = "/_spider";
        private const string RuntimeTracesEndpoint = "/_spider/runtime/traces";

        internal static IServiceCollection AddSpiderDevelopmentDefaults(this IServiceCollection services, IHostEnvironment environment)
        {
            if (!environment.IsDevelopment())
                return services;

            services.AddSpiderRuntimeTracing(tracing =>
            {
                tracing.Verbosity = SpiderTraceVerbosity.Normal;
                tracing.QueueCapacity = 1000;
                tracing.Backpressure = SpiderTraceBackpressure.DropOldest;
                tracing.UseInMemoryStore(options =>
                {
                    options.MaxTraces = 100;
                    options.MaxEventsPerTrace = 500;
                    options.TraceTtl = TimeSpan.FromMinutes(60);
                });
            });

            return services;
        }

        internal static IEndpointRouteBuilder MapSpiderDevelopmentEndpoints(this IEndpointRouteBuilder endpoints, IWebHostEnvironment environment)
        {
            if (!environment.IsDevelopment())
                return endpoints;

            endpoints.MapGet(ArchitectureEndpoint, RenderArchitectureAsync);
            endpoints.MapGet(RuntimeTracesEndpoint, RenderRuntimeTracesAsync);

            return endpoints;
        }

        private static async Task<IResult> RenderArchitectureAsync(HttpContext context, CancellationToken cancellationToken)
        {
            var snapshot = await ReadRuntimeTracesAsync(context.RequestServices, cancellationToken);
            var renderer = new SpiderArchitectureWebRenderer();
            var html = renderer.Render(BuildManifest(), new SpiderArchitectureWebOptions
            {
                Title = "Heroes Spider",
                IncludeRuntimeTraces = true,
                RuntimeTracesEndpoint = RuntimeTracesEndpoint,
                RuntimeTraceSummaries = snapshot.Summaries,
                RuntimeTraces = snapshot.Traces,
            });

            return Results.Content(html, "text/html; charset=utf-8");
        }

        private static async Task<IResult> RenderRuntimeTracesAsync(HttpContext context, CancellationToken cancellationToken)
        {
            var snapshot = await ReadRuntimeTracesAsync(context.RequestServices, cancellationToken);
            var serializer = new SpiderRuntimeTraceWebSerializer();
            var json = serializer.Serialize(snapshot.Summaries, snapshot.Traces);

            return Results.Content(json, "application/json");
        }

        private static async Task<RuntimeTraceSnapshot> ReadRuntimeTracesAsync(IServiceProvider services, CancellationToken cancellationToken)
        {
            var reader = services.GetService<ISpiderTraceReader>();
            if (reader is null)
                return new RuntimeTraceSnapshot([], []);

            var summaries = new List<SpiderTraceSummary>();
            await foreach (var summary in reader.QueryAsync(new SpiderTraceQuery { Limit = 50 }, cancellationToken).WithCancellation(cancellationToken))
                summaries.Add(summary);

            var traces = new List<SpiderTrace>();
            foreach (var summary in summaries)
            {
                var trace = await reader.GetAsync(summary.TraceId, cancellationToken);
                if (trace is not null)
                    traces.Add(trace);
            }

            return new RuntimeTraceSnapshot(summaries, traces);
        }

        private static SpiderArchitectureManifest BuildManifest()
            => MergeManifests(
                TurtlePathCommandArchitecture.BuildManifest(),
                TurtlePathTransactionArchitecture.BuildManifest(),
                TurtlePathAutomationArchitecture.BuildManifest(typeof(Constants).Assembly),
                BuildHeroesManifest());

        private static SpiderArchitectureManifest BuildHeroesManifest()
        {
            var components = new List<SpiderComponentDescriptor>
            {
                Component("heroes.profile.report", "spider.flow-profile", "Heroes.Report", "Demo profile used by custom Heroes report handlers.", "heroes,report,profile"),
            };
            var relations = new List<SpiderRelationDescriptor>();

            AddCustomReportFlow(components, relations);

            return new SpiderArchitectureManifest(components, relations);
        }

        private static void AddCustomReportFlow(
            ICollection<SpiderComponentDescriptor> components,
            ICollection<SpiderRelationDescriptor> relations)
        {
            const string flowId = "heroes.report.operations";
            const string stepId = "heroes.report.operations.read";

            components.Add(Component(flowId, "spider.flow", "Build hero operations report", "Runs the Heroes demo custom ADO.NET read model and returns the aggregated operations report.", "heroes,custom-handler,report,read-model"));
            components.Add(Component(stepId, "spider.flow-step", "Read operations report", "Delegates to the feature service that composes the non-standard operations report.", "ado-net,report,read-model"));
            relations.Add(Relation($"{flowId}.uses-profile", flowId, "heroes.profile.report", "uses-profile"));
            relations.Add(Relation($"{flowId}.contains.read", flowId, stepId, "contains"));
        }

        private static SpiderComponentDescriptor Component(
            string id,
            string kind,
            string displayName,
            string description,
            string tags)
            => new(id, kind, displayName, new Dictionary<string, string>
            {
                ["description"] = description,
                ["tags"] = tags,
            });

        private static SpiderRelationDescriptor Relation(string id, string sourceId, string targetId, string kind)
            => new(id, sourceId, targetId, kind, new Dictionary<string, string>());

        private static SpiderArchitectureManifest MergeManifests(params SpiderArchitectureManifest[] manifests)
        {
            var components = new Dictionary<string, SpiderComponentDescriptor>(StringComparer.Ordinal);
            var relations = new Dictionary<string, SpiderRelationDescriptor>(StringComparer.Ordinal);

            foreach (var manifest in manifests)
            {
                foreach (var component in manifest.Components)
                    components.TryAdd(component.Id, component);

                foreach (var relation in manifest.Relations)
                    relations.TryAdd(relation.Id, relation);
            }

            return new SpiderArchitectureManifest(components.Values.ToArray(), relations.Values.ToArray());
        }

        private sealed record RuntimeTraceSnapshot(
            IReadOnlyCollection<SpiderTraceSummary> Summaries,
            IReadOnlyCollection<SpiderTrace> Traces);
    }
}
