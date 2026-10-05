using Spider.Pipelines.Architecture;

namespace TurtlePath.Commands;

/// <summary>
/// Provides Spider architecture metadata for TurtlePath command handler flows.
/// </summary>
public static class TurtlePathCommandArchitecture
{
    /// <summary>
    /// Builds the default TurtlePath command flow architecture manifest.
    /// </summary>
    /// <returns>A Spider architecture manifest that describes the abstract command handler flows.</returns>
    public static SpiderArchitectureManifest BuildManifest()
    {
        var components = new List<SpiderComponentDescriptor>
        {
            Component("turtlepath.profile.command", "spider.flow-profile", TurtlePathCommandFlowProfiles.Command, "Default profile used by TurtlePath command handler flows.", "turtlepath,command,profile"),
        };
        var relations = new List<SpiderRelationDescriptor>();

        AddFlow(components, relations, "create", "Create command", "Validates a create request, maps it to a new entity, saves it, and optionally maps a response.", [
            ("validate", "Validate request", "Runs validation hooks and validates the incoming create request.", "validation,hooks"),
            ("map", "Map entity", "Runs map hooks and creates the entity from the request.", "mapping,hooks"),
            ("save", "Save entity", "Runs save hooks and persists the created entity.", "persistence,hooks"),
            ("response", "Map response", "Runs response hooks and maps the saved entity to the command response when the handler returns one.", "response,projection,hooks"),
        ]);

        AddFlow(components, relations, "update", "Update command", "Loads an entity, validates the request, maps changes, saves the entity, and optionally maps a response.", [
            ("load", "Load entity", "Runs get-entity hooks and loads the entity targeted by the request.", "lookup,hooks"),
            ("validate", "Validate request", "Runs validation hooks and validates the request against the loaded entity.", "validation,hooks"),
            ("map", "Map entity", "Runs map hooks and applies request values to the loaded entity.", "mapping,hooks"),
            ("save", "Save entity", "Runs save hooks and persists the updated entity.", "persistence,hooks"),
            ("response", "Map response", "Runs response hooks and maps the updated entity to the command response when the handler returns one.", "response,projection,hooks"),
        ]);

        AddFlow(components, relations, "patch", "Patch command", "Loads an entity, optionally validates the request, applies partial changes, saves the entity, and optionally maps a response.", [
            ("load", "Load entity", "Runs get-entity hooks and loads the entity targeted by the patch request.", "lookup,hooks"),
            ("validate", "Validate request", "Runs validation hooks and validates the patch request when validation is enabled.", "validation,hooks"),
            ("patch", "Patch entity", "Runs patch hooks and applies partial request changes to the loaded entity.", "patch,hooks"),
            ("save", "Save entity", "Runs save hooks and persists the patched entity.", "persistence,hooks"),
            ("response", "Map response", "Runs response hooks and maps the patched entity to the command response when the handler returns one.", "response,projection,hooks"),
        ]);

        AddFlow(components, relations, "delete", "Delete command", "Loads an entity, optionally validates the request, deletes the entity, and optionally maps a response.", [
            ("load", "Load entity", "Runs get-entity hooks and loads the entity targeted by the delete request.", "lookup,hooks"),
            ("validate", "Validate request", "Runs validation hooks and validates the delete request when validation is enabled.", "validation,hooks"),
            ("delete", "Delete entity", "Runs delete hooks and removes the loaded entity from storage.", "delete,persistence,hooks"),
            ("response", "Map response", "Runs response hooks and maps the deleted entity to the command response when the handler returns one.", "response,hooks"),
        ]);

        return new SpiderArchitectureManifest(components, relations);
    }

    private static void AddFlow(
        ICollection<SpiderComponentDescriptor> components,
        ICollection<SpiderRelationDescriptor> relations,
        string operation,
        string displayName,
        string description,
        IReadOnlyList<(string Id, string DisplayName, string Description, string Tags)> steps)
    {
        var flowId = $"turtlepath.command.{operation}";
        components.Add(Component(flowId, "spider.flow", displayName, description, $"turtlepath,command,{operation}"));
        relations.Add(Relation($"{flowId}.uses-profile", flowId, "turtlepath.profile.command", "uses-profile"));

        string previousStepId = null;
        foreach (var step in steps)
        {
            var stepId = $"{flowId}.{step.Id}";
            components.Add(Component(stepId, "spider.flow-step", step.DisplayName, step.Description, step.Tags));
            relations.Add(Relation($"{flowId}.contains.{step.Id}", flowId, stepId, "contains"));

            if (previousStepId != null)
                relations.Add(Relation($"{previousStepId}.next.{step.Id}", previousStepId, stepId, "next"));

            previousStepId = stepId;
        }
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
}
