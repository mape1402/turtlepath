using TurtlePath.Studio.Abstractions.Commands;
using TurtlePath.Studio.Abstractions.Projects;
using TurtlePath.Studio.App.ViewModels;
using Microsoft.Maui.Controls.Shapes;

namespace TurtlePath.Studio.App;

public partial class MainPage : ContentPage
{
    private const string IconFont = "Segoe MDL2 Assets";

    private static readonly Color Ink = Color.FromArgb("#081F1A");
    private static readonly Color Muted = Color.FromArgb("#5A7168");
    private static readonly Color Surface = Color.FromArgb("#F4F8F5");
    private static readonly Color Panel = Colors.White;
    private static readonly Color Primary = Color.FromArgb("#2E7143");
    private static readonly Color PrimaryDark = Color.FromArgb("#083229");
    private static readonly Color SidebarMuted = Color.FromArgb("#9AB9AD");
    private static readonly Color SidebarTrack = Color.FromArgb("#0D3C31");
    private static readonly Color SidebarActive = Color.FromArgb("#EAF5EE");
    private static readonly Color SidebarAccent = Color.FromArgb("#7CCC55");
    private static readonly Color Line = Color.FromArgb("#D9E5DE");

    private readonly StudioViewModel viewModel;
    private readonly Grid root = new();
    private readonly VerticalStackLayout navigation = new() { Spacing = 4 };
    private readonly ContentView body = new();
    private readonly ContentView modalHost = new() { IsVisible = false };
    private readonly Label title = new();
    private readonly Label subtitle = new();
    private readonly Label sidebarLogo = new();
    private readonly Label sidebarTitle = new();
    private readonly Label sidebarSubtitle = new();
    private readonly Button sidebarToggle = new();

    public MainPage(StudioViewModel viewModel)
    {
        this.viewModel = viewModel;

        InitializeComponent();
        BuildShell();
        Render();
    }

    private void BuildShell()
    {
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(284) });
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
                new RowDefinition { Height = GridLength.Star }
            },
            Padding = new Thickness(16, 24),
            RowSpacing = 22,
            BackgroundColor = PrimaryDark
        };

        var brand = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = 8
        };

        sidebarLogo.Text = "TP";
        sidebarLogo.FontSize = 32;
        sidebarLogo.FontAttributes = FontAttributes.Bold;
        sidebarLogo.TextColor = SidebarAccent;
        sidebarLogo.VerticalTextAlignment = TextAlignment.Center;
        brand.Add(sidebarLogo, 0, 0);

        var brandText = new VerticalStackLayout { Spacing = 2 };
        sidebarTitle.Text = "TurtlePath Studio";
        sidebarTitle.FontSize = 19;
        sidebarTitle.FontAttributes = FontAttributes.Bold;
        sidebarTitle.TextColor = Colors.White;
        sidebarSubtitle.Text = "Project launcher";
        sidebarSubtitle.TextColor = Color.FromArgb("#B9D2C8");
        brandText.Add(sidebarTitle);
        brandText.Add(sidebarSubtitle);
        brand.Add(brandText, 1, 0);

        sidebarToggle.Text = "\uE700";
        sidebarToggle.FontFamily = IconFont;
        sidebarToggle.FontSize = 16;
        sidebarToggle.TextColor = SidebarMuted;
        sidebarToggle.BackgroundColor = Color.FromArgb("#124A3C");
        sidebarToggle.BorderWidth = 0;
        sidebarToggle.CornerRadius = 10;
        sidebarToggle.WidthRequest = 38;
        sidebarToggle.HeightRequest = 38;
        sidebarToggle.MinimumWidthRequest = 38;
        sidebarToggle.MinimumHeightRequest = 38;
        sidebarToggle.Padding = 0;
        sidebarToggle.Clicked += (_, _) =>
        {
            viewModel.ToggleSidebar();
            Render();
        };
        brand.Add(sidebarToggle, 2, 0);

        sidebar.Add(brand, 0, 0);
        sidebar.Add(navigation, 0, 1);
        return sidebar;
    }

    private View BuildWorkspace()
    {
        var workspace = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            },
            Padding = new Thickness(34, 30),
            RowSpacing = 22
        };

        var heading = new VerticalStackLayout { Spacing = 4 };
        title.FontSize = 34;
        title.FontAttributes = FontAttributes.Bold;
        title.TextColor = Ink;
        subtitle.TextColor = Muted;
        subtitle.FontSize = 15;
        heading.Add(title);
        heading.Add(subtitle);

        workspace.Add(heading, 0, 0);
        workspace.Add(body, 0, 1);
        return workspace;
    }

    private void Render()
    {
        root.ColumnDefinitions[0].Width = new GridLength(viewModel.SidebarCollapsed ? 84 : 284);
        sidebarLogo.IsVisible = !viewModel.SidebarCollapsed;
        sidebarTitle.IsVisible = !viewModel.SidebarCollapsed;
        sidebarSubtitle.IsVisible = !viewModel.SidebarCollapsed;

        title.Text = viewModel.PageTitle;
        subtitle.Text = viewModel.PageSubtitle;

        RenderNavigation();
        body.Content = viewModel.Section switch
        {
            StudioSection.Home => BuildHome(),
            StudioSection.Templates => BuildTemplates(),
            StudioSection.Guides => BuildGuides(),
            StudioSection.Environment => BuildEnvironment(),
            _ => BuildHome()
        };

        if (viewModel.IsWizardOpen)
            RenderWizard();
        else
            modalHost.IsVisible = false;
    }

    private void RenderNavigation()
    {
        navigation.Clear();
        navigation.Add(CreateSideItem("Home", "\uE80F", "Start", StudioSection.Home));
        navigation.Add(CreateSideItem("Templates", "\uE8A5", "Create projects", StudioSection.Templates));
        navigation.Add(CreateSideItem("Guides", "\uE82D", "Use the template", StudioSection.Guides));
        navigation.Add(CreateSideItem("Environment", "\uE713", "Setup tools", StudioSection.Environment));
    }

    private View CreateSideItem(string text, string iconGlyph, string caption, StudioSection target)
    {
        var selected = viewModel.Section == target;
        var item = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star }
            },
            ColumnSpacing = 12,
            VerticalOptions = LayoutOptions.Center
        };

        var leading = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = 8,
            VerticalOptions = LayoutOptions.Center
        };

        leading.Add(new BoxView
        {
            WidthRequest = 3,
            HeightRequest = 30,
            Color = selected ? SidebarAccent : Colors.Transparent,
            VerticalOptions = LayoutOptions.Center
        }, 0, 0);

        var mark = new Border
        {
            WidthRequest = 40,
            HeightRequest = 40,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            BackgroundColor = selected && !viewModel.SidebarCollapsed ? SidebarActive : Colors.Transparent,
            Content = new Label
            {
                Text = iconGlyph,
                FontFamily = IconFont,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                TextColor = selected
                    ? viewModel.SidebarCollapsed ? SidebarAccent : PrimaryDark
                    : SidebarMuted,
                FontSize = 18
            }
        };

        leading.Add(mark, 1, 0);
        item.Add(leading, 0, 0);

        if (!viewModel.SidebarCollapsed)
        {
            var copy = new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center };
            copy.Add(new Label
            {
                Text = text,
                FontAttributes = selected ? FontAttributes.Bold : FontAttributes.None,
                TextColor = selected ? Colors.White : Color.FromArgb("#DCEBE5"),
                FontSize = 14
            });
            copy.Add(new Label
            {
                Text = caption,
                TextColor = selected ? Color.FromArgb("#BFE1D1") : SidebarMuted,
                FontSize = 12
            });
            item.Add(copy, 1, 0);
        }

        var border = new Border
        {
            Padding = new Thickness(viewModel.SidebarCollapsed ? 0 : 2, 6),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            BackgroundColor = selected && !viewModel.SidebarCollapsed ? SidebarTrack : Colors.Transparent,
            Content = item
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
        {
            viewModel.Navigate(target);
            Render();
        };
        border.GestureRecognizers.Add(tap);

        return border;
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

        grid.Add(CreateActionCard("Create from template", "API / Consumer or one-shot Job with a focused wizard.", "Open templates", () => Navigate(StudioSection.Templates)), 0, 0);
        grid.Add(CreateActionCard("Read the guide", "Use the project step-by-step guide instead of guessing structure.", "Open guides", () => Navigate(StudioSection.Guides)), 1, 0);
        grid.Add(CreateActionCard("Repair environment", "Install or update the TurtlePath template when setup is incomplete.", "Open setup", () => Navigate(StudioSection.Environment)), 2, 0);

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
            BackgroundColor = Colors.White
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
        panel.Add(CreateButton("Create project", () => Navigate(StudioSection.Templates)), 1, 0);
        return CreateBorder(panel);
    }

    private View BuildTemplates()
    {
        var layout = new VerticalStackLayout { Spacing = 18 };

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
            viewModel.OpenWizard(hostMode);
            Render();
        }));
        actions.Add(CreateButton("Guide", () => Navigate(StudioSection.Guides), secondary: true));
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
            Text = "Open the guide after creation or jump to the documentation section now.",
            TextColor = Muted
        });
        grid.Add(copy, 0, 0);
        grid.Add(CreateButton("Open guide", () => Navigate(StudioSection.Guides), secondary: true), 1, 0);
        return CreateBorder(grid);
    }

    private View BuildGuides()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(300) },
                new ColumnDefinition { Width = GridLength.Star }
            },
            ColumnSpacing = 18
        };

        grid.Add(BuildGuideIndex(), 0, 0);
        grid.Add(BuildGuideDetails(), 1, 0);
        return grid;
    }

    private View BuildGuideIndex()
    {
        var index = new VerticalStackLayout { Spacing = 10 };
        index.Add(new Label
        {
            Text = "Template assistant",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = Ink
        });
        index.Add(new Label
        {
            Text = "Pick a topic and follow the steps. This is the generated project playbook.",
            TextColor = Muted,
            LineBreakMode = LineBreakMode.WordWrap
        });

        foreach (var topic in viewModel.GuideTopics)
            index.Add(CreateGuideIndexItem(topic));

        return CreateBorder(new ScrollView { Content = index });
    }

    private View CreateGuideIndexItem(GuideTopic topic)
    {
        var selected = viewModel.SelectedGuideTopicKey == topic.Key;
        var layout = new VerticalStackLayout { Spacing = 4 };
        layout.Add(new Label
        {
            Text = topic.Title,
            FontAttributes = FontAttributes.Bold,
            TextColor = selected ? Colors.White : Ink
        });
        layout.Add(new Label
        {
            Text = topic.Summary,
            TextColor = selected ? Color.FromArgb("#E2F1E8") : Muted,
            FontSize = 12,
            LineBreakMode = LineBreakMode.WordWrap
        });

        var border = new Border
        {
            Padding = new Thickness(14, 12),
            StrokeThickness = selected ? 0 : 1,
            Stroke = Line,
            BackgroundColor = selected ? Primary : Color.FromArgb("#F9FBFA"),
            Content = layout
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
        {
            viewModel.SelectGuideTopic(topic.Key);
            Render();
        };
        border.GestureRecognizers.Add(tap);
        return border;
    }

    private View BuildGuideDetails()
    {
        var topic = viewModel.SelectedGuideTopic;
        var layout = new VerticalStackLayout { Spacing = 16 };

        layout.Add(new Label
        {
            Text = topic.Title,
            FontSize = 28,
            FontAttributes = FontAttributes.Bold,
            TextColor = Ink
        });
        layout.Add(new Label
        {
            Text = topic.Summary,
            FontSize = 15,
            TextColor = Muted,
            LineBreakMode = LineBreakMode.WordWrap
        });

        for (var i = 0; i < topic.Steps.Count; i++)
            layout.Add(CreateGuideStep(i + 1, topic.Steps[i]));

        return CreateBorder(new ScrollView { Content = layout });
    }

    private View CreateGuideStep(int number, GuideStep step)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star }
            },
            ColumnSpacing = 14
        };

        grid.Add(new Border
        {
            WidthRequest = 34,
            HeightRequest = 34,
            StrokeThickness = 0,
            BackgroundColor = Color.FromArgb("#E2F1E8"),
            Content = new Label
            {
                Text = number.ToString(),
                TextColor = Primary,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        }, 0, 0);

        var content = new VerticalStackLayout { Spacing = 8 };
        content.Add(new Label
        {
            Text = step.Title,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Ink
        });
        content.Add(new Label
        {
            Text = step.Body,
            TextColor = Muted,
            LineBreakMode = LineBreakMode.WordWrap
        });

        foreach (var detail in step.Details)
            content.Add(new Label { Text = $"- {detail}", TextColor = Ink, FontSize = 13, LineBreakMode = LineBreakMode.WordWrap });

        grid.Add(content, 1, 0);

        return new Border
        {
            Padding = new Thickness(14),
            Stroke = Line,
            StrokeThickness = 1,
            BackgroundColor = Color.FromArgb("#F9FBFA"),
            Content = grid
        };
    }

    private View BuildEnvironment()
    {
        var layout = new VerticalStackLayout { Spacing = 18 };
        layout.Add(CreateMessage());

        var statusText = viewModel.Environment is null
            ? "Environment has not been checked yet."
            : viewModel.Environment.CanCreateProjects
                ? $"TurtlePath.Template is installed{FormatVersion(viewModel.Environment.Template.Version)}. You can create projects."
                : "TurtlePath.Template is missing or .NET template discovery failed. Install the template, then check again.";

        layout.Add(CreateDocSection("Local status", statusText));

        var actions = new HorizontalStackLayout { Spacing = 10 };
        actions.Add(CreateButton("Check environment", async () =>
        {
            await viewModel.RefreshEnvironmentAsync();
            Render();
        }, secondary: true));
        actions.Add(CreateButton("Install template", async () =>
        {
            await viewModel.InstallTemplateAsync();
            Render();
        }));
        actions.Add(CreateButton("Open templates", () => Navigate(StudioSection.Templates), secondary: true));
        layout.Add(actions);

        if (viewModel.Commands.Count > 0)
            layout.Add(CreateExecutionLog());

        return new ScrollView { Content = layout };
    }

    private static string FormatVersion(string version) => string.IsNullOrWhiteSpace(version) ? string.Empty : $" ({version})";

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
        layout.Add(CreateButton(actionText, action, secondary: true));
        return CreateBorder(layout, minHeight: 190);
    }

    private void RenderWizard()
    {
        var overlay = new Grid
        {
            BackgroundColor = Color.FromRgba(2, 12, 10, 0.72),
            Padding = new Thickness(36)
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
            BackgroundColor = Color.FromArgb("#FBFDFC")
        };

        modal.Add(BuildWizardHeader(), 0, 0);
        modal.Add(BuildWizardSteps(), 0, 1);
        modal.Add(new ScrollView { Content = BuildWizardBody() }, 0, 2);
        modal.Add(BuildWizardFooter(), 0, 3);

        var modalFrame = new Border
        {
            WidthRequest = 840,
            HeightRequest = 580,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            BackgroundColor = Color.FromArgb("#FBFDFC"),
            Stroke = Color.FromArgb("#89AA99"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(18) },
            Shadow = new Shadow
            {
                Brush = Brush.Black,
                Offset = new Point(0, 18),
                Radius = 42,
                Opacity = 0.34f
            },
            Content = modal
        };

        overlay.Add(modalFrame);
        if (viewModel.IsBusy)
            overlay.Add(BuildBusyDialog());

        modalHost.Content = overlay;
        modalHost.IsVisible = true;
    }

    private View BuildBusyDialog()
    {
        var content = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star }
            },
            ColumnSpacing = 16,
            Padding = new Thickness(24, 20),
            BackgroundColor = Colors.White
        };

        content.Add(new ActivityIndicator
        {
            IsRunning = true,
            Color = Primary,
            WidthRequest = 34,
            HeightRequest = 34,
            VerticalOptions = LayoutOptions.Center
        }, 0, 0);

        var copy = new VerticalStackLayout { Spacing = 4 };
        copy.Add(new Label
        {
            Text = "Creating project",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Ink
        });
        copy.Add(new Label
        {
            Text = "Studio is running the template commands. This can take a moment.",
            TextColor = Muted,
            LineBreakMode = LineBreakMode.WordWrap
        });
        content.Add(copy, 1, 0);

        return new Border
        {
            WidthRequest = 460,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#89AA99"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(16) },
            Shadow = new Shadow
            {
                Brush = Brush.Black,
                Offset = new Point(0, 14),
                Radius = 34,
                Opacity = 0.28f
            },
            Content = content
        };
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
            Text = viewModel.SelectedHost == ProjectHostMode.Job ? "Create one-shot job" : "Create API / Consumer service",
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
        header.Add(CreateButton("Close", () =>
        {
            viewModel.CloseWizard();
            Render();
        }, secondary: true), 1, 0);
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
        var active = viewModel.WizardStep == target;
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
        return viewModel.WizardStep switch
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

        var nameEntry = CreateEntry(viewModel.ProjectName, "BillingService");
        nameEntry.TextChanged += (_, args) => viewModel.ProjectName = args.NewTextValue;

        var pathEntry = CreateEntry(viewModel.OutputRoot, "C:\\work");
        pathEntry.HorizontalOptions = LayoutOptions.Fill;
        pathEntry.TextChanged += (_, args) => viewModel.OutputRoot = args.NewTextValue;

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
        pathRow.Add(CreateButton("Browse", async () =>
        {
            await viewModel.PickOutputDirectoryAsync();
            Render();
        }, secondary: true), 1, 0);

        layout.Add(CreateField("Project name", nameEntry));
        layout.Add(CreateField("Destination folder", pathRow));
        layout.Add(CreateMessage($"Project will be created at: {viewModel.ProjectDirectoryPreview}", error: false));
        return layout;
    }

    private View BuildOptionsStep()
    {
        var layout = new VerticalStackLayout { Spacing = 16 };
        layout.Add(CreateDocSection("Validation after creation", "Choose what Studio should run after creating the project."));
        layout.Add(CreateSwitchRow("Restore packages", "Runs package restore after template generation.", viewModel.RestoreAfterCreation, value => viewModel.RestoreAfterCreation = value));
        layout.Add(CreateSwitchRow("Build project", "Compiles the generated solution.", viewModel.BuildAfterCreation, value => viewModel.BuildAfterCreation = value));
        layout.Add(CreateSwitchRow("Run tests", "Executes the generated test project.", viewModel.TestAfterCreation, value => viewModel.TestAfterCreation = value));
        layout.Add(CreateSwitchRow("Skip guide after success", "Goes back to Templates after creating the project.", viewModel.HideGuideAfterCreation, value => viewModel.HideGuideAfterCreation = value));
        return layout;
    }

    private View BuildReviewStep()
    {
        var layout = new VerticalStackLayout { Spacing = 14 };
        layout.Add(CreateDocSection("Ready to create", "Review the project settings before executing the template command."));
        layout.Add(CreateSummaryRow("Template", viewModel.SelectedTemplateName));
        layout.Add(CreateSummaryRow("Project name", viewModel.ProjectName));
        layout.Add(CreateSummaryRow("Destination", viewModel.ProjectDirectoryPreview));
        layout.Add(CreateSummaryRow("Validation", $"{BoolText(viewModel.RestoreAfterCreation)} restore, {BoolText(viewModel.BuildAfterCreation)} build, {BoolText(viewModel.TestAfterCreation)} test"));
        layout.Add(CreateMessage());
        return layout;
    }

    private View BuildResultStep()
    {
        var layout = new VerticalStackLayout { Spacing = 14 };
        layout.Add(CreateMessage());
        layout.Add(new Label
        {
            Text = viewModel.IsCreated ? $"Created at {viewModel.CreatedDirectory}" : viewModel.IsBusy ? "Creating project..." : "No project created yet.",
            FontAttributes = FontAttributes.Bold,
            TextColor = viewModel.IsCreated ? Primary : Muted
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
            viewModel.CloseWizard();
            Navigate(StudioSection.Guides);
        }, secondary: true));

        if (viewModel.IsCreated)
            left.Add(CreateButton("Open folder", async () =>
            {
                await viewModel.OpenCreatedFolderAsync();
                Render();
            }, secondary: true));

        var right = new HorizontalStackLayout { Spacing = 10 };
        if (viewModel.WizardStep != WizardStep.Basics && !viewModel.IsBusy)
            right.Add(CreateButton("Back", () =>
            {
                viewModel.PreviousWizardStep();
                Render();
            }, secondary: true));

        right.Add(viewModel.WizardStep switch
        {
            WizardStep.Basics => CreateButton("Continue", () =>
            {
                viewModel.NextWizardStep();
                Render();
            }),
            WizardStep.Options => CreateButton("Continue", () =>
            {
                viewModel.NextWizardStep();
                Render();
            }),
            WizardStep.Review => CreateButton("Create project", async () =>
            {
                var creation = viewModel.CreateProjectAsync();
                Render();
                await creation;
                Render();
            }, disabled: viewModel.IsBusy),
            WizardStep.Result => CreateButton(viewModel.IsCreated && viewModel.HideGuideAfterCreation ? "Done" : "Open guide", () =>
            {
                viewModel.FinishWizard();
                Render();
            }),
            _ => CreateButton("Continue", () =>
            {
                viewModel.NextWizardStep();
                Render();
            })
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

        var toggle = new Switch { IsToggled = value, OnColor = Primary, ThumbColor = Colors.White };
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

    private Button CreateButton(string text, Action action, bool secondary = false, bool disabled = false)
    {
        return CreateButton(text, () =>
        {
            action();
            return Task.CompletedTask;
        }, secondary, disabled);
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
            IsEnabled = !disabled && !viewModel.IsBusy
        };

        button.Clicked += async (_, _) =>
        {
            if (viewModel.IsBusy)
                return;

            await action();
        };

        return button;
    }

    private View CreateMessage() => CreateMessage(viewModel.Message, viewModel.MessageIsError);

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
        if (viewModel.Commands.Count == 0)
            return new Label { Text = "No commands executed yet.", TextColor = Muted };

        var layout = new VerticalStackLayout { Spacing = 10 };
        foreach (CommandExecutionResult command in viewModel.Commands)
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

    private void Navigate(StudioSection section)
    {
        viewModel.Navigate(section);
        Render();
    }

    private static string BoolText(bool value) => value ? "yes" : "no";
}
