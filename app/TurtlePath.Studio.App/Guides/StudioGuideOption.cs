namespace TurtlePath.Studio.App.Guides;

public sealed record StudioGuideOption(
    string Id,
    string Title,
    string DocumentationVersion,
    string PackageId,
    string SupportedTemplateVersionRange,
    IReadOnlyList<string> SupportedTemplateVersions,
    IReadOnlyList<StudioGuideCulture> Cultures,
    string Source);
