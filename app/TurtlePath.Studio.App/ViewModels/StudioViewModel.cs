using System.Text.Json;
using Microsoft.Maui.ApplicationModel;
using TurtlePath.Studio.Abstractions.Commands;
using TurtlePath.Studio.Abstractions.Projects;
using TurtlePath.Studio.Abstractions.Validation;
using TurtlePath.Studio.App.Guides;
using TurtlePath.Studio.Abstractions.Workspace;
using TurtlePath.Studio.App.Settings;
using TurtlePath.Studio.App.Updates;
using TurtlePath.Studio.Application.Defaults;
using TurtlePath.Studio.Application.Environment;
using TurtlePath.Studio.Application.UseCases;

namespace TurtlePath.Studio.App.ViewModels;

public sealed class StudioViewModel
{
    private const string CacheDirectoryName = "TurtlePath";
    private const string StudioDirectoryName = "Studio";
    private const string TemplateEnvironmentCacheFileName = "template-environment-cache.json";

    private static readonly JsonSerializerOptions CacheJsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly InspectStudioEnvironmentUseCase inspectEnvironment;
    private readonly InstallTemplateUseCase installTemplate;
    private readonly CreateTurtlePathProjectUseCase createProject;
    private readonly IStudioWorkspaceService workspace;
    private readonly IStudioSettingsStore settingsStore;
    private readonly IStudioGuideProvider guideProvider;
    private readonly IStudioUpdater studioUpdater;

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
    public string UpdateManifestUrl { get; set; } = string.Empty;
    public string UpdateChannel { get; set; } = string.Empty;
    public bool CheckUpdatesOnStartup { get; set; }
    public bool IsBusy { get; private set; }
    public string BusyTitle { get; private set; } = "Working";
    public string BusyMessage { get; private set; } = "Studio is running a command. This can take a moment.";
    public bool IsWizardOpen { get; private set; }
    public bool IsTemplateUpdatePromptOpen { get; private set; }
    public bool IsCommandOutputOpen { get; private set; }
    public bool IsStatusMessageOpen { get; private set; }
    public bool IsCreated { get; private set; }
    public string? CreatedDirectory { get; private set; }
    public string Message { get; private set; } = "Ready.";
    public bool MessageIsError { get; private set; }
    public bool MessageIsWarning { get; private set; }
    public IReadOnlyList<CommandExecutionResult> Commands { get; private set; } = [];
    public StudioUpdateCheckResult? StudioUpdate { get; private set; }
    public IReadOnlyList<StudioGuideOption> GuideOptions { get; private set; } = [];
    public IReadOnlyList<StudioTemplateGuideOption> TemplateGuideOptions { get; private set; } = [];
    public StudioTemplateGuideOption? SelectedTemplateGuideOption { get; private set; }
    public StudioGuideOption? SelectedGuide { get; private set; }
    public StudioGuideCulture? SelectedGuideCulture { get; private set; }
    public StudioGuideDocument? CurrentGuide { get; private set; }
    public string GuideStatus { get; private set; } = "Guide not loaded yet.";
    public string SelectedTemplateGuideText => SelectedTemplateGuideOption is null
        ? "Select template version"
        : $"Template {SelectedTemplateGuideOption.TemplateVersion}";

    public string SelectedDocumentationGuideText => SelectedTemplateGuideOption is null
        ? "Documentation not loaded yet."
        : $"Guide docs {SelectedTemplateGuideOption.Guide.DocumentationVersion} for template {SelectedTemplateGuideOption.TemplateVersion}.";
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
        : TemplateEnvironments.Any(environment => environment.TemplateRequiresUpdate)
            ? SidebarCollapsed ? "!" : "Environment: update available"
            : TemplatesCanCreateProjects
                ? SidebarCollapsed ? "OK" : "Environment: ready"
                : SidebarCollapsed ? "!" : "Environment: needs attention";

    public string SelectedTemplateName { get; private set; } = "API / Consumer";

    public string ProjectDirectoryPreview => string.IsNullOrWhiteSpace(ProjectName) || string.IsNullOrWhiteSpace(OutputRoot)
        ? "Complete the project name and destination folder"
        : Path.Combine(OutputRoot, ProjectName);

    public string StudioVersion => $"v{StudioUpdater.GetCurrentVersion()}";
    public string StudioUpdateText => StudioUpdate is null
        ? "Studio updates have not been checked yet."
        : StudioUpdate.Message;

    public string TemplateActionText => TemplateEnvironments.Any(environment => environment.TemplateRequiresUpdate)
        ? "Update templates"
        : TemplateEnvironments.Count > 0 && TemplateEnvironments.All(environment => environment.Template.IsInstalled)
            ? "Repair templates"
            : "Install templates";

    public bool TemplateIsCurrent => TemplatesCanCreateProjects && !TemplateEnvironments.Any(environment => environment.TemplateRequiresUpdate);

    public bool TemplatesCanCreateProjects => TemplateEnvironments.Count > 0
        && TemplateEnvironments.All(environment => environment.CanCreateProjects);

    public bool ShouldPromptTemplateUpdate => TemplatesCanCreateProjects
        && TemplateEnvironments.Any(environment => environment.TemplateRequiresUpdate);

    public string TemplateUpdatePromptMessage
    {
        get
        {
            var updates = TemplateEnvironments
                .Where(environment => environment.TemplateRequiresUpdate)
                .Select(environment => $"{environment.Template.PackageId}: installed {environment.Template.Version}, latest {environment.Template.LatestVersion}")
                .ToArray();

            return updates.Length == 0
                ? "A newer template version is available."
                : string.Join(global::System.Environment.NewLine, updates);
        }
    }

    public StudioViewModel(
        InspectStudioEnvironmentUseCase inspectEnvironment,
        InstallTemplateUseCase installTemplate,
        CreateTurtlePathProjectUseCase createProject,
        IStudioWorkspaceService workspace,
        IStudioSettingsStore settingsStore,
        IStudioGuideProvider guideProvider,
        IStudioUpdater studioUpdater)
    {
        this.inspectEnvironment = inspectEnvironment;
        this.installTemplate = installTemplate;
        this.createProject = createProject;
        this.workspace = workspace;
        this.settingsStore = settingsStore;
        this.guideProvider = guideProvider;
        this.studioUpdater = studioUpdater;

        var settings = settingsStore.Load();
        ApplySettings(settings);
        ProjectName = settings.ProjectNamePlaceholder;
        OutputRoot = settings.DefaultOutputRoot;
        RestoreAfterCreation = settings.RestoreAfterCreation;
        BuildAfterCreation = settings.BuildAfterCreation;
        TestAfterCreation = settings.TestAfterCreation;
        HideGuideAfterCreation = settings.HideGuideAfterCreation;

        TemplateEnvironments = LoadCachedTemplateEnvironments();
        Environment = TemplateEnvironments.FirstOrDefault(environment =>
            environment.Template.PackageId == TurtlePathStudioDefaults.TemplatePackageId);
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

    public void CloseTemplateUpdatePrompt() => IsTemplateUpdatePromptOpen = false;

    public void CloseStatusMessage() => IsStatusMessageOpen = false;

    public void OpenCommandOutput()
    {
        if (Commands.Count > 0)
            IsCommandOutputOpen = true;
    }

    public void CloseCommandOutput() => IsCommandOutputOpen = false;

    public async Task LoadGuidesAsync(bool forceRefresh = false)
    {
        var templateVersion = Environment?.Template.Version;
        if (string.IsNullOrWhiteSpace(templateVersion))
        {
            TemplateEnvironments = await InspectTemplateEnvironmentsAsync();
            Environment = TemplateEnvironments.FirstOrDefault(environment =>
                environment.Template.PackageId == TurtlePathStudioDefaults.TemplatePackageId);
            templateVersion = Environment?.Template.Version ?? string.Empty;
        }

        GuideOptions = await guideProvider.GetGuidesAsync(
            TurtlePathStudioDefaults.TemplatePackageId,
            string.Empty);
        TemplateGuideOptions = BuildTemplateGuideOptions(GuideOptions, templateVersion);

        if (TemplateGuideOptions.Count == 0)
        {
            GuideStatus = "No versioned guides match the installed template. Using embedded fallback.";
            return;
        }

        SelectedTemplateGuideOption = SelectTemplateGuideOption(templateVersion);
        var selectedGuide = SelectedTemplateGuideOption.Guide;
        SelectedGuide = selectedGuide;
        SelectedGuideCulture ??= selectedGuide.Cultures.FirstOrDefault(culture => string.Equals(culture.Code, "en", StringComparison.OrdinalIgnoreCase))
            ?? selectedGuide.Cultures.FirstOrDefault();

        if (SelectedGuideCulture is null)
        {
            GuideStatus = "Selected guide has no available cultures.";
            return;
        }

        CurrentGuide = await guideProvider.GetGuideAsync(selectedGuide, SelectedGuideCulture, forceRefresh);
        GuideStatus = CurrentGuide.Status;
    }

    public Task SelectTemplateGuideAsync(StudioTemplateGuideOption option)
    {
        var previousGuideId = CurrentGuide?.Guide.Id;
        var previousCultureCode = CurrentGuide?.Culture.Code;

        SelectedTemplateGuideOption = option;
        SelectedGuide = option.Guide;
        SelectedGuideCulture = option.Guide.Cultures.FirstOrDefault(culture => string.Equals(culture.Code, SelectedGuideCulture?.Code, StringComparison.OrdinalIgnoreCase))
            ?? option.Guide.Cultures.FirstOrDefault();

        if (CurrentGuide is null ||
            !string.Equals(previousGuideId, SelectedGuide.Id, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(previousCultureCode, SelectedGuideCulture?.Code, StringComparison.OrdinalIgnoreCase))
        {
            CurrentGuide = null;
            GuideStatus = "Loading selected guide...";
        }

        return Task.CompletedTask;
    }

    public Task SelectGuideCultureAsync(StudioGuideCulture culture)
    {
        if (string.Equals(SelectedGuideCulture?.Code, culture.Code, StringComparison.OrdinalIgnoreCase))
            return Task.CompletedTask;

        SelectedGuideCulture = culture;
        CurrentGuide = null;
        GuideStatus = "Loading selected guide...";

        return Task.CompletedTask;
    }

    public static string FormatGuideOption(StudioTemplateGuideOption option)
    {
        return $"Template {option.TemplateVersion} - guide docs {option.Guide.DocumentationVersion}";
    }

    public static string FormatTemplateRange(string range)
    {
        if (string.IsNullOrWhiteSpace(range))
            return "All template versions";

        var normalized = range.Trim();
        if (normalized.StartsWith('[') && normalized.EndsWith(')'))
        {
            var parts = normalized[1..^1].Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length == 2)
                return $"Template {parts[0]} - before {parts[1]}";
        }

        if (normalized.StartsWith('[') && normalized.EndsWith(']'))
        {
            var parts = normalized[1..^1].Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length == 2)
                return $"Template {parts[0]} - {parts[1]}";
        }

        return $"Template {normalized}";
    }

    private IReadOnlyList<StudioTemplateGuideOption> BuildTemplateGuideOptions(
        IReadOnlyList<StudioGuideOption> guides,
        string installedTemplateVersion)
    {
        var options = guides
            .SelectMany(guide => guide.SupportedTemplateVersions.Select(version => new StudioTemplateGuideOption(version, guide)))
            .ToList();

        if (!string.IsNullOrWhiteSpace(installedTemplateVersion) &&
            !options.Any(option => string.Equals(option.TemplateVersion, installedTemplateVersion, StringComparison.OrdinalIgnoreCase)))
        {
            var guide = guides.FirstOrDefault(candidate => IsVersionInRange(candidate.SupportedTemplateVersionRange, installedTemplateVersion));
            if (guide is not null)
                options.Add(new StudioTemplateGuideOption(installedTemplateVersion, guide));
        }

        return options
            .DistinctBy(option => option.TemplateVersion)
            .OrderByDescending(option => Version.TryParse(option.TemplateVersion, out var parsed) ? parsed : new Version(0, 0))
            .ToArray();
    }

    private StudioTemplateGuideOption SelectTemplateGuideOption(string installedTemplateVersion)
    {
        if (SelectedTemplateGuideOption is not null &&
            TemplateGuideOptions.Any(option => option.TemplateVersion == SelectedTemplateGuideOption.TemplateVersion))
        {
            return TemplateGuideOptions.First(option => option.TemplateVersion == SelectedTemplateGuideOption.TemplateVersion);
        }

        if (!string.IsNullOrWhiteSpace(installedTemplateVersion))
        {
            var installed = TemplateGuideOptions.FirstOrDefault(option =>
                string.Equals(option.TemplateVersion, installedTemplateVersion, StringComparison.OrdinalIgnoreCase));
            if (installed is not null)
                return installed;
        }

        return TemplateGuideOptions[0];
    }

    private static bool IsVersionInRange(string range, string version)
    {
        if (string.IsNullOrWhiteSpace(version) || !Version.TryParse(NormalizeVersion(version), out var parsed))
            return true;

        if (string.IsNullOrWhiteSpace(range) || range.Length < 5)
            return true;

        var includeMin = range[0] == '[';
        var includeMax = range[^1] == ']';
        var parts = range[1..^1].Split(',', StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
            return true;

        return IsLowerBoundValid(parsed, parts[0], includeMin) &&
               IsUpperBoundValid(parsed, parts[1], includeMax);
    }

    private static bool IsLowerBoundValid(Version version, string minimum, bool inclusive)
    {
        if (string.IsNullOrWhiteSpace(minimum) || !Version.TryParse(NormalizeVersion(minimum), out var parsed))
            return true;

        var comparison = version.CompareTo(parsed);
        return inclusive ? comparison >= 0 : comparison > 0;
    }

    private static bool IsUpperBoundValid(Version version, string maximum, bool inclusive)
    {
        if (string.IsNullOrWhiteSpace(maximum) || !Version.TryParse(NormalizeVersion(maximum), out var parsed))
            return true;

        var comparison = version.CompareTo(parsed);
        return inclusive ? comparison <= 0 : comparison < 0;
    }

    private static string NormalizeVersion(string version)
    {
        var normalized = version.Trim();
        if (normalized.StartsWith('v'))
            normalized = normalized[1..];

        var metadataIndex = normalized.IndexOf('+', StringComparison.Ordinal);
        return metadataIndex >= 0 ? normalized[..metadataIndex] : normalized;
    }

    public Task SyncGuideDocumentationAsync()
    {
        return RunAsync("Syncing documentation", "Studio is downloading the latest available guide content for the selected documentation version.", async () =>
        {
            await LoadGuidesAsync(forceRefresh: true);

            Message = CurrentGuide?.LoadedFromCache == false && CurrentGuide.IsEmbeddedFallback == false
                ? "Documentation synced from GitHub."
                : "Documentation is available locally. GitHub could not be reached, so Studio kept the local guide.";
            MessageIsError = false;
            MessageIsWarning = CurrentGuide?.LoadedFromCache != false || CurrentGuide.IsEmbeddedFallback;
            IsStatusMessageOpen = true;
        });
    }

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
            Commands = [];
            IsCommandOutputOpen = false;
            TemplateEnvironments = await InspectTemplateEnvironmentsAsync(forceRefresh: true);
            Environment = TemplateEnvironments.FirstOrDefault(environment =>
                environment.Template.PackageId == TurtlePathStudioDefaults.TemplatePackageId);

            if (!TemplatesCanCreateProjects)
            {
                Message = "One or more templates are missing. Install templates before creating projects.";
                MessageIsError = true;
                MessageIsWarning = false;
                IsStatusMessageOpen = true;
                return;
            }

            if (ShouldPromptTemplateUpdate)
            {
                Message = "Template update recommended. You can create projects now, but a newer template version is available.";
                MessageIsError = false;
                MessageIsWarning = true;
                IsTemplateUpdatePromptOpen = true;
                return;
            }

            Message = "Environment ready.";
            MessageIsError = false;
            MessageIsWarning = false;
            IsStatusMessageOpen = true;
        });
    }

    public Task InstallTemplateAsync()
    {
        return RunAsync("Installing templates", "Studio is installing or updating the TurtlePath template packages.", async () =>
        {
            TemplateEnvironments = await InspectTemplateEnvironmentsAsync();
            Environment = TemplateEnvironments.FirstOrDefault(environment =>
                environment.Template.PackageId == TurtlePathStudioDefaults.TemplatePackageId);

            if (TemplateEnvironments.Count > 0 &&
                TemplateEnvironments.All(environment => environment.Template.IsInstalled) &&
                TemplateEnvironments.All(environment => !environment.TemplateRequiresUpdate))
            {
                Commands = [];
                Message = "All TurtlePath templates are up to date. No update is needed.";
                MessageIsError = false;
                MessageIsWarning = false;
                IsStatusMessageOpen = true;
                return;
            }

            var commands = new List<CommandExecutionResult>();
            foreach (var packageId in TurtlePathStudioDefaults.TemplatePackageIds)
                commands.Add(await installTemplate.ExecuteAsync(packageId, forceUpdate: true));

            Commands = commands;
            TemplateEnvironments = await InspectTemplateEnvironmentsAsync(forceRefresh: true);
            Environment = TemplateEnvironments.FirstOrDefault(environment =>
                environment.Template.PackageId == TurtlePathStudioDefaults.TemplatePackageId);

            Message = commands.All(command => command.Succeeded)
                ? "Templates installed. Environment was checked again."
                : "Template installation failed. Check the command output.";
            MessageIsError = !commands.All(command => command.Succeeded);
            MessageIsWarning = false;
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
            DefaultHideGuideAfterCreation,
            UpdateManifestUrl.Trim(),
            UpdateChannel.Trim(),
            CheckUpdatesOnStartup);

        settingsStore.Save(settings);
        ApplySettings(settings);
        Message = "Default values saved.";
        MessageIsError = false;
        MessageIsWarning = false;
    }

    public void ResetDefaults()
    {
        settingsStore.Reset();
        ApplySettings(settingsStore.Load());
        Message = "Default values restored.";
        MessageIsError = false;
        MessageIsWarning = false;
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
            var selectedTemplate = await GetTemplateEnvironmentAsync(SelectedTemplatePackageId);
            if (!selectedTemplate.CanCreateProjects)
            {
                var install = await installTemplate.ExecuteAsync(SelectedTemplatePackageId, forceUpdate: true);
                Commands = [install];

                selectedTemplate = await GetTemplateEnvironmentAsync(SelectedTemplatePackageId, forceRefresh: true);
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
                MessageIsWarning = true;
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
            MessageIsWarning = result.Succeeded && selectedTemplate.TemplateRequiresUpdate;
        });
    }

    public async Task<bool> PrepareCreateProjectAsync()
    {
        if (!ValidateInput())
        {
            WizardStep = WizardStep.Basics;
            return false;
        }

        var canCreate = true;
        await RunAsync("Checking template version", "Studio is checking the installed template before creating the project.", async () =>
        {
            var selectedTemplate = await GetTemplateEnvironmentAsync(SelectedTemplatePackageId);
            if (SelectedTemplatePackageId == TurtlePathStudioDefaults.TemplatePackageId)
                Environment = selectedTemplate;

            if (!selectedTemplate.TemplateRequiresUpdate)
                return;

            TemplateEnvironments = UpdateTemplateEnvironment(selectedTemplate);
            Message = BuildTemplateUpdateSuggestionMessage(selectedTemplate);
            MessageIsError = false;
            MessageIsWarning = true;
            IsTemplateUpdatePromptOpen = true;
            canCreate = false;
        });

        return canCreate;
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

    private async Task<IReadOnlyList<StudioEnvironmentReport>> InspectTemplateEnvironmentsAsync(bool forceRefresh = false)
    {
        if (!forceRefresh)
        {
            var cached = LoadCachedTemplateEnvironments();
            if (cached.Count > 0)
                return cached;
        }

        var environments = new List<StudioEnvironmentReport>();
        foreach (var packageId in TurtlePathStudioDefaults.TemplatePackageIds)
            environments.Add(await inspectEnvironment.ExecuteAsync(packageId));

        SaveCachedTemplateEnvironments(environments);
        return environments;
    }

    private async Task<StudioEnvironmentReport> GetTemplateEnvironmentAsync(
        string packageId,
        bool forceRefresh = false)
    {
        var environments = await InspectTemplateEnvironmentsAsync(forceRefresh);
        var environment = environments.FirstOrDefault(candidate =>
            candidate.Template.PackageId == packageId);

        if (environment is not null)
            return environment;

        environment = await inspectEnvironment.ExecuteAsync(packageId);
        TemplateEnvironments = UpdateTemplateEnvironment(environment);
        SaveCachedTemplateEnvironments(TemplateEnvironments);
        return environment;
    }

    private IReadOnlyList<StudioEnvironmentReport> UpdateTemplateEnvironment(StudioEnvironmentReport selectedTemplate)
    {
        if (TemplateEnvironments.Count == 0)
            return [selectedTemplate];

        return TemplateEnvironments
            .Select(environment => environment.Template.PackageId == selectedTemplate.Template.PackageId
                ? selectedTemplate
                : environment)
            .ToArray();
    }

    private static IReadOnlyList<StudioEnvironmentReport> LoadCachedTemplateEnvironments()
    {
        var path = GetTemplateEnvironmentCachePath();
        if (!File.Exists(path))
            return [];

        try
        {
            var cache = JsonSerializer.Deserialize<CachedTemplateEnvironment>(
                File.ReadAllText(path),
                CacheJsonOptions);

            if (cache is null)
                return [];

            return cache.Environments;
        }
        catch
        {
            return [];
        }
    }

    private static void SaveCachedTemplateEnvironments(IReadOnlyList<StudioEnvironmentReport> environments)
    {
        try
        {
            var path = GetTemplateEnvironmentCachePath();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var cache = new CachedTemplateEnvironment(DateTimeOffset.UtcNow, environments.ToArray());
            File.WriteAllText(path, JsonSerializer.Serialize(cache, CacheJsonOptions));
        }
        catch
        {
            // Cache is an optimization only. The Studio can always inspect the environment again.
        }
    }

    private static string GetTemplateEnvironmentCachePath()
    {
        return Path.Combine(
            global::System.Environment.GetFolderPath(global::System.Environment.SpecialFolder.LocalApplicationData),
            CacheDirectoryName,
            StudioDirectoryName,
            TemplateEnvironmentCacheFileName);
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
        MessageIsWarning = false;
        IsStatusMessageOpen = true;
    }

    public Task CheckStudioUpdateAsync()
    {
        return RunAsync("Checking Studio updates", "Studio is checking the configured update manifest.", async () =>
        {
            StudioUpdate = await studioUpdater.CheckForUpdatesAsync(UpdateManifestUrl, UpdateChannel);
            Message = StudioUpdate.Message;
            MessageIsError = !StudioUpdate.Succeeded;
            MessageIsWarning = StudioUpdate.Succeeded && StudioUpdate.IsAvailable;
            IsStatusMessageOpen = true;
        });
    }

    public async Task CheckStudioUpdateQuietlyAsync()
    {
        try
        {
            StudioUpdate = await studioUpdater.CheckForUpdatesAsync(UpdateManifestUrl, UpdateChannel);
            if (!StudioUpdate.IsAvailable)
                return;

            Message = StudioUpdate.Message;
            MessageIsError = false;
            MessageIsWarning = true;
        }
        catch
        {
            StudioUpdate = null;
        }
    }

    public async Task CheckTemplateUpdatesQuietlyAsync()
    {
        try
        {
            TemplateEnvironments = LoadCachedTemplateEnvironments();
            Environment = TemplateEnvironments.FirstOrDefault(environment =>
                environment.Template.PackageId == TurtlePathStudioDefaults.TemplatePackageId);

            if (ShouldPromptTemplateUpdate)
            {
                Message = "Template update recommended. You can create projects now, but a newer template version is available.";
                MessageIsError = false;
                MessageIsWarning = true;
                IsTemplateUpdatePromptOpen = true;
                return;
            }

            TemplateEnvironments = await InspectTemplateEnvironmentsAsync(forceRefresh: true);
            Environment = TemplateEnvironments.FirstOrDefault(environment =>
                environment.Template.PackageId == TurtlePathStudioDefaults.TemplatePackageId);

            if (!ShouldPromptTemplateUpdate)
                return;

            Message = "Template update recommended. You can create projects now, but a newer template version is available.";
            MessageIsError = false;
            MessageIsWarning = true;
            IsTemplateUpdatePromptOpen = true;
        }
        catch
        {
            TemplateEnvironments = LoadCachedTemplateEnvironments();
            Environment = TemplateEnvironments.FirstOrDefault(environment =>
                environment.Template.PackageId == TurtlePathStudioDefaults.TemplatePackageId);
        }
    }

    public Task InstallStudioUpdateAsync()
    {
        return RunAsync("Installing Studio update", "Studio is downloading and preparing the update. The app will restart when the updater takes over.", async () =>
        {
            if (StudioUpdate is null || !StudioUpdate.IsAvailable)
                StudioUpdate = await studioUpdater.CheckForUpdatesAsync(UpdateManifestUrl, UpdateChannel);

            if (!StudioUpdate.IsAvailable)
            {
                Message = StudioUpdate.Succeeded
                    ? "TurtlePath Studio is up to date. No update is needed."
                    : StudioUpdate.Message;
                MessageIsError = !StudioUpdate.Succeeded;
                MessageIsWarning = false;
                IsStatusMessageOpen = true;
                return;
            }

            await studioUpdater.StartUpdateAsync(StudioUpdate);
        });
    }

    public void RestoreDefaultUpdateSource()
    {
        UpdateManifestUrl = PreferencesStudioSettingsStore.DefaultUpdateManifestUrl;
        UpdateChannel = PreferencesStudioSettingsStore.DefaultUpdateChannel;
        CheckUpdatesOnStartup = true;
        StudioUpdate = null;
        Message = "Studio update source restored.";
        MessageIsError = false;
        MessageIsWarning = false;
        IsStatusMessageOpen = true;
    }

    private void ClearMessage()
    {
        Message = "Ready.";
        MessageIsError = false;
        MessageIsWarning = false;
    }

    private void ApplySettings(StudioSettings settings)
    {
        DefaultOutputRoot = settings.DefaultOutputRoot;
        ProjectNamePlaceholder = settings.ProjectNamePlaceholder;
        DefaultRestoreAfterCreation = settings.RestoreAfterCreation;
        DefaultBuildAfterCreation = settings.BuildAfterCreation;
        DefaultTestAfterCreation = settings.TestAfterCreation;
        DefaultHideGuideAfterCreation = settings.HideGuideAfterCreation;
        UpdateManifestUrl = settings.UpdateManifestUrl;
        UpdateChannel = settings.UpdateChannel;
        CheckUpdatesOnStartup = settings.CheckUpdatesOnStartup;
    }

    private static string BuildTemplateUpdateSuggestionMessage(StudioEnvironmentReport environment)
    {
        return environment.Template.HasLatestVersion
            ? $"{environment.Template.PackageId} has a newer version available. Installed: {environment.Template.Version}; latest: {environment.Template.LatestVersion}."
            : $"{environment.Template.PackageId} is installed, but Studio could not verify the latest NuGet version.";
    }

    private sealed record CachedTemplateEnvironment(
        DateTimeOffset CachedAt,
        StudioEnvironmentReport[] Environments);
}
