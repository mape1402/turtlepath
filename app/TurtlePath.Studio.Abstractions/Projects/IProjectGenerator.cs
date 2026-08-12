namespace TurtlePath.Studio.Abstractions.Projects;

public interface IProjectGenerator
{
    Task<CreateProjectResult> CreateAsync(
        CreateProjectRequest request,
        CancellationToken cancellationToken = default);
}
