using TurtlePath.Studio.Abstractions.Commands;
using TurtlePath.Studio.Abstractions.Projects;
using TurtlePath.Studio.Abstractions.Validation;
using TurtlePath.Studio.Abstractions.Workspace;
using TurtlePath.Studio.Application.Environment;
using TurtlePath.Studio.Application.UseCases;

namespace TurtlePath.Studio.App.ViewModels;

public sealed class StudioViewModel(
    InspectStudioEnvironmentUseCase inspectEnvironment,
    InstallTemplateUseCase installTemplate,
    CreateTurtlePathProjectUseCase createProject,
    IStudioWorkspaceService workspace)
{
    public StudioSection Section { get; private set; } = StudioSection.Home;
    public bool SidebarCollapsed { get; private set; }
    public StudioEnvironmentReport? Environment { get; private set; }
    public ProjectHostMode SelectedHost { get; private set; } = ProjectHostMode.ApiConsumer;
    public WizardStep WizardStep { get; private set; } = WizardStep.Basics;
    public string ProjectName { get; set; } = "BillingService";
    public string OutputRoot { get; set; } = global::System.Environment.GetFolderPath(global::System.Environment.SpecialFolder.MyDocuments);
    public bool RestoreAfterCreation { get; set; } = true;
    public bool BuildAfterCreation { get; set; } = true;
    public bool TestAfterCreation { get; set; } = true;
    public bool HideGuideAfterCreation { get; set; }
    public bool IsBusy { get; private set; }
    public bool IsWizardOpen { get; private set; }
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
        StudioSection.Environment => "Environment",
        _ => "TurtlePath Studio"
    };

    public string PageSubtitle => Section switch
    {
        StudioSection.Home => "Start from the template, read the guide, or check your local environment.",
        StudioSection.Templates => "Pick the host type and let the wizard create the project.",
        StudioSection.Guides => "Step-by-step notes for generated projects and TurtlePath conventions.",
        StudioSection.Environment => "Validate and repair the local .NET template setup.",
        _ => "Project launcher"
    };

    public string EnvironmentText => Environment is null
        ? SidebarCollapsed ? "?" : "Environment: not checked"
        : Environment.CanCreateProjects
            ? SidebarCollapsed ? "OK" : "Environment: ready"
            : SidebarCollapsed ? "!" : "Environment: needs attention";

    public string SelectedTemplateName => SelectedHost == ProjectHostMode.Job
        ? "One-shot Job"
        : "API / Consumer";

    public string ProjectDirectoryPreview => string.IsNullOrWhiteSpace(ProjectName) || string.IsNullOrWhiteSpace(OutputRoot)
        ? "Complete the project name and destination folder"
        : Path.Combine(OutputRoot, ProjectName);

    public void ToggleSidebar() => SidebarCollapsed = !SidebarCollapsed;

    public void Navigate(StudioSection section)
    {
        Section = section;
        ClearMessage();
    }

    public void OpenWizard(ProjectHostMode hostMode)
    {
        SelectedHost = hostMode;
        ProjectName = hostMode == ProjectHostMode.Job ? "BillingJob" : "BillingService";
        WizardStep = WizardStep.Basics;
        IsCreated = false;
        CreatedDirectory = null;
        Commands = [];
        IsWizardOpen = true;
        ClearMessage();
    }

    public void CloseWizard() => IsWizardOpen = false;

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

    public Task RefreshEnvironmentAsync()
    {
        return RunAsync(async () =>
        {
            Environment = await inspectEnvironment.ExecuteAsync();
            Message = Environment.CanCreateProjects
                ? "Environment ready."
                : "Template or .NET environment needs attention. Use the actions below to repair it.";
            MessageIsError = !Environment.CanCreateProjects;
        });
    }

    public Task InstallTemplateAsync()
    {
        return RunAsync(async () =>
        {
            var result = await installTemplate.ExecuteAsync(forceUpdate: true);
            Commands = [result];
            Message = result.Succeeded
                ? "Template installed. Environment was checked again."
                : "Template installation failed. Check the command output.";
            MessageIsError = !result.Succeeded;

            Environment = await inspectEnvironment.ExecuteAsync();
            if (Environment.CanCreateProjects)
                MessageIsError = false;
        });
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

        await RunAsync(async () =>
        {
            var result = await createProject.ExecuteAsync(new CreateProjectRequest(
                ProjectName.Trim(),
                ProjectDirectoryPreview,
                SelectedHost,
                RestoreAfterCreation,
                BuildAfterCreation,
                TestAfterCreation));

            Commands = CollectCommands(result).ToArray();
            IsCreated = result.Succeeded;
            CreatedDirectory = result.Creation.ProjectDirectory;
            Message = result.Succeeded
                ? "Project created successfully."
                : "Project creation finished with errors. Check the execution log.";
            MessageIsError = !result.Succeeded;
        });
    }

    public async Task OpenCreatedFolderAsync()
    {
        if (!string.IsNullOrWhiteSpace(CreatedDirectory))
            await workspace.OpenDirectoryAsync(CreatedDirectory);
    }

    private async Task RunAsync(Func<Task> action)
    {
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
}
