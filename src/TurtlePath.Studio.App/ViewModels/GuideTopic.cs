namespace TurtlePath.Studio.App.ViewModels;

public sealed record GuideTopic(
    string Key,
    string Title,
    string Summary,
    IReadOnlyList<GuideStep> Steps);

public sealed record GuideStep(
    string Title,
    string Body,
    IReadOnlyList<string> Details);
