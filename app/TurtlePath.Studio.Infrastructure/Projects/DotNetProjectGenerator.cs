using TurtlePath.Studio.Abstractions.Commands;
using TurtlePath.Studio.Abstractions.Projects;

namespace TurtlePath.Studio.Infrastructure.Projects;

public sealed class DotNetProjectGenerator(ICommandExecutor commandExecutor) : IProjectGenerator
{
    public async Task<CreateProjectResult> CreateAsync(
        CreateProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ProjectName);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.OutputDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.TemplateShortName);

        var outputDirectory = Path.GetFullPath(request.OutputDirectory);
        Directory.CreateDirectory(outputDirectory);
        var arguments = new List<string>
        {
            "new",
            request.TemplateShortName,
            "-n",
            request.ProjectName,
            "-o",
            outputDirectory
        };

        if (request.IncludeHostOption)
        {
            arguments.Add("--host");
            arguments.Add(ToTemplateHost(request.HostMode));
        }

        var result = await commandExecutor.ExecuteAsync(
            new CommandSpec(
                "dotnet",
                arguments,
                outputDirectory),
            cancellationToken);

        return new CreateProjectResult(
            request.ProjectName,
            outputDirectory,
            request.HostMode,
            result);
    }

    private static string ToTemplateHost(ProjectHostMode hostMode)
    {
        return hostMode switch
        {
            ProjectHostMode.ApiConsumer => "api-consumer",
            ProjectHostMode.Job => "job",
            _ => throw new ArgumentOutOfRangeException(nameof(hostMode), hostMode, null)
        };
    }
}
