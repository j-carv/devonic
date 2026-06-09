using Devonic.CLI;
using Devonic.CLI.Commands;
using Devonic.Core.Entities;
using Spectre.Console;

var services = new ServiceLocator();

if (args.Length == 0)
    return await InteractiveSelectAsync(services);

var command = args[0].ToLowerInvariant();
var remaining = args.Length > 1 ? args[1..] : [];

return command switch
{
    "add" => await AddCommand.RunAsync(services),
    "remove" => await RemoveCommand.RunAsync(services, remaining.FirstOrDefault()),
    "edit" => await EditCommand.RunAsync(services, remaining.FirstOrDefault()),
    "list" or "ls" => await HandleListAsync(services, remaining),
    "search" when remaining.Length > 0 => await SearchCommand.RunAsync(services, string.Join(" ", remaining)),
    "search" => Error("Usage: dev search <query>"),
    "recent" => await RecentCommand.RunAsync(services),
    "favorites" or "fav" => await FavoritesCommand.RunAsync(services),
    "clone" => await CloneCommand.RunAsync(services, remaining.FirstOrDefault()),
    "config" => await ConfigCommand.RunAsync(services, remaining),
    "scan" => await ScanCommand.RunAsync(services, remaining.FirstOrDefault()),
    "doctor" => await DoctorCommand.RunAsync(services),
    "stats" => await StatsCommand.RunAsync(services),
    "--help" or "-h" or "help" => ShowHelp(),
    "--version" or "-v" => ShowVersion(),
    _ => await OpenProject(services, command, remaining)
};

static async Task<int> InteractiveSelectAsync(ServiceLocator services)
{
    var projects = await services.ProjectRepository.GetAllAsync();

    if (projects.Count == 0)
    {
        ShowHelp();
        return 0;
    }

    var choices = projects
        .OrderBy(p => p.Name)
        .Select(p =>
        {
            var label = p.IsFavorite ? $"* {p.Name}" : $"  {p.Name}";
            if (p.Tags.Count > 0) label += $" [{string.Join(", ", p.Tags)}]";
            return label;
        })
        .ToList();

    var selected = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[bold]Select a project to open:[/]")
            .PageSize(15)
            .HighlightStyle(new Style(Color.Green))
            .AddChoices(choices));

    var name = selected.TrimStart('*', ' ').Split(" [")[0];
    return await OpenCommand.RunAsync(services, name, run: false);
}

static async Task<int> HandleListAsync(ServiceLocator services, string[] remaining)
{
    string? tag = null;
    for (var i = 0; i < remaining.Length - 1; i++)
    {
        if (remaining[i] is "--tag" or "-t")
            tag = remaining[i + 1];
    }
    return await ListCommand.RunAsync(services, tag);
}

static async Task<int> OpenProject(ServiceLocator services, string name, string[] flags)
{
    var run = flags.Contains("--run");
    var shell = flags.Contains("--shell");
    Ide? ideOverride = null;

    var ideIndex = Array.IndexOf(flags, "--ide");
    if (ideIndex >= 0 && ideIndex < flags.Length - 1)
    {
        if (Enum.TryParse<Ide>(flags[ideIndex + 1], ignoreCase: true, out var ide))
            ideOverride = ide;
        else
        {
            AnsiConsole.MarkupLine($"[red]  Invalid IDE '{Markup.Escape(flags[ideIndex + 1])}'. Options: {string.Join(", ", Enum.GetNames<Ide>())}[/]");
            return 1;
        }
    }

    return await OpenCommand.RunAsync(services, name, run, ideOverride, shell);
}

static int ShowHelp()
{
    AnsiConsole.WriteLine();
    AnsiConsole.Write(new Rule("[bold blue]devonic[/]").LeftJustified());
    AnsiConsole.MarkupLine("  [dim]Open your projects fast from the terminal[/]\n");

    var table = new Table()
        .Border(TableBorder.None)
        .HideHeaders()
        .AddColumn("")
        .AddColumn("");

    table.AddRow("[green]dev[/]", "Interactive project selector");
    table.AddRow("[green]dev <project>[/]", "Open a project in its configured IDE");
    table.AddRow("[green]dev <project> --run[/]", "Open and run configured command");
    table.AddRow("[green]dev <project> --ide <ide>[/]", "Open with a different IDE");
    table.AddRow("[green]dev <project> --shell[/]", "Open a terminal in the project directory");
    table.AddRow("", "");
    table.AddRow("[yellow]dev add[/]", "Register a new project");
    table.AddRow("[yellow]dev remove[/] [dim]<name>[/]", "Remove a project");
    table.AddRow("[yellow]dev edit[/] [dim]<name>[/]", "Edit a project");
    table.AddRow("[yellow]dev list[/] [dim]--tag <tag>[/]", "List all projects (optionally filter by tag)");
    table.AddRow("", "");
    table.AddRow("[cyan]dev search <text>[/]", "Search projects by name, alias, or tag");
    table.AddRow("[cyan]dev recent[/]", "Show recently opened projects");
    table.AddRow("[cyan]dev favorites[/]", "Show favorite projects");
    table.AddRow("", "");
    table.AddRow("[magenta]dev scan[/] [dim]<dir>[/]", "Auto-detect and register projects in a directory");
    table.AddRow("[magenta]dev clone <url>[/]", "Clone a repo and register it");
    table.AddRow("", "");
    table.AddRow("[blue]dev stats[/]", "Show usage statistics");
    table.AddRow("[blue]dev doctor[/]", "Check health of all projects");
    table.AddRow("[blue]dev config[/]", "Show or set global configuration");
    table.AddRow("", "");
    table.AddRow("[dim]dev --version[/]", "Show version");
    table.AddRow("[dim]dev --help[/]", "Show this help");

    AnsiConsole.Write(table);
    AnsiConsole.WriteLine();
    return 0;
}

static int ShowVersion()
{
    AnsiConsole.MarkupLine("[bold]devonic[/] v0.2.0");
    return 0;
}

static int Error(string message)
{
    AnsiConsole.MarkupLine($"[red]  {Markup.Escape(message)}[/]");
    return 1;
}
