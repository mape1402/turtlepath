namespace TurtlePath.Studio.App.Guides;

public sealed record StudioGuideDocument(
    StudioGuideOption Guide,
    StudioGuideCulture Culture,
    string Html,
    string Status,
    bool LoadedFromCache,
    bool IsEmbeddedFallback,
    bool IsTemplatePackage = false);
