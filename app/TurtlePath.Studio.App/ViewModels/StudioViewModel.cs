using Microsoft.Maui.ApplicationModel;
using TurtlePath.Studio.Abstractions.Commands;
using TurtlePath.Studio.Abstractions.Projects;
using TurtlePath.Studio.Abstractions.Validation;
using TurtlePath.Studio.Abstractions.Workspace;
using TurtlePath.Studio.App.Settings;
using TurtlePath.Studio.Application.Defaults;
using TurtlePath.Studio.Application.Environment;
using TurtlePath.Studio.Application.UseCases;

namespace TurtlePath.Studio.App.ViewModels;

public sealed class StudioViewModel
{
    private readonly InspectStudioEnvironmentUseCase inspectEnvironment;
    private readonly InstallTemplateUseCase installTemplate;
    private readonly CreateTurtlePathProjectUseCase createProject;
    private readonly IStudioWorkspaceService workspace;
    private readonly IStudioSettingsStore settingsStore;

    public StudioSection Section { get; private set; } = StudioSection.Home;
    public bool SidebarCollapsed { get; private set; }
    public StudioEnvironmentReport? Environment { get; private set; }
    public IReadOnlyList<StudioEnvironmentReport> TemplateEnvironments { get; private set; } = [];
    public ProjectHostMode SelectedHost { get; private set; } = ProjectHostMode.ApiConsumer;
    public string SelectedTemplatePackageId { get; private set; } = TurtlePathStudioDefaults.TemplatePackageId;
    public string SelectedTemplateShortName { get; private set; } = TurtlePathStudioDefaults.TemplateShortName;
    public bool SelectedTemplateIncludesHostOption { get; private set; } = true;
    public WizardStep WizardStep { get; private set; } = WizardStep.Basics;
    public string ProjectName { get; set; }
    public string OutputRoot { get; set; }
    public bool RestoreAfterCreation { get; set; }
    public bool BuildAfterCreation { get; set; }
    public bool TestAfterCreation { get; set; }
    public bool HideGuideAfterCreation { get; set; }
    public string DefaultOutputRoot { get; set; } = string.Empty;
    public string ProjectNamePlaceholder { get; set; } = string.Empty;
    public bool DefaultRestoreAfterCreation { get; set; }
    public bool DefaultBuildAfterCreation { get; set; }
    public bool DefaultTestAfterCreation { get; set; }
    public bool DefaultHideGuideAfterCreation { get; set; }
    public bool IsBusy { get; private set; }
    public string BusyTitle { get; private set; } = "Working";
    public string BusyMessage { get; private set; } = "Studio is running a command. This can take a moment.";
    public bool IsWizardOpen { get; private set; }
    public bool IsCommandOutputOpen { get; private set; }
    public bool IsCreated { get; private set; }
    public string? CreatedDirectory { get; private set; }
    public string Message { get; private set; } = "Ready.";
    public bool MessageIsError { get; private set; }
    public IReadOnlyList<CommandExecutionResult> Commands { get; private set; } = [];
    public string PageTitle => Section switch
    {
        StudioSection.Home => "Build TurtlePath projects faster",
        StudioSection.Templates => "Templates",
        StudioSection.Guides => "Usage guides",
        StudioSection.Demos => "Demos",
        StudioSection.Environment => "Environment",
        _ => "TurtlePath Studio"
    };

    public string PageSubtitle => Section switch
    {
        StudioSection.Home => "Start from the template, read the guide, or check your local environment.",
        StudioSection.Templates => "Pick the host type and let the wizard create the project.",
        StudioSection.Guides => "Step-by-step notes for generated projects and TurtlePath conventions.",
        StudioSection.Demos => "Generate complete reference projects that show TurtlePath features working together.",
        StudioSection.Environment => "Validate and repair the local .NET template setup.",
        _ => "Project launcher"
    };

    public string EnvironmentText => TemplateEnvironments.Count == 0
        ? SidebarCollapsed ? "?" : "Environment: not checked"
        : TemplatesCanCreateProjects
            ? SidebarCollapsed ? "OK" : "Environment: ready"
            : TemplateEnvironments.Any(environment => environment.TemplateRequiresUpdate)
                ? SidebarCollapsed ? "!" : "Environment: update required"
                : SidebarCollapsed ? "!" : "Environment: needs attention";

    public string SelectedTemplateName { get; private set; } = "API / Consumer";

    public string ProjectDirectoryPreview => string.IsNullOrWhiteSpace(ProjectName) || string.IsNullOrWhiteSpace(OutputRoot)
        ? "Complete the project name and destination folder"
        : Path.Combine(OutputRoot, ProjectName);

    public string StudioVersion => $"v{AppInfo.Current.VersionString}";

    public string TemplateActionText => TemplateEnvironments.Any(environment => environment.TemplateRequiresUpdate)
        ? "Update templates"
        : "Install templates";

    public bool TemplateIsCurrent => TemplatesCanCreateProjects && !TemplateEnvironments.Any(environment => environment.TemplateRequiresUpdate);

    public bool TemplatesCanCreateProjects => TemplateEnvironments.Count > 0
        && TemplateEnvironments.All(environment => environment.CanCreateProjects);

    public StudioViewModel(
        InspectStudioEnvironmentUseCase inspectEnvironment,
        InstallTemplateUseCase installTemplate,
        CreateTurtlePathProjectUseCase createProject,
        IStudioWorkspaceService workspace,
        IStudioSettingsStore settingsStore)
    {
        this.inspectEnvironment = inspectEnvironment;
        this.installTemplate = installTemplate;
        this.createProject = createProject;
        this.workspace = workspace;
        this.settingsStore = settingsStore;

        var settings = settingsStore.Load();
        ApplySettings(settings);
        ProjectName = settings.ProjectNamePlaceholder;
        OutputRoot = settings.DefaultOutputRoot;
        RestoreAfterCreation = settings.RestoreAfterCreation;
        BuildAfterCreation = settings.BuildAfterCreation;
        TestAfterCreation = settings.TestAfterCreation;
        HideGuideAfterCreation = settings.HideGuideAfterCreation;
    }

    public void ToggleSidebar() => SidebarCollapsed = !SidebarCollapsed;

    public void Navigate(StudioSection section)
    {
        Section = section;
        ClearMessage();
    }

    public void OpenWizard(ProjectHostMode hostMode)
    {
        SelectedHost = hostMode;
        SelectedTemplateName = hostMode == ProjectHostMode.Job ? "One-shot Job" : "API / Consumer";
        SelectedTemplatePackageId = TurtlePathStudioDefaults.TemplatePackageId;
        SelectedTemplateShortName = TurtlePathStudioDefaults.TemplateShortName;
        SelectedTemplateIncludesHostOption = true;
        ProjectName = string.IsNullOrWhiteSpace(ProjectNamePlaceholder)
            ? "TurtlePath.Service"
            : ProjectNamePlaceholder;
        OutputRoot = DefaultOutputRoot;
        RestoreAfterCreation = DefaultRestoreAfterCreation;
        BuildAfterCreation = DefaultBuildAfterCreation;
        TestAfterCreation = DefaultTestAfterCreation;
        HideGuideAfterCreation = DefaultHideGuideAfterCreation;
        WizardStep = WizardStep.Basics;
        IsCreated = false;
        CreatedDirectory = null;
        Commands = [];
        IsCommandOutputOpen = false;
        IsWizardOpen = true;
        ClearMessage();
    }

    public void OpenHeroesShowcaseWizard()
    {
        SelectedHost = ProjectHostMode.ApiConsumer;
        SelectedTemplateName = "Heroes Showcase";
        SelectedTemplatePackageId = TurtlePathStudioDefaults.HeroesShowcaseTemplatePackageId;
        SelectedTemplateShortName = TurtlePathStudioDefaults.HeroesShowcaseTemplateShortName;
        SelectedTemplateIncludesHostOption = false;
        ProjectName = "Heroes.Service";
        OutputRoot = DefaultOutputRoot;
        RestoreAfterCreation = DefaultRestoreAfterCreation;
        BuildAfterCreation = DefaultBuildAfterCreation;
        TestAfterCreation = DefaultTestAfterCreation;
        HideGuideAfterCreation = DefaultHideGuideAfterCreation;
        WizardStep = WizardStep.Basics;
        IsCreated = false;
        CreatedDirectory = null;
        Commands = [];
        IsCommandOutputOpen = false;
        IsWizardOpen = true;
        ClearMessage();
    }

    public void CloseWizard() => IsWizardOpen = false;

    public void OpenCommandOutput()
    {
        if (Commands.Count > 0)
            IsCommandOutputOpen = true;
    }

    public void CloseCommandOutput() => IsCommandOutputOpen = false;

    public void FinishWizard()
    {
        IsWizardOpen = false;
        if (IsCreated && !HideGuideAfterCreation)
            Section = StudioSection.Guides;
    }

    public bool NextWizardStep()
    {
        if (WizardStep == WizardStep.Basics && !ValidateInput())
            return false;

        WizardStep = WizardStep switch
        {
            WizardStep.Basics => WizardStep.Options,
            WizardStep.Options => WizardStep.Review,
            WizardStep.Review => WizardStep.Result,
            _ => WizardStep
        };

        return true;
    }

    public void PreviousWizardStep()
    {
        WizardStep = WizardStep switch
        {
            WizardStep.Options => WizardStep.Basics,
            WizardStep.Review => WizardStep.Options,
            WizardStep.Result => WizardStep.Review,
            _ => WizardStep
        };
    }

    public async Task PickOutputDirectoryAsync()
    {
        OutputRoot = await workspace.PickOutputDirectoryAsync(OutputRoot);
    }

    public async Task PickDefaultOutputDirectoryAsync()
    {
        DefaultOutputRoot = await workspace.PickOutputDirectoryAsync(DefaultOutputRoot);
    }

    public Task RefreshEnvironmentAsync()
    {
        return RunAsync("Checking environment", "Studio is checking .NET and installed TurtlePath templates.", async () =>
        {
            TemplateEnvironments = await InspectTemplateEnvironmentsAsync();
            Environment = TemplateEnvironments.FirstOrDefault(environment =>
                environment.Template.PackageId == TurtlePathStudioDefaults.TemplatePackageId);

            Message = TemplatesCanCreateProjects
                ? "Environment ready."
                : "One or more templates need attention. Use the actions below to repair them.";
            MessageIsError = !TemplatesCanCreateProjects;
        });
    }

    public Task InstallTemplateAsync()
    {
        return RunAsync("Installing templates", "Studio is installing or updating the TurtlePath template packages.", async () =>
        {
            var commands = new List<CommandExecutionResult>();
            foreach (var packageId in TurtlePathStudioDefaults.TemplatePackageIds)
                commands.Add(await installTemplate.ExecuteAsync(packageId, forceUpdate: true));

            Commands = commands;
            TemplateEnvironments = await InspectTemplateEnvironmentsAsync();
            Environment = TemplateEnvironments.FirstOrDefault(environment =>
                environment.Template.PackageId == TurtlePathStudioDefaults.TemplatePackageId);

            Message = commands.All(command => command.Succeeded)
                ? "Templates installed. Environment was checked again."
                : "Template installation failed. Check the command output.";
            MessageIsError = !commands.All(command => command.Succeeded);
            IsCommandOutputOpen = true;

            if (TemplatesCanCreateProjects)
                MessageIsError = false;
        });
    }

    public void SaveDefaults()
    {
        if (string.IsNullOrWhiteSpace(DefaultOutputRoot))
        {
            SetError("Default output path is required.");
            return;
        }

        if (string.IsNullOrWhiteSpace(ProjectNamePlaceholder))
        {
            SetError("Project name placeholder is required.");
            return;
        }

        var settings = new StudioSettings(
            DefaultOutputRoot.Trim(),
            ProjectNamePlaceholder.Trim(),
            DefaultRestoreAfterCreation,
            DefaultBuildAfterCreation,
            DefaultTestAfterCreation,
            DefaultHideGuideAfterCreation);

        settingsStore.Save(settings);
        ApplySettings(settings);
        Message = "Default values saved.";
        MessageIsError = false;
    }

    public void ResetDefaults()
    {
        settingsStore.Reset();
        ApplySettings(settingsStore.Load());
        Message = "Default values restored.";
        MessageIsError = false;
    }

    public async Task CreateProjectAsync()
    {
        if (!ValidateInput())
        {
            WizardStep = WizardStep.Basics;
            return;
        }

        WizardStep = WizardStep.Result;
        IsCreated = false;
        Commands = [];

        await RunAsync("Creating project", "Studio is running the template and optional validation commands.", async () =>
        {
            var selectedTemplate = await inspectEnvironment.ExecuteAsync(SelectedTemplatePackageId);
            if (!selectedTemplate.CanCreateProjects)
            {
                var install = await installTemplate.ExecuteAsync(SelectedTemplatePackageId, forceUpdate: true);
                Commands = [install];

                selectedTemplate = await inspectEnvironment.ExecuteAsync(SelectedTemplatePackageId);
                if (!install.Succeeded || !selectedTemplate.CanCreateProjects)
                {
                    Message = $"{SelectedTemplatePackageId} must be installed before creating projects.";
                    MessageIsError = true;
                    IsCommandOutputOpen = true;
                    return;
                }
            }

            if (SelectedTemplatePackageId == TurtlePathStudioDefaults.TemplatePackageId)
                Environment = selectedTemplate;

            if (selectedTemplate.TemplateRequiresUpdate)
            {
                Message = BuildTemplateUpdateSuggestionMessage(selectedTemplate);
                MessageIsError = false;
            }

            var result = await createProject.ExecuteAsync(new CreateProjectRequest(
                ProjectName.Trim(),
                ProjectDirectoryPreview,
                SelectedHost,
                SelectedTemplateShortName,
                SelectedTemplateIncludesHostOption,
                RestoreAfterCreation,
                BuildAfterCreation,
                TestAfterCreation));

            Commands = [.. Commands, .. CollectCommands(result)];
            IsCreated = result.Succeeded;
            CreatedDirectory = result.Creation.ProjectDirectory;
            Message = result.Succeeded
                ? selectedTemplate.TemplateRequiresUpdate
                    ? $"{BuildTemplateUpdateSuggestionMessage(selectedTemplate)} Project created successfully."
                    : "Project created successfully."
                : "Project creation finished with errors. Check the execution log.";
            MessageIsError = !result.Succeeded;
            IsCommandOutputOpen = true;
        });
    }

    public async Task OpenCreatedFolderAsync()
    {
        if (!string.IsNullOrWhiteSpace(CreatedDirectory))
            await workspace.OpenDirectoryAsync(CreatedDirectory);
    }

    private async Task RunAsync(string title, string message, Func<Task> action)
    {
        BusyTitle = title;
        BusyMessage = message;
        IsBusy = true;
        try
        {
            await action();
        }
        catch (Exception exception)
        {
            SetError(exception.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<IReadOnlyList<StudioEnvironmentReport>> InspectTemplateEnvironmentsAsync()
    {
        var environments = new List<StudioEnvironmentReport>();
        foreach (var packageId in TurtlePathStudioDefaults.TemplatePackageIds)
            environments.Add(await inspectEnvironment.ExecuteAsync(packageId));

        return environments;
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(ProjectName))
        {
            SetError("Project name is required.");
            return false;
        }

        if (ProjectName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            SetError("Project name contains invalid characters.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(OutputRoot))
        {
            SetError("Destination folder is required.");
            return false;
        }

        ClearMessage();
        return true;
    }

    private static IEnumerable<CommandExecutionResult> CollectCommands(CreateTurtlePathProjectUseCaseResult result)
    {
        yield return result.Creation.Generation;

        if (result.Validation is null)
            yield break;

        foreach (ProjectValidationStepResult step in result.Validation.Steps)
            yield return step.Execution;
    }

    private void SetError(string text)
    {
        Message = text;
        MessageIsError = true;
    }

    private void ClearMessage()
    {
        Message = "Ready.";
        MessageIsError = false;
    }

    private void ApplySettings(StudioSettings settings)
    {
        DefaultOutputRoot = settings.DefaultOutputRoot;
        ProjectNamePlaceholder = settings.ProjectNamePlaceholder;
        DefaultRestoreAfterCreation = settings.RestoreAfterCreation;
        DefaultBuildAfterCreation = settings.BuildAfterCreation;
        DefaultTestAfterCreation = settings.TestAfterCreation;
        DefaultHideGuideAfterCreation = settings.HideGuideAfterCreation;
    }

    private static string BuildTemplateUpdateSuggestionMessage(StudioEnvironmentReport environment)
    {
        return environment.Template.HasLatestVersion
            ? $"{environment.Template.PackageId} has a newer version available. Installed: {environment.Template.Version}; latest: {environment.Template.LatestVersion}."
            : $"{environment.Template.PackageId} is installed, but Studio could not verify the latest NuGet version.";
    }
}
