using TurtlePath.Studio.Abstractions.Commands;
using TurtlePath.Studio.Abstractions.Environment;
using TurtlePath.Studio.Abstractions.Templates;
using TurtlePath.Studio.Application.Defaults;
using TurtlePath.Studio.Application.UseCases;

namespace TurtlePath.Studio.Tests.UseCases;

public sealed class StudioEnvironmentUseCaseTests
{
    [Fact]
    public async Task InspectStudioEnvironment_uses_default_template_when_package_id_is_empty()
    {
        var dotNet = new StubDotNetEnvironmentReader();
        var templates = new StubTemplatePackageManager();
        var useCase = new InspectStudioEnvironmentUseCase(dotNet, templates);

        var report = await useCase.ExecuteAsync(" ");

        Assert.Same(dotNet.Info, report.DotNet);
        Assert.Same(templates.Installed, report.Template);
        Assert.Equal(TurtlePathStudioDefaults.TemplatePackageId, templates.PackageId);
    }

    [Fact]
    public async Task InspectStudioEnvironment_uses_requested_template_package()
    {
        var templates = new StubTemplatePackageManager();
        var useCase = new InspectStudioEnvironmentUseCase(new StubDotNetEnvironmentReader(), templates);

        await useCase.ExecuteAsync("Custom.Template");

        Assert.Equal("Custom.Template", templates.PackageId);
    }

    [Fact]
    public async Task InspectStudioEnvironment_reports_template_install_and_update_state()
    {
        var templates = new StubTemplatePackageManager
        {
            Installed = new TemplatePackageInfo("Template", "1.0.0", false, "1.1.0")
        };
        var useCase = new InspectStudioEnvironmentUseCase(new StubDotNetEnvironmentReader(), templates);

        var report = await useCase.ExecuteAsync("Template");

        Assert.False(report.CanCreateProjects);
        Assert.True(report.TemplateRequiresInstall);
        Assert.False(report.TemplateRequiresUpdate);
    }

    [Fact]
    public async Task InstallTemplate_uses_default_and_custom_values()
    {
        var templates = new StubTemplatePackageManager();
        var useCase = new InstallTemplateUseCase(templates);

        await useCase.ExecuteAsync(" ", forceUpdate: true);
        var defaultRequest = templates.InstallRequest;
        await useCase.ExecuteAsync("Custom.Template", "1.2.3");

        Assert.Equal(TurtlePathStudioDefaults.TemplatePackageId, defaultRequest.PackageId);
        Assert.True(defaultRequest.ForceUpdate);
        Assert.Equal("Custom.Template", templates.InstallRequest.PackageId);
        Assert.Equal("1.2.3", templates.InstallRequest.Version);
    }

    private sealed class StubDotNetEnvironmentReader : IDotNetEnvironmentReader
    {
        public DotNetEnvironmentInfo Info { get; } = new(true, "10.0.100", [], "dotnet", string.Empty);

        public Task<DotNetEnvironmentInfo> ReadAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Info);
    }

    private sealed class StubTemplatePackageManager : ITemplatePackageManager
    {
        public TemplatePackageInfo Installed { get; init; } = new("Template", "1.0.0", true);

        public string PackageId { get; private set; }

        public TemplateInstallRequest InstallRequest { get; private set; }

        public Task<TemplatePackageInfo> GetInstalledAsync(
            string packageId,
            CancellationToken cancellationToken = default)
        {
            PackageId = packageId;
            return Task.FromResult(Installed);
        }

        public Task<string> GetLatestVersionAsync(
            string packageId,
            CancellationToken cancellationToken = default)
            => Task.FromResult("1.0.0");

        public Task<CommandExecutionResult> InstallAsync(
            TemplateInstallRequest request,
            CancellationToken cancellationToken = default)
        {
            InstallRequest = request;
            return Task.FromResult(new CommandExecutionResult(
                new CommandSpec("dotnet", [], System.Environment.CurrentDirectory),
                0,
                TimeSpan.Zero,
                []));
        }
    }
}
