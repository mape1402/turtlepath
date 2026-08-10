namespace TurtlePath.Studio.Abstractions.Projects;

public sealed record CreateProjectRequest(
    string ProjectName,
    string OutputDirectory,
    ProjectHostMode HostMode,
    bool RestoreAfterCreation = true,
    bool BuildAfterCreation = true,
    bool TestAfterCreation = true);
