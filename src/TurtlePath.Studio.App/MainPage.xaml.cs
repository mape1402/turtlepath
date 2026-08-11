using TurtlePath.Studio.Abstractions.Commands;
using TurtlePath.Studio.Abstractions.Projects;
using TurtlePath.Studio.Abstractions.Validation;
using TurtlePath.Studio.Abstractions.Workspace;
using TurtlePath.Studio.Application.Environment;
using TurtlePath.Studio.Application.UseCases;

namespace TurtlePath.Studio.App;

public partial class MainPage : ContentPage
{
    private readonly InspectStudioEnvironmentUseCase inspectEnvironment;
    private readonly CreateTurtlePathProjectUseCase createProject;
    private readonly IStudioWorkspaceService workspace;

    private readonly VerticalStackLayout content = new() { Spacing = 18 };
    private readonly VerticalStackLayout steps = new() { Spacing = 10 };
    private readonly Label title = new();
    private readonly Label status = new();
    private readonly Border messageBox = new();
    private readonly Label messageLabel = new();
    private readonly HorizontalStackLayout footer = new() { Spacing = 12, HorizontalOptions = LayoutOptions.End };

    private StudioEnvironmentReport? environment;
    private ProjectHostMode selectedHost = ProjectHostMode.ApiConsumer;
    private WizardStep step = WizardStep.ChooseTemplate;
    private string projectName = "MyService";
    private string outputRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    private bool restoreAfterCreation = true;
    private bool buildAfterCreation = true;
    private bool testAfterCreation = true;
    private bool hideDocsAfterCreation;
    private bool busy;
    private bool created;
    private string? createdDirectory;
    private string message = "Ready. Use Refresh when you want to check the local .NET and template environment.";
    private bool messageIsError;
    private IReadOnlyList<CommandExecutionResult> commands = [];

    public MainPage(
        InspectStudioEnvironmentUseCase inspectEnvironment,
        CreateTurtlePathProjectUseCase createProject,
        IStudioWorkspaceService workspace)
    {
        this.inspectEnvironment = inspectEnvironment;
        this.createProject = createProject;
        this.workspace = workspace;

        InitializeComponent();
        BuildShell();
        Render();
    }

    private void BuildShell()
    {
        var root = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(300) },
                new ColumnDefinition { Width = GridLength.Star }
            },
            BackgroundColor = Color.FromArgb("#EEF3EF")
        };

        root.Add(BuildRail(), 0, 0);
        root.Add(BuildWorkspace(), 1, 0);
        Content = root;
    }

    private View BuildRail()
    {
        var rail = new VerticalStackLayout
        {
            Spacing = 28,
            Padding = new Thickness(28),
            BackgroundColor = Color.FromArgb("#08211C")
        };

        rail.Add(new Label
        {
            Text = "TP",
            FontSize = 28,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#7CCC55")
        });

        rail.Add(new Label
        {
            Text = "TurtlePath Studio",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        });

        status.TextColor = Color.FromArgb("#D7E9DF");
        status.BackgroundColor = Color.FromArgb("#143B33");
        status.Padding = new Thickness(14, 8);
        rail.Add(status);
        rail.Add(steps);

        return rail;
    }

    private View BuildWorkspace()
    {
        var workspaceGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto }
            },
            Padding = new Thickness(34),
            RowSpacing = 18
        };

        var header = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        title.FontSize = 32;
        title.FontAttributes = FontAttributes.Bold;
        title.TextColor = Color.FromArgb("#061C18");
        header.Add(title, 0, 0);
        header.Add(CreateButton("Refresh", RefreshEnvironmentAsync, secondary: true), 1, 0);

        messageLabel.FontAttributes = FontAttributes.Bold;
        messageLabel.TextColor = Color.FromArgb("#124A1E");
        messageBox.Padding = new Thickness(14, 10);
        messageBox.StrokeThickness = 0;
        messageBox.Content = messageLabel;

        workspaceGrid.Add(header, 0, 0);
        workspaceGrid.Add(messageBox, 0, 1);
        workspaceGrid.Add(new ScrollView { Content = content }, 0, 2);
        workspaceGrid.Add(footer, 0, 3);

        return workspaceGrid;
    }

    private void Render()
    {
        title.Text = step switch
        {
            WizardStep.ChooseTemplate => "What are we creating today?",
            WizardStep.ConfigureProject => "Name it and choose where it lives",
            WizardStep.CreateProject => "Creating your project",
            WizardStep.Documentation => "Project guide",
            _ => "TurtlePath Studio"
        };

        status.Text = environment is null
            ? "Not checked"
            : environment.CanCreateProjects ? "Ready" : "Needs attention";

        messageLabel.Text = message;
        messageLabel.TextColor = messageIsError ? Color.FromArgb("#8B241A") : Color.FromArgb("#124A1E");
        messageBox.BackgroundColor = messageIsError ? Color.FromArgb("#F9DFDC") : Color.FromArgb("#DDF4D7");

        RenderSteps();
        content.Clear();
        footer.Clear();

        switch (step)
        {
            case WizardStep.ChooseTemplate:
                RenderChoose();
                break;
            case WizardStep.ConfigureProject:
                RenderConfigure();
                break;
            case WizardStep.CreateProject:
                RenderCreate();
                break;
            case WizardStep.Documentation:
                RenderDocumentation();
                break;
        }
    }

    private void RenderSteps()
    {
        steps.Clear();
        AddStep("Choose", WizardStep.ChooseTemplate);
        AddStep("Configure", WizardStep.ConfigureProject);
        AddStep("Create", WizardStep.CreateProject);
        AddStep("Guide", WizardStep.Documentation);
    }

    private void AddStep(string text, WizardStep target)
    {
        var label = new Label
        {
            Text = text,
            Padding = new Thickness(14, 12),
            FontAttributes = step == target ? FontAttributes.Bold : FontAttributes.None,
            TextColor = step == target ? Color.FromArgb("#08211C") : Color.FromArgb("#D7E9DF"),
            BackgroundColor = step == target ? Color.FromArgb("#F4FBF7") : Color.FromArgb("#143B33")
        };

        steps.Add(label);
    }

    private void RenderChoose()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star }
            },
            ColumnSpacing = 18
        };

        grid.Add(CreateProjectCard(ProjectHostMode.ApiConsumer, "API / Consumer", "REST API and message consumer host with TurtlePath defaults."), 0, 0);
        grid.Add(CreateProjectCard(ProjectHostMode.Job, "One-shot Job", "Console-style job host for Kubernetes CronJobs and scheduled execution."), 1, 0);
        content.Add(grid);

        footer.Add(CreateButton("Continue", () =>
        {
            ClearMessage();
            step = WizardStep.ConfigureProject;
            Render();
            return Task.CompletedTask;
        }));
    }

    private Button CreateProjectCard(ProjectHostMode hostMode, string cardTitle, string description)
    {
        var selected = selectedHost == hostMode;
        return new Button
        {
            Text = $"{cardTitle}{Environment.NewLine}{Environment.NewLine}{description}",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#061C18"),
            BackgroundColor = selected ? Color.FromArgb("#EDF8EA") : Colors.White,
            BorderColor = selected ? Color.FromArgb("#2E7143") : Color.FromArgb("#DCE6E0"),
            BorderWidth = selected ? 2 : 1,
            CornerRadius = 12,
            Padding = new Thickness(28),
            HeightRequest = 220,
            HorizontalOptions = LayoutOptions.Fill,
            Command = new Command(() =>
            {
                selectedHost = hostMode;
                projectName = hostMode == ProjectHostMode.Job ? "BillingJob" : "BillingService";
                Render();
            })
        };
    }

    private void RenderConfigure()
    {
        var projectNameEntry = CreateEntry(projectName, "BillingService");
        projectNameEntry.TextChanged += (_, args) => projectName = args.NewTextValue;

        var outputEntry = CreateEntry(outputRoot, "C:\\work");
        outputEntry.HorizontalOptions = LayoutOptions.Fill;
        outputEntry.TextChanged += (_, args) => outputRoot = args.NewTextValue;

        var browse = CreateButton("Browse", PickFolderAsync, secondary: true);
        var outputRow = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = 10
        };
        outputRow.Add(outputEntry, 0, 0);
        outputRow.Add(browse, 1, 0);

        content.Add(CreatePanel(
            SelectedTitle,
            CreateField("Project name", projectNameEntry),
            CreateField("Destination folder", outputRow),
            CreatePreview(),
            CreateChecks()));

        content.Add(CreateGuidePanel(compact: true));

        footer.Add(CreateButton("Back", () =>
        {
            ClearMessage();
            step = WizardStep.ChooseTemplate;
            Render();
            return Task.CompletedTask;
        }, secondary: true));

        footer.Add(CreateButton("Create project", CreateProjectAsync));
    }

    private void RenderCreate()
    {
        content.Add(CreatePanel(
            busy ? "Creating project" : created ? "Project created" : "Ready",
            new Label
            {
                Text = created ? $"Created at {createdDirectory}" : busy ? "Working..." : "Ready to run.",
                TextColor = Color.FromArgb("#38544A")
            },
            CreateExecutionLog()));

        footer.Add(CreateButton("Back", () =>
        {
            ClearMessage();
            step = WizardStep.ConfigureProject;
            Render();
            return Task.CompletedTask;
        }, secondary: true, disabled: busy));

        if (created)
        {
            footer.Add(CreateButton("Open folder", OpenCreatedFolderAsync, secondary: true));
            footer.Add(CreateButton("Continue", () =>
            {
                step = hideDocsAfterCreation ? WizardStep.ChooseTemplate : WizardStep.Documentation;
                Render();
                return Task.CompletedTask;
            }));
        }
    }

    private void RenderDocumentation()
    {
        content.Add(CreateGuidePanel(compact: false));
        content.Add(CreateExecutionLog());

        footer.Add(CreateButton("Create another project", () =>
        {
            ClearMessage();
            created = false;
            commands = [];
            createdDirectory = null;
            step = WizardStep.ChooseTemplate;
            Render();
            return Task.CompletedTask;
        }, secondary: true));

        footer.Add(CreateButton("Open folder", OpenCreatedFolderAsync));
    }

    private View CreateField(string label, View input)
    {
        var layout = new VerticalStackLayout { Spacing = 6 };
        layout.Add(new Label { Text = label, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#38544A") });
        layout.Add(input);
        return layout;
    }

    private static Entry CreateEntry(string value, string placeholder)
    {
        return new Entry
        {
            Text = value,
            Placeholder = placeholder,
            TextColor = Color.FromArgb("#061C18"),
            PlaceholderColor = Color.FromArgb("#7C9188"),
            BackgroundColor = Color.FromArgb("#F7FAF6"),
            HeightRequest = 46
        };
    }

    private View CreatePreview()
    {
        return new Label
        {
            Text = $"Project will be created at: {ProjectDirectoryPreview}",
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#2E7143")
        };
    }

    private View CreateChecks()
    {
        var layout = new HorizontalStackLayout { Spacing = 18 };
        layout.Add(CreateCheck("Restore", restoreAfterCreation, value => restoreAfterCreation = value));
        layout.Add(CreateCheck("Build", buildAfterCreation, value => buildAfterCreation = value));
        layout.Add(CreateCheck("Test", testAfterCreation, value => testAfterCreation = value));
        layout.Add(CreateCheck("Hide guide", hideDocsAfterCreation, value => hideDocsAfterCreation = value));
        return layout;
    }

    private static View CreateCheck(string text, bool value, Action<bool> changed)
    {
        var check = new CheckBox { IsChecked = value, Color = Color.FromArgb("#2E7143") };
        check.CheckedChanged += (_, args) => changed(args.Value);

        var layout = new HorizontalStackLayout { Spacing = 6 };
        layout.Add(check);
        layout.Add(new Label
        {
            Text = text,
            TextColor = Color.FromArgb("#38544A"),
            VerticalTextAlignment = TextAlignment.Center
        });
        return layout;
    }

    private View CreateGuidePanel(bool compact)
    {
        var hostText = selectedHost == ProjectHostMode.Job
            ? "Job mode creates a one-shot host for Kubernetes CronJobs. Keep business code in the same feature layout and register the job services in the host."
            : "API / Consumer mode creates the regular host for REST endpoints and message consumers. It keeps TurtlePath defaults ready for handlers, automations, jobs, exceptions and testing.";

        var details = compact
            ? hostText
            : $"{hostText}{Environment.NewLine}{Environment.NewLine}Recommended flow: create DTOs, entities and automations for happy paths. Use hooks for cross-cutting behavior. Create custom handlers only when the path stops being standard.";

        return CreatePanel("Guide", new Label
        {
            Text = details,
            TextColor = Color.FromArgb("#38544A"),
            LineBreakMode = LineBreakMode.WordWrap
        });
    }

    private View CreateExecutionLog()
    {
        if (commands.Count == 0)
            return new Label { Text = "No commands executed yet.", TextColor = Color.FromArgb("#38544A") };

        var layout = new VerticalStackLayout { Spacing = 10 };

        foreach (var command in commands)
        {
            var output = string.Join(Environment.NewLine, command.Output.TakeLast(8).Select(line => line.Text));
            layout.Add(new Label
            {
                Text = $"{command.Command.DisplayText}{Environment.NewLine}Exit code: {command.ExitCode}{Environment.NewLine}{output}",
                TextColor = command.Succeeded ? Color.FromArgb("#124A1E") : Color.FromArgb("#8B241A"),
                FontFamily = "Consolas"
            });
        }

        return layout;
    }

    private static View CreatePanel(string heading, params View[] children)
    {
        var stack = new VerticalStackLayout { Spacing = 14 };
        stack.Add(new Label
        {
            Text = heading,
            FontSize = 22,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#061C18")
        });

        foreach (var child in children)
            stack.Add(child);

        return new Border
        {
            Padding = new Thickness(24),
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#DCE6E0"),
            StrokeThickness = 1,
            Content = stack
        };
    }

    private Button CreateButton(string text, Func<Task> action, bool secondary = false, bool disabled = false)
    {
        var button = new Button
        {
            Text = text,
            FontAttributes = FontAttributes.Bold,
            TextColor = secondary ? Color.FromArgb("#061C18") : Colors.White,
            BackgroundColor = secondary ? Colors.White : Color.FromArgb("#2E7143"),
            BorderColor = Color.FromArgb("#CFE0D6"),
            BorderWidth = secondary ? 1 : 0,
            CornerRadius = 10,
            Padding = new Thickness(18, 10),
            IsEnabled = !disabled
        };

        button.Clicked += async (_, _) =>
        {
            if (busy && !disabled)
                return;

            await action();
        };

        return button;
    }

    private async Task RefreshEnvironmentAsync()
    {
        await RunAsync(async () =>
        {
            environment = await inspectEnvironment.ExecuteAsync();
            message = environment.CanCreateProjects
                ? "Environment ready."
                : "Template or .NET environment is not ready. Creating a project may fail until the template is installed.";
            messageIsError = !environment.CanCreateProjects;
        });
    }

    private async Task PickFolderAsync()
    {
        outputRoot = await workspace.PickOutputDirectoryAsync(outputRoot);
        Render();
    }

    private async Task CreateProjectAsync()
    {
        if (!ValidateInput())
            return;

        step = WizardStep.CreateProject;
        created = false;
        commands = [];
        Render();

        await RunAsync(async () =>
        {
            var result = await createProject.ExecuteAsync(new CreateProjectRequest(
                projectName.Trim(),
                ProjectDirectoryPreview,
                selectedHost,
                restoreAfterCreation,
                buildAfterCreation,
                testAfterCreation));

            commands = CollectCommands(result).ToArray();
            created = result.Succeeded;
            createdDirectory = result.Creation.ProjectDirectory;
            message = result.Succeeded
                ? "Project created successfully."
                : "Project creation finished with errors. Check the execution log.";
            messageIsError = !result.Succeeded;

            if (result.Succeeded)
                await workspace.OpenDirectoryAsync(createdDirectory);
        });
    }

    private async Task OpenCreatedFolderAsync()
    {
        if (!string.IsNullOrWhiteSpace(createdDirectory))
            await workspace.OpenDirectoryAsync(createdDirectory);
    }

    private async Task RunAsync(Func<Task> action)
    {
        busy = true;
        Render();

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
            busy = false;
            Render();
        }
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(projectName))
        {
            SetError("Project name is required.");
            Render();
            return false;
        }

        if (projectName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            SetError("Project name contains invalid characters.");
            Render();
            return false;
        }

        if (string.IsNullOrWhiteSpace(outputRoot))
        {
            SetError("Destination folder is required.");
            Render();
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
        message = text;
        messageIsError = true;
    }

    private void ClearMessage()
    {
        message = "Ready.";
        messageIsError = false;
    }

    private string SelectedTitle => selectedHost == ProjectHostMode.Job ? "One-shot Job" : "API / Consumer";

    private string ProjectDirectoryPreview => string.IsNullOrWhiteSpace(projectName) || string.IsNullOrWhiteSpace(outputRoot)
        ? "Complete the project name and destination folder"
        : Path.Combine(outputRoot, projectName);

    private enum WizardStep
    {
        ChooseTemplate = 0,
        ConfigureProject = 1,
        CreateProject = 2,
        Documentation = 3
    }
}
