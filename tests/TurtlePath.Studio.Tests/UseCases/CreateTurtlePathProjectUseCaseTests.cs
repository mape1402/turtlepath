using TurtlePath.Studio.Abstractions.Commands;
using TurtlePath.Studio.Abstractions.Projects;
using TurtlePath.Studio.Abstractions.Validation;
using TurtlePath.Studio.Application.UseCases;

namespace TurtlePath.Studio.Tests.UseCases;

public class CreateTurtlePathProjectUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_creates_project_and_runs_requested_validation()
    {
        var generator = new StubProjectGenerator();
        var validator = new StubProjectValidator();
        var useCase = new CreateTurtlePathProjectUseCase(generator, validator);

        var result = await useCase.ExecuteAsync(new CreateProjectRequest(
            "Billing",
            "C:\\work\\Billing",
            ProjectHostMode.ApiConsumer,
            RestoreAfterCreation: true,
            BuildAfterCreation: false,
            TestAfterCreation: true));

        Assert.True(result.Succeeded);
        Assert.NotNull(validator.Request);
        Assert.True(validator.Request.Restore);
        Assert.False(validator.Request.Build);
        Assert.True(validator.Request.Test);
    }

    private sealed class StubProjectGenerator : IProjectGenerator
    {
        public Task<CreateProjectResult> CreateAsync(
            CreateProjectRequest request,
            CancellationToken cancellationToken = default)
        {
            var command = new CommandSpec("dotnet", [], request.OutputDirectory);
            var execution = new CommandExecutionResult(command, 0, TimeSpan.Zero, []);

            return Task.FromResult(new CreateProjectResult(
                request.ProjectName,
                request.OutputDirectory,
                request.HostMode,
                execution));
        }
    }

    private sealed class StubProjectValidator : IProjectValidator
    {
        public ProjectValidationRequest Request { get; private set; }

        public Task<ProjectValidationResult> ValidateAsync(
            ProjectValidationRequest request,
            CancellationToken cancellationToken = default)
        {
            Request = request;

            return Task.FromResult(new ProjectValidationResult(
                request.ProjectDirectory,
                []));
        }
    }
}
