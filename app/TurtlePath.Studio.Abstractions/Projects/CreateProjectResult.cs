using TurtlePath.Studio.Abstractions.Commands;

namespace TurtlePath.Studio.Abstractions.Projects;

public sealed record CreateProjectResult(
    string ProjectName,
    string ProjectDirectory,
    ProjectHostMode HostMode,
    CommandExecutionResult Generation);
