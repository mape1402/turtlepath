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
    public string SelectedGuideTopicKey { get; private set; } = "project-flow";

    public IReadOnlyList<GuideTopic> GuideTopics { get; } =
    [
        new(
            "project-flow",
            "Project Flow",
            "Create a service or job from the template and understand what Studio generates.",
            [
                new("Pick a template", "Use API / Consumer for HTTP plus consumers. Use One-shot Job when Kubernetes owns the schedule.", ["Open Templates.", "Choose API / Consumer or One-shot Job.", "Click Create to open the project wizard."]),
                new("Configure basics", "Set the project name and destination folder. Studio previews the final output path before running anything.", ["Project name should be the service name, for example BillingService.", "Destination folder is the parent folder where the project directory will be created.", "Use Browse when you do not want to type the path manually."]),
                new("Choose validation", "Restore, build and test can run immediately after project generation.", ["Keep Restore enabled for normal work.", "Keep Build enabled to catch template or dependency issues immediately.", "Enable Test when you want confidence before opening the project."]),
                new("Review and create", "The wizard shows the template, project name, destination and validation choices before execution.", ["Review the path.", "Click Create project.", "Use Open folder after success."])
            ]),
        new(
            "dependencies",
            "Dependencies",
            "Where registrations live and how to customize TurtlePath defaults safely.",
            [
                new("API layer owns composition", "Dependency injection belongs to the host layer, not the Business layer.", ["Register TurtlePath defaults from the API or Job host.", "Keep custom registrations in CustomContainerExtensions.", "Avoid modifying defaults for service-specific dependencies."]),
                new("Adapters", "Use adapters for mapping, validation and filtering instead of coupling features to external libraries.", ["OctoMap goes through the TurtlePath OctoMap adapter.", "Crabalidator goes through the TurtlePath Crabalidator adapter.", "DataScorpio is the default filtering adapter."]),
                new("Custom services", "Feature-specific services live inside the feature. Shared services live in Business/Services.", ["Customers/Services/SAT can contain ISatService and its implementation.", "Business/Services/Audit can contain cross-feature auditing.", "Extract shared service folders later into packages when they become reusable."])
            ]),
        new(
            "domain",
            "Domain",
            "Where entities, identifiers and domain contracts belong.",
            [
                new("Entities", "Domain entities live in the domain layer and should stay persistence-ignorant.", ["Use BaseEntity when following the recommended TurtlePath path.", "Use IEntity<TKey> when custom identifiers or legacy models require flexibility.", "Keep business invariants inside the entity when they are truly domain rules."]),
                new("Identifiers", "CId hides the underlying key type while keeping code structurally consistent.", ["New projects can use the same CId type across entities.", "Legacy projects can configure different CId backing types per entity.", "Do not compare unrelated CId values just because the wrapper type looks similar."]),
                new("Contracts", "Contracts such as IEntity<TKey> and BaseEntity belong under Domain/Contracts.", ["Do not leak EF concerns into Domain.", "Do not put application handlers in Domain.", "Keep domain classes focused on model and rules."])
            ]),
        new(
            "entity-framework",
            "Entity Framework",
            "How persistence, DbContext and entity configurations are organized.",
            [
                new("DbContext", "Use the TurtlePath DbContext base when the service follows the default persistence path.", ["Register the application DbContext in the host.", "Configure CId through profiles, not static metadata.", "Keep transaction boundaries configured in the API or Job host."]),
                new("EntityTypeConfiguration", "Entity configurations use the CustomerConfiguration naming style.", ["Put configuration classes in the persistence project.", "Use IEntityTypeConfiguration<TEntity> for table, keys, relationships and indexes.", "Keep EF mapping out of domain entities."]),
                new("Outbox and transactions", "The template defaults should support EF outbox and transaction boundaries.", ["Use Spider boundaries for transaction flow.", "Suppress Pigeon transaction scope when needed.", "Do not put transaction behavior inside feature handlers unless the use case is special."])
            ]),
        new(
            "handlers",
            "Handlers",
            "Where commands, queries, hooks and automations fit.",
            [
                new("Commands", "Commands are request classes with the Request suffix.", ["CreateCustomerRequest.", "UpdateInvoiceRequest.", "ChangeOrderStatusRequest.", "Handlers use CreateCustomerCommandHandler naming."]),
                new("Queries", "Queries use Query suffix and handlers use QueryHandler suffix.", ["GetCustomerByIdQuery.", "GetPagedInvoicesQuery.", "GetCustomerByIdQueryHandler can be nested when useful."]),
                new("Automations", "Use automations for standard CRUD and query happy paths.", ["CustomerAutomationProfile belongs in Customers/Automations.", "Use hooks for validation, enrichment, event sourcing and auditing.", "Create custom handlers only when automation cannot express the behavior."]),
                new("Hooks", "Hooks describe the action and where they run.", ["Use hooks for pre-create validation, post-save side effects or event sourcing.", "Keep hooks feature-scoped unless they are truly cross-cutting.", "Shared cross-cutting hooks can move into reusable packages later."])
            ]),
        new(
            "testing",
            "Testing",
            "How generated tests should be used.",
            [
                new("Unit tests", "Use handler test helpers for handwritten command and query handlers.", ["Arrange request, entity data and mocked dependencies.", "Execute the handler directly.", "Assert response, state changes and dependency calls."]),
                new("Integration tests", "Use integration tests for automations because generated handlers are not handwritten code.", ["Run against the real service registration.", "Use SQLite or the configured test database.", "Assert persistence, mapping, validation and query behavior together."]),
                new("Feature test layout", "Keep tests close to the feature scenario being validated.", ["Customers/CreateCustomerTests.", "Invoices/GetInvoiceByIdTests.", "Orders/ChangeOrderStatusTests."])
            ])
    ];

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

    public GuideTopic SelectedGuideTopic => GuideTopics.First(topic => topic.Key == SelectedGuideTopicKey);

    public string ProjectDirectoryPreview => string.IsNullOrWhiteSpace(ProjectName) || string.IsNullOrWhiteSpace(OutputRoot)
        ? "Complete the project name and destination folder"
        : Path.Combine(OutputRoot, ProjectName);

    public void ToggleSidebar() => SidebarCollapsed = !SidebarCollapsed;

    public void SelectGuideTopic(string key)
    {
        if (GuideTopics.Any(topic => topic.Key == key))
            SelectedGuideTopicKey = key;
    }

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

            if (result.Succeeded)
                await workspace.OpenDirectoryAsync(CreatedDirectory);
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
