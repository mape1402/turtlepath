namespace TurtlePath.Automations
{
    using System.Reflection;
    using System.Text;
    using Spider.Pipelines.Architecture;
    using TurtlePath.Automations.Descriptors;

    /// <summary>
    /// Builds Spider architecture metadata for TurtlePath automation declarations.
    /// </summary>
    public static class TurtlePathAutomationArchitecture
    {
        private const string AutomationProfileId = "turtlepath.profile.automation";

        /// <summary>
        /// Discovers automation declarations from the provided assemblies and builds a Spider architecture manifest.
        /// </summary>
        /// <param name="assemblies">Assemblies that contain TurtlePath automation attributes or profiles.</param>
        /// <returns>A Spider architecture manifest that describes the discovered automation operations.</returns>
        public static SpiderArchitectureManifest BuildManifest(params Assembly[] assemblies)
            => BuildManifest(AutomationDescriptorDiscovery.Discover(assemblies));

        private static SpiderArchitectureManifest BuildManifest(IReadOnlyCollection<AutomationDescriptor> descriptors)
        {
            var components = new List<SpiderComponentDescriptor>
            {
                Component(
                    AutomationProfileId,
                    "spider.flow-profile",
                    "TurtlePath.Automation",
                    new Dictionary<string, string>
                    {
                        ["description"] = "Profile used to group the concrete automation operations discovered from attributes and automation profiles.",
                        ["tags"] = "turtlepath,automation,profile",
                    }),
            };
            var relations = new List<SpiderRelationDescriptor>();

            foreach (var descriptor in descriptors.OrderBy(item => item.EntityType.FullName).ThenBy(item => item.OperationKind).ThenBy(item => item.RequestType.FullName))
                AddAutomationFlow(components, relations, descriptor);

            return new SpiderArchitectureManifest(components, relations);
        }

        private static void AddAutomationFlow(
            ICollection<SpiderComponentDescriptor> components,
            ICollection<SpiderRelationDescriptor> relations,
            AutomationDescriptor descriptor)
        {
            var operationName = ToOperationName(descriptor.OperationKind);
            var flowId = $"turtlepath.automation.{operationName}.{Sanitize(descriptor.RequestType.FullName ?? descriptor.RequestType.Name)}";
            var flowMetadata = new Dictionary<string, string>
            {
                ["description"] = CreateDescription(descriptor),
                ["tags"] = CreateTags(descriptor),
                ["operation"] = operationName,
                ["request.type"] = descriptor.RequestType.FullName ?? descriptor.RequestType.Name,
                ["entity.type"] = descriptor.EntityType.FullName ?? descriptor.EntityType.Name,
                ["key.type"] = descriptor.KeyType.FullName ?? descriptor.KeyType.Name,
                ["return.mode"] = descriptor.ReturnMode.ToString(),
                ["source.kind"] = descriptor.SourceKind.ToString(),
            };

            if (descriptor.ResponseType is not null)
                flowMetadata["response.type"] = descriptor.ResponseType.FullName ?? descriptor.ResponseType.Name;

            if (descriptor.ValidateRequest.HasValue)
                flowMetadata["validation.enabled"] = descriptor.ValidateRequest.Value.ToString();

            if (descriptor.ReloadBeforeResponse)
                flowMetadata["projection.reload-before-response"] = "True";

            if (!string.IsNullOrWhiteSpace(descriptor.DefaultSortProperty))
                flowMetadata["query.default-sort"] = descriptor.DefaultSortProperty;

            if (descriptor.ResponseIncludeExpressions.Count > 0)
                flowMetadata["projection.includes"] = descriptor.ResponseIncludeExpressions.Count.ToString();

            components.Add(Component(flowId, "spider.flow", CreateDisplayName(descriptor), flowMetadata));
            relations.Add(Relation($"{flowId}.uses-profile", flowId, AutomationProfileId, "uses-profile"));

            var baseFlowId = ResolveBaseFlowId(descriptor.OperationKind);
            if (baseFlowId is not null)
                relations.Add(Relation($"{flowId}.implements", flowId, baseFlowId, "implements"));

            AddSteps(components, relations, flowId, descriptor);
        }

        private static void AddSteps(
            ICollection<SpiderComponentDescriptor> components,
            ICollection<SpiderRelationDescriptor> relations,
            string flowId,
            AutomationDescriptor descriptor)
        {
            string previousStepId = null;
            foreach (var step in ResolveSteps(descriptor))
            {
                var stepId = $"{flowId}.{step.Id}";
                components.Add(Component(stepId, "spider.flow-step", step.DisplayName, new Dictionary<string, string>
                {
                    ["description"] = step.Description,
                    ["tags"] = step.Tags,
                }));
                relations.Add(Relation($"{flowId}.contains.{step.Id}", flowId, stepId, "contains"));

                if (previousStepId is not null)
                    relations.Add(Relation($"{previousStepId}.next.{step.Id}", previousStepId, stepId, "next"));

                previousStepId = stepId;
            }
        }

        private static IReadOnlyList<AutomationStep> ResolveSteps(AutomationDescriptor descriptor)
            => descriptor.OperationKind switch
            {
                AutomationOperationKind.Create => MutationSteps(descriptor, [
                    new("validate", "Validate request", ResolveValidationDescription(descriptor), "validation"),
                    new("map", "Map entity", "Maps the request into a new entity instance.", "mapping"),
                    new("save", "Save entity", "Persists the created entity.", "persistence"),
                ]),
                AutomationOperationKind.Update => MutationSteps(descriptor, [
                    new("load", "Load entity", "Loads the entity targeted by the request.", "lookup"),
                    new("validate", "Validate request", ResolveValidationDescription(descriptor), "validation"),
                    new("map", "Map entity", "Maps the request values into the loaded entity.", "mapping"),
                    new("save", "Save entity", "Persists the updated entity.", "persistence"),
                ]),
                AutomationOperationKind.Patch => MutationSteps(descriptor, [
                    new("load", "Load entity", "Loads the entity targeted by the patch request.", "lookup"),
                    new("validate", "Validate request", ResolveValidationDescription(descriptor), "validation"),
                    new("patch", "Patch entity", "Applies partial changes to the loaded entity.", "patch"),
                    new("save", "Save entity", "Persists the patched entity.", "persistence"),
                ]),
                AutomationOperationKind.Delete => MutationSteps(descriptor, [
                    new("load", "Load entity", "Loads the entity targeted by the delete request.", "lookup"),
                    new("validate", "Validate request", ResolveValidationDescription(descriptor), "validation"),
                    new("delete", "Delete entity", "Removes the loaded entity from storage.", "delete,persistence"),
                ]),
                AutomationOperationKind.GetById => [
                    new("key", "Resolve key", "Resolves the entity key from the query.", "query,key"),
                    new("read", "Read entity", "Reads the entity matching the resolved key.", "query,lookup"),
                    new("response", "Map response", "Maps the entity into the query response.", "response,projection"),
                ],
                AutomationOperationKind.GetOne => [
                    new("filter", "Build filter", "Builds the filter used to read a single entity.", "query,filter"),
                    new("read", "Read entity", "Reads the entity matching the query filter.", "query,lookup"),
                    new("response", "Map response", "Maps the entity into the query response.", "response,projection"),
                ],
                AutomationOperationKind.GetMany => [
                    new("filter", "Build query", "Builds filters and default sorting for the collection query.", "query,filter,sort"),
                    new("read", "Read collection", "Reads and maps the matching entity collection.", "query,collection,projection"),
                ],
                AutomationOperationKind.GetPaged => [
                    new("filter", "Build query", "Builds filters and default sorting for the paged query.", "query,filter,sort"),
                    new("page", "Read page", "Reads and maps the requested page of entities.", "query,paging,projection"),
                ],
                _ => []
            };

        private static IReadOnlyList<AutomationStep> MutationSteps(AutomationDescriptor descriptor, IReadOnlyList<AutomationStep> steps)
        {
            if (!descriptor.HasResponse)
                return steps;

            return steps
                .Concat([
                    new(
                        "response",
                        "Map response",
                        descriptor.ReloadBeforeResponse
                            ? "Reloads the saved entity before mapping the command response."
                            : "Maps the saved entity to the command response.",
                        descriptor.ReloadBeforeResponse ? "response,projection,reload" : "response,projection"),
                ])
                .ToArray();
        }

        private static string ResolveValidationDescription(AutomationDescriptor descriptor)
            => descriptor.ValidateRequest switch
            {
                true => "Validation is explicitly enabled for this operation.",
                false => "Validation is explicitly disabled for this operation.",
                _ => "Validation follows the TurtlePath default for this operation kind.",
            };

        private static string CreateDisplayName(AutomationDescriptor descriptor)
        {
            var responseName = descriptor.ResponseType is null ? "void" : GetFriendlyName(descriptor.ResponseType);
            return $"{GetFriendlyName(descriptor.RequestType)} -> {responseName}";
        }

        private static string CreateDescription(AutomationDescriptor descriptor)
            => $"Automated {ToOperationName(descriptor.OperationKind)} operation for {GetFriendlyName(descriptor.EntityType)}.";

        private static string CreateTags(AutomationDescriptor descriptor)
            => $"turtlepath,automation,{ToOperationName(descriptor.OperationKind)},{(IsQuery(descriptor.OperationKind) ? "query" : "command")}";

        private static string ResolveBaseFlowId(AutomationOperationKind operationKind)
            => operationKind switch
            {
                AutomationOperationKind.Create => "turtlepath.command.create",
                AutomationOperationKind.Update => "turtlepath.command.update",
                AutomationOperationKind.Patch => "turtlepath.command.patch",
                AutomationOperationKind.Delete => "turtlepath.command.delete",
                _ => null
            };

        private static bool IsQuery(AutomationOperationKind operationKind)
            => operationKind is AutomationOperationKind.GetById or AutomationOperationKind.GetOne or AutomationOperationKind.GetMany or AutomationOperationKind.GetPaged;

        private static string ToOperationName(AutomationOperationKind operationKind)
            => operationKind.ToString().ToLowerInvariant();

        private static string GetFriendlyName(Type type)
        {
            if (!type.IsGenericType)
                return type.Name;

            var name = type.Name;
            var tickIndex = name.IndexOf('`');
            if (tickIndex >= 0)
                name = name[..tickIndex];

            return $"{name}<{string.Join(", ", type.GetGenericArguments().Select(GetFriendlyName))}>";
        }

        private static string Sanitize(string value)
        {
            var builder = new StringBuilder(value.Length);
            foreach (var character in value)
                builder.Append(char.IsLetterOrDigit(character) ? char.ToLowerInvariant(character) : '.');

            return builder.ToString().Trim('.');
        }

        private static SpiderComponentDescriptor Component(
            string id,
            string kind,
            string displayName,
            IReadOnlyDictionary<string, string> metadata)
            => new(id, kind, displayName, new Dictionary<string, string>(metadata));

        private static SpiderRelationDescriptor Relation(string id, string sourceId, string targetId, string kind)
            => new(id, sourceId, targetId, kind, new Dictionary<string, string>());

        private sealed record AutomationStep(string Id, string DisplayName, string Description, string Tags);
    }
}
