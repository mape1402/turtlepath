using TurtlePath.Studio.Abstractions.Commands;
using TurtlePath.Studio.Abstractions.Projects;
using TurtlePath.Studio.Abstractions.Validation;
using TurtlePath.Studio.Abstractions.Workspace;
using TurtlePath.Studio.Application.Environment;
using TurtlePath.Studio.Application.UseCases;

namespace TurtlePath.Studio.App;

public partial class MainPage : ContentPage
{
    private static readonly Color Ink = Color.FromArgb("#081F1A");
    private static readonly Color Muted = Color.FromArgb("#5A7168");
    private static readonly Color Surface = Color.FromArgb("#F4F8F5");
    private static readonly Color Panel = Colors.White;
    private static readonly Color Primary = Color.FromArgb("#2E7143");
    private static readonly Color PrimaryDark = Color.FromArgb("#083229");
    private static readonly Color Line = Color.FromArgb("#D9E5DE");

    private readonly InspectStudioEnvironmentUseCase inspectEnvironment;
    private readonly CreateTurtlePathProjectUseCase createProject;
    private readonly IStudioWorkspaceService workspace;

    private readonly Grid root = new();
    private readonly VerticalStackLayout navigation = new() { Spacing = 8 };
    private readonly Grid workspaceGrid = new();
    private readonly ContentView body = new();
    private readonly ContentView modalHost = new() { IsVisible = false };
    private readonly Label title = new();
    private readonly Label subtitle = new();
    private readonly Label environmentChip = new();

    private StudioSection section = StudioSection.Home;
    private StudioEnvironmentReport? environment;
    private ProjectHostMode selectedHost = ProjectHostMode.ApiConsumer;
    private WizardStep wizardStep = WizardStep.Basics;
    private string projectName = "BillingService";
    private string outputRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    private bool restoreAfterCreation = true;
    private bool buildAfterCreation = true;
    private bool testAfterCreation = true;
    private bool hideGuideAfterCreation;
    private bool busy;
    private bool created;
    private string? createdDirectory;
    private string message = "Ready.";
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
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(292) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        root.BackgroundColor = Surface;

        root.Add(BuildSidebar(), 0, 0);
        root.Add(BuildWorkspace(), 1, 0);
        root.Add(modalHost, 0, 0);
        Grid.SetColumnSpan(modalHost, 2);

        Content = root;
    }

    private View BuildSidebar()
    {
        var sidebar = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto }
            },
            Padding = new Thickness(24, 28),
            RowSpacing = 26,
            BackgroundColor = PrimaryDark
        };

        var brand = new VerticalStackLayout { Spacing = 12 };
        brand.Add(new Label
        {
            Text = "TP",
            FontSize = 32,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#7CCC55")
        });
        brand.Add(new Label
        {
            Text = "TurtlePath Studio",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        });
        brand.Add(new Label
        {
            Text = "Project launcher",
            TextColor = Color.FromArgb("#B9D2C8")
        });

        environmentChip.Padding = new Thickness(12, 8);
        environmentChip.TextColor = Color.FromArgb("#D7E9DF");
        environmentChip.BackgroundColor = Color.FromArgb("#15483B");

        sidebar.Add(brand, 0, 0);
        sidebar.Add(environmentChip, 0, 1);
        sidebar.Add(navigation, 0, 2);
        sidebar.Add(CreateSideButton("Refresh environment", StudioSection.Environment), 0, 3);

        return sidebar;
    }

    private View BuildWorkspace()
    {
        workspaceGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        workspaceGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
        workspaceGrid.Padding = new Thickness(34, 30);
        workspaceGrid.RowSpacing = 22;

        var header = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = 16
        };

        var heading = new VerticalStackLayout { Spacing = 4 };
        title.FontSize = 34;
        title.FontAttributes = FontAttributes.Bold;
        title.TextColor = Ink;
        subtitle.TextColor = Muted;
        subtitle.FontSize = 15;
        heading.Add(title);
        heading.Add(subtitle);

        header.Add(heading, 0, 0);
        header.Add(CreateButton("Refresh", RefreshEnvironmentAsync, secondary: true), 1, 0);

        workspaceGrid.Add(header, 0, 0);
        workspaceGrid.Add(body, 0, 1);
        return workspaceGrid;
    }

    private void Render()
    {
        RenderNavigation();
        environmentChip.Text = environment is null
            ? "Environment: not checked"
            : environment.CanCreateProjects ? "Environment: ready" : "Environment: needs attention";

        (title.Text, subtitle.Text) = section switch
        {
            StudioSection.Home => ("Build TurtlePath projects faster", "Start from the template, read the guide, or check your local environment."),
            StudioSection.Templates => ("Templates", "Pick the host type and let the wizard create the project."),
            StudioSection.Guides => ("Usage guides", "Step-by-step notes for generated projects and TurtlePath conventions."),
            StudioSection.Environment => ("Environment", "Validate .NET and the installed TurtlePath template package."),
            _ => ("TurtlePath Studio", "Project launcher")
        };

        body.Content = section switch
        {
            StudioSection.Home => BuildHome(),
            StudioSection.Templates => BuildTemplates(),
            StudioSection.Guides => BuildGuides(),
            StudioSection.Environment => BuildEnvironment(),
            _ => BuildHome()
        };
    }

    private void RenderNavigation()
    {
        navigation.Clear();
        navigation.Add(CreateSideButton("Home", StudioSection.Home));
        navigation.Add(CreateSideButton("Templates", StudioSection.Templates));
        navigation.Add(CreateSideButton("Usage guides", StudioSection.Guides));
        navigation.Add(CreateSideButton("Environment", StudioSection.Environment));
    }

    private Button CreateSideButton(string text, StudioSection target)
    {
        var selected = section == target;
        var button = new Button
        {
            Text = text,
            HorizontalOptions = LayoutOptions.Fill,
            Padding = new Thickness(16, 12),
            CornerRadius = 8,
            FontAttributes = selected ? FontAttributes.Bold : FontAttributes.None,
            TextColor = selected ? Ink : Color.FromArgb("#D7E9DF"),
            BackgroundColor = selected ? Color.FromArgb("#F1FAF4") : Color.FromArgb("#15483B"),
            BorderWidth = 0
        };

        button.Clicked += (_, _) =>
        {
            section = target;
            Render();
        };

        return button;
    }

    private View BuildHome()
    {
        var layout = new VerticalStackLayout { Spacing = 22 };

        layout.Add(CreateHero());

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star }
            },
            ColumnSpacing = 16
        };

        grid.Add(CreateActionCard("Create from template", "API / Consumer or one-shot Job with a focused wizard.", "Open templates", () =>
        {
            section = StudioSection.Templates;
            Render();
        }), 0, 0);
        grid.Add(CreateActionCard("Read the guide", "Review conventions, structure, automations and testing.", "Open guides", () =>
        {
            section = StudioSection.Guides;
            Render();
        }), 1, 0);
        grid.Add(CreateActionCard("Check environment", "Validate local .NET and the TurtlePath template package.", "Inspect", () =>
        {
            section = StudioSection.Environment;
            Render();
        }), 2, 0);

        layout.Add(grid);
        return new ScrollView { Content = layout };
    }

    private View CreateHero()
    {
        var panel = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            Padding = new Thickness(28),
            BackgroundColor = Color.FromArgb("#FFFFFF")
        };

        var copy = new VerticalStackLayout { Spacing = 10 };
        copy.Add(new Label
        {
            Text = "TurtlePath project creation without terminal friction.",
            FontSize = 26,
            FontAttributes = FontAttributes.Bold,
            TextColor = Ink
        });
        copy.Add(new Label
        {
            Text = "Choose a template, configure name and path, then generate, restore, build and test from one place.",
            TextColor = Muted,
            FontSize = 16
        });

        panel.Add(copy, 0, 0);
        panel.Add(CreateButton("Create project", () =>
        {
            section = StudioSection.Templates;
            Render();
            return Task.CompletedTask;
        }), 1, 0);

        return CreateBorder(panel);
    }

    private View BuildTemplates()
    {
        var layout = new VerticalStackLayout { Spacing = 18 };
        layout.Add(CreateMessage());

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star }
            },
            ColumnSpacing = 18
        };

        grid.Add(CreateTemplateCard(
            ProjectHostMode.ApiConsumer,
            "API / Consumer",
            "REST API and message consumer in one host.",
            "Includes TurtlePath defaults, exception handling, jobs, testing, DataScorpio, OctoMap and Crabalidator-ready structure."), 0, 0);

        grid.Add(CreateTemplateCard(
            ProjectHostMode.Job,
            "One-shot Job",
            "Kubernetes CronJob style execution.",
            "Creates a focused job host while keeping the same Business and persistence conventions as the service template."), 1, 0);

        layout.Add(grid);
        layout.Add(CreateGuidePreview());
        return new ScrollView { Content = layout };
    }

    private View CreateTemplateCard(ProjectHostMode hostMode, string name, string summary, string details)
    {
        var layout = new VerticalStackLayout { Spacing = 14 };
        layout.Add(new Label
        {
            Text = hostMode == ProjectHostMode.Job ? "JOB" : "API",
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = Primary
        });
        layout.Add(new Label
        {
            Text = name,
            FontSize = 26,
            FontAttributes = FontAttributes.Bold,
            TextColor = Ink
        });
        layout.Add(new Label { Text = summary, FontSize = 16, TextColor = Ink });
        layout.Add(new Label { Text = details, TextColor = Muted, LineBreakMode = LineBreakMode.WordWrap });

        var actions = new HorizontalStackLayout { Spacing = 10 };
        actions.Add(CreateButton("Create", () =>
        {
            OpenWizard(hostMode);
            return Task.CompletedTask;
        }));
        actions.Add(CreateButton("Guide", () =>
        {
            section = StudioSection.Guides;
            Render();
            return Task.CompletedTask;
        }, secondary: true));
        layout.Add(actions);

        return CreateBorder(layout, minHeight: 260);
    }

    private View CreateGuidePreview()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = 16
        };
        var copy = new VerticalStackLayout { Spacing = 6 };
        copy.Add(new Label
        {
            Text = "Need the assistant?",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = Ink
        });
        copy.Add(new Label
        {
            Text = "Open the usage guide after creation or jump to the documentation section now.",
            TextColor = Muted
        });
        grid.Add(copy, 0, 0);
        grid.Add(CreateButton("Open guide", () =>
        {
            section = StudioSection.Guides;
            Render();
            return Task.CompletedTask;
        }, secondary: true), 1, 0);
        return CreateBorder(grid);
    }

    private View BuildGuides()
    {
        var layout = new VerticalStackLayout { Spacing = 18 };

        layout.Add(CreateDocSection("Recommended flow",
            "Use automations for happy paths. Create DTOs and entities first, then register the automation profile. Add hooks for cross-cutting behavior. Write custom handlers only when the workflow stops being standard."));

        layout.Add(CreateDocSection("Generated structure",
            "Replace the Feature folder with the real feature name, for example Customers or Invoices. Keep Commands, Queries, Validators, Mappings, Hooks, Automations, Models and Services scoped to that feature."));

        layout.Add(CreateDocSection("Testing",
            "The template includes testing setup so developers can focus on scenario code. Handlers can be tested directly; automations are better validated through integration tests."));

        layout.Add(CreateDocSection("Jobs",
            "Use API / Consumer when the service owns HTTP or consumers. Use One-shot Job when Kubernetes owns the schedule and the executable should finish after completing its work."));

        return new ScrollView { Content = layout };
    }

    private View CreateDocSection(string heading, string text)
    {
        var layout = new VerticalStackLayout { Spacing = 8 };
        layout.Add(new Label
        {
            Text = heading,
            FontSize = 22,
            FontAttributes = FontAttributes.Bold,
            TextColor = Ink
        });
        layout.Add(new Label
        {
            Text = text,
            FontSize = 15,
            TextColor = Muted,
            LineBreakMode = LineBreakMode.WordWrap
        });
        return CreateBorder(layout);
    }

    private View BuildEnvironment()
    {
        var layout = new VerticalStackLayout { Spacing = 18 };
        layout.Add(CreateMessage());

        layout.Add(CreateDocSection(
            "Local status",
            environment is null
                ? "Environment has not been checked yet."
                : environment.CanCreateProjects
                    ? "Environment is ready to create TurtlePath projects."
                    : "Environment needs attention. Refresh to inspect .NET and the installed template package."));

        layout.Add(CreateButton("Refresh environment", RefreshEnvironmentAsync));
        return layout;
    }

    private View CreateActionCard(string heading, string text, string actionText, Action action)
    {
        var layout = new VerticalStackLayout { Spacing = 12 };
        layout.Add(new Label
        {
            Text = heading,
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = Ink
        });
        layout.Add(new Label { Text = text, TextColor = Muted, LineBreakMode = LineBreakMode.WordWrap });

        var button = CreateButton(actionText, () =>
        {
            action();
            return Task.CompletedTask;
        }, secondary: true);
        layout.Add(button);
        return CreateBorder(layout, minHeight: 190);
    }

    private void OpenWizard(ProjectHostMode hostMode)
    {
        selectedHost = hostMode;
        projectName = hostMode == ProjectHostMode.Job ? "BillingJob" : "BillingService";
        wizardStep = WizardStep.Basics;
        created = false;
        createdDirectory = null;
        commands = [];
        message = "Ready.";
        messageIsError = false;
        RenderWizard();
        modalHost.IsVisible = true;
    }

    private void RenderWizard()
    {
        var overlay = new Grid
        {
            BackgroundColor = Color.FromRgba(4, 16, 13, 0.58),
            Padding = new Thickness(42)
        };

        var modal = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto }
            },
            RowSpacing = 18,
            Padding = new Thickness(28),
            BackgroundColor = Colors.White,
            WidthRequest = 820,
            HeightRequest = 640,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        modal.Add(BuildWizardHeader(), 0, 0);
        modal.Add(BuildWizardSteps(), 0, 1);
        modal.Add(new ScrollView { Content = BuildWizardBody() }, 0, 2);
        modal.Add(BuildWizardFooter(), 0, 3);

        overlay.Add(CreateBorder(modal, stroke: Color.FromArgb("#CFE0D6")));
        modalHost.Content = overlay;
    }

    private View BuildWizardHeader()
    {
        var header = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        var copy = new VerticalStackLayout { Spacing = 4 };
        copy.Add(new Label
        {
            Text = selectedHost == ProjectHostMode.Job ? "Create one-shot job" : "Create API / Consumer service",
            FontSize = 28,
            FontAttributes = FontAttributes.Bold,
            TextColor = Ink
        });
        copy.Add(new Label
        {
            Text = "Configure the essentials and let TurtlePath generate the project.",
            TextColor = Muted
        });

        header.Add(copy, 0, 0);
        header.Add(CreateButton("Close", CloseWizardAsync, secondary: true), 1, 0);
        return header;
    }

    private View BuildWizardSteps()
    {
        var steps = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star }
            },
            ColumnSpacing = 8
        };

        steps.Add(CreateWizardStep("1. Basics", WizardStep.Basics), 0, 0);
        steps.Add(CreateWizardStep("2. Options", WizardStep.Options), 1, 0);
        steps.Add(CreateWizardStep("3. Review", WizardStep.Review), 2, 0);
        steps.Add(CreateWizardStep("4. Result", WizardStep.Result), 3, 0);
        return steps;
    }

    private View CreateWizardStep(string text, WizardStep target)
    {
        var active = wizardStep == target;
        return new Label
        {
            Text = text,
            Padding = new Thickness(12, 10),
            HorizontalTextAlignment = TextAlignment.Center,
            FontAttributes = active ? FontAttributes.Bold : FontAttributes.None,
            TextColor = active ? Colors.White : Muted,
            BackgroundColor = active ? Primary : Color.FromArgb("#EDF3EF")
        };
    }

    private View BuildWizardBody()
    {
        return wizardStep switch
        {
            WizardStep.Basics => BuildBasicsStep(),
            WizardStep.Options => BuildOptionsStep(),
            WizardStep.Review => BuildReviewStep(),
            WizardStep.Result => BuildResultStep(),
            _ => BuildBasicsStep()
        };
    }

    private View BuildBasicsStep()
    {
        var layout = new VerticalStackLayout { Spacing = 16 };

        var nameEntry = CreateEntry(projectName, "BillingService");
        nameEntry.TextChanged += (_, args) => projectName = args.NewTextValue;

        var pathEntry = CreateEntry(outputRoot, "C:\\work");
        pathEntry.HorizontalOptions = LayoutOptions.Fill;
        pathEntry.TextChanged += (_, args) => outputRoot = args.NewTextValue;

        var pathRow = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = 10
        };
        pathRow.Add(pathEntry, 0, 0);
        pathRow.Add(CreateButton("Browse", PickFolderInWizardAsync, secondary: true), 1, 0);

        layout.Add(CreateField("Project name", nameEntry));
        layout.Add(CreateField("Destination folder", pathRow));
        layout.Add(CreateMessage($"Project will be created at: {ProjectDirectoryPreview}", error: false));
        return layout;
    }

    private View BuildOptionsStep()
    {
        var layout = new VerticalStackLayout { Spacing = 16 };
        layout.Add(CreateDocSection("Validation after creation", "Choose what Studio should run after creating the project."));
        layout.Add(CreateSwitchRow("Restore packages", "Runs package restore after template generation.", restoreAfterCreation, value => restoreAfterCreation = value));
        layout.Add(CreateSwitchRow("Build project", "Compiles the generated solution.", buildAfterCreation, value => buildAfterCreation = value));
        layout.Add(CreateSwitchRow("Run tests", "Executes the generated test project.", testAfterCreation, value => testAfterCreation = value));
        layout.Add(CreateSwitchRow("Skip guide after success", "Goes back to Templates after creating the project.", hideGuideAfterCreation, value => hideGuideAfterCreation = value));
        return layout;
    }

    private View BuildReviewStep()
    {
        var layout = new VerticalStackLayout { Spacing = 14 };
        layout.Add(CreateDocSection("Ready to create", "Review the project settings before executing the template command."));
        layout.Add(CreateSummaryRow("Template", selectedHost == ProjectHostMode.Job ? "One-shot Job" : "API / Consumer"));
        layout.Add(CreateSummaryRow("Project name", projectName));
        layout.Add(CreateSummaryRow("Destination", ProjectDirectoryPreview));
        layout.Add(CreateSummaryRow("Validation", $"{BoolText(restoreAfterCreation)} restore, {BoolText(buildAfterCreation)} build, {BoolText(testAfterCreation)} test"));
        layout.Add(CreateMessage());
        return layout;
    }

    private View BuildResultStep()
    {
        var layout = new VerticalStackLayout { Spacing = 14 };
        layout.Add(CreateMessage());
        layout.Add(new Label
        {
            Text = created ? $"Created at {createdDirectory}" : busy ? "Creating project..." : "No project created yet.",
            FontAttributes = FontAttributes.Bold,
            TextColor = created ? Primary : Muted
        });
        layout.Add(CreateExecutionLog());
        return layout;
    }

    private View BuildWizardFooter()
    {
        var footer = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        var left = new HorizontalStackLayout { Spacing = 10 };
        left.Add(CreateButton("Help", () =>
        {
            section = StudioSection.Guides;
            modalHost.IsVisible = false;
            Render();
            return Task.CompletedTask;
        }, secondary: true));

        if (created)
            left.Add(CreateButton("Open folder", OpenCreatedFolderAsync, secondary: true));

        var right = new HorizontalStackLayout { Spacing = 10 };
        if (wizardStep != WizardStep.Basics && !busy)
            right.Add(CreateButton("Back", PreviousWizardStepAsync, secondary: true));

        right.Add(wizardStep switch
        {
            WizardStep.Basics => CreateButton("Continue", NextWizardStepAsync),
            WizardStep.Options => CreateButton("Continue", NextWizardStepAsync),
            WizardStep.Review => CreateButton("Create project", CreateProjectFromWizardAsync, disabled: busy),
            WizardStep.Result => CreateButton(created && hideGuideAfterCreation ? "Done" : "Open guide", FinishWizardAsync),
            _ => CreateButton("Continue", NextWizardStepAsync)
        });

        footer.Add(left, 0, 0);
        footer.Add(right, 1, 0);
        return footer;
    }

    private View CreateSwitchRow(string heading, string description, bool value, Action<bool> changed)
    {
        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            Padding = new Thickness(14),
            BackgroundColor = Color.FromArgb("#F7FAF6")
        };

        var copy = new VerticalStackLayout { Spacing = 3 };
        copy.Add(new Label { Text = heading, FontAttributes = FontAttributes.Bold, TextColor = Ink });
        copy.Add(new Label { Text = description, TextColor = Muted });

        var toggle = new Switch
        {
            IsToggled = value,
            OnColor = Primary,
            ThumbColor = Colors.White
        };
        toggle.Toggled += (_, args) => changed(args.Value);

        row.Add(copy, 0, 0);
        row.Add(toggle, 1, 0);
        return row;
    }

    private View CreateSummaryRow(string label, string value)
    {
        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(150) },
                new ColumnDefinition { Width = GridLength.Star }
            },
            Padding = new Thickness(0, 4)
        };
        row.Add(new Label { Text = label, TextColor = Muted, FontAttributes = FontAttributes.Bold }, 0, 0);
        row.Add(new Label { Text = value, TextColor = Ink, LineBreakMode = LineBreakMode.WordWrap }, 1, 0);
        return row;
    }

    private async Task NextWizardStepAsync()
    {
        if (wizardStep == WizardStep.Basics && !ValidateInput())
        {
            RenderWizard();
            return;
        }

        wizardStep = wizardStep switch
        {
            WizardStep.Basics => WizardStep.Options,
            WizardStep.Options => WizardStep.Review,
            WizardStep.Review => WizardStep.Result,
            _ => wizardStep
        };
        RenderWizard();
        await Task.CompletedTask;
    }

    private async Task PreviousWizardStepAsync()
    {
        wizardStep = wizardStep switch
        {
            WizardStep.Options => WizardStep.Basics,
            WizardStep.Review => WizardStep.Options,
            WizardStep.Result => WizardStep.Review,
            _ => wizardStep
        };
        RenderWizard();
        await Task.CompletedTask;
    }

    private async Task CloseWizardAsync()
    {
        modalHost.IsVisible = false;
        await Task.CompletedTask;
    }

    private async Task FinishWizardAsync()
    {
        modalHost.IsVisible = false;
        if (created && !hideGuideAfterCreation)
            section = StudioSection.Guides;
        Render();
        await Task.CompletedTask;
    }

    private async Task PickFolderInWizardAsync()
    {
        outputRoot = await workspace.PickOutputDirectoryAsync(outputRoot);
        RenderWizard();
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

    private async Task CreateProjectFromWizardAsync()
    {
        if (!ValidateInput())
        {
            wizardStep = WizardStep.Basics;
            RenderWizard();
            return;
        }

        wizardStep = WizardStep.Result;
        created = false;
        commands = [];
        RenderWizard();

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
        }, renderWizard: true);
    }

    private async Task OpenCreatedFolderAsync()
    {
        if (!string.IsNullOrWhiteSpace(createdDirectory))
            await workspace.OpenDirectoryAsync(createdDirectory);
    }

    private async Task RunAsync(Func<Task> action, bool renderWizard = false)
    {
        busy = true;
        if (renderWizard)
            RenderWizard();
        else
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
            if (renderWizard)
                RenderWizard();
            else
                Render();
        }
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(projectName))
        {
            SetError("Project name is required.");
            return false;
        }

        if (projectName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            SetError("Project name contains invalid characters.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(outputRoot))
        {
            SetError("Destination folder is required.");
            return false;
        }

        ClearMessage();
        return true;
    }

    private View CreateField(string label, View input)
    {
        var layout = new VerticalStackLayout { Spacing = 7 };
        layout.Add(new Label { Text = label, FontAttributes = FontAttributes.Bold, TextColor = Ink });
        layout.Add(input);
        return layout;
    }

    private static Entry CreateEntry(string value, string placeholder)
    {
        return new Entry
        {
            Text = value,
            Placeholder = placeholder,
            TextColor = Ink,
            PlaceholderColor = Color.FromArgb("#7C9188"),
            BackgroundColor = Color.FromArgb("#F7FAF6"),
            HeightRequest = 48
        };
    }

    private Button CreateButton(string text, Func<Task> action, bool secondary = false, bool disabled = false)
    {
        var button = new Button
        {
            Text = text,
            FontAttributes = FontAttributes.Bold,
            TextColor = secondary ? Ink : Colors.White,
            BackgroundColor = secondary ? Colors.White : Primary,
            BorderColor = secondary ? Line : Primary,
            BorderWidth = secondary ? 1 : 0,
            CornerRadius = 9,
            Padding = new Thickness(18, 11),
            IsEnabled = !disabled && !busy
        };

        button.Clicked += async (_, _) =>
        {
            if (busy)
                return;

            await action();
        };

        return button;
    }

    private View CreateMessage() => CreateMessage(message, messageIsError);

    private static View CreateMessage(string text, bool error)
    {
        return new Border
        {
            Padding = new Thickness(14, 10),
            StrokeThickness = 0,
            BackgroundColor = error ? Color.FromArgb("#F9DFDC") : Color.FromArgb("#DDF4D7"),
            Content = new Label
            {
                Text = text,
                TextColor = error ? Color.FromArgb("#8B241A") : Color.FromArgb("#124A1E"),
                FontAttributes = FontAttributes.Bold,
                LineBreakMode = LineBreakMode.WordWrap
            }
        };
    }

    private static View CreateBorder(View content, double minHeight = -1, Color? stroke = null)
    {
        return new Border
        {
            Padding = new Thickness(24),
            BackgroundColor = Panel,
            Stroke = stroke ?? Line,
            StrokeThickness = 1,
            MinimumHeightRequest = minHeight,
            Content = content
        };
    }

    private View CreateExecutionLog()
    {
        if (commands.Count == 0)
            return new Label { Text = "No commands executed yet.", TextColor = Muted };

        var layout = new VerticalStackLayout { Spacing = 10 };
        foreach (var command in commands)
        {
            var output = string.Join(Environment.NewLine, command.Output.TakeLast(8).Select(line => line.Text));
            layout.Add(CreateBorder(new Label
            {
                Text = $"{command.Command.DisplayText}{Environment.NewLine}Exit code: {command.ExitCode}{Environment.NewLine}{output}",
                TextColor = command.Succeeded ? Color.FromArgb("#124A1E") : Color.FromArgb("#8B241A"),
                FontFamily = "Consolas",
                LineBreakMode = LineBreakMode.WordWrap
            }));
        }

        return layout;
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

    private static string BoolText(bool value) => value ? "yes" : "no";

    private string ProjectDirectoryPreview => string.IsNullOrWhiteSpace(projectName) || string.IsNullOrWhiteSpace(outputRoot)
        ? "Complete the project name and destination folder"
        : Path.Combine(outputRoot, projectName);

    private enum StudioSection
    {
        Home,
        Templates,
        Guides,
        Environment
    }

    private enum WizardStep
    {
        Basics,
        Options,
        Review,
        Result
    }
}
