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
    "remove" or "rm" => await RemoveCommand.RunAsync(services, remaining.FirstOrDefault()),
    "edit" => await EditCommand.RunAsync(services, remaining.FirstOrDefault()),
    "list" or "ls" => await HandleListAsync(services, remaining),
    "search" or "s" when remaining.Length > 0 => await SearchCommand.RunAsync(services, string.Join(" ", remaining)),
    "search" or "s" => Fail("Usage: dev search <query>"),
    "recent" => await RecentCommand.RunAsync(services),
    "favorites" or "fav" => await FavoritesCommand.RunAsync(services),
    "clone" => await CloneCommand.RunAsync(services, remaining.FirstOrDefault()),
    "config" => await ConfigCommand.RunAsync(services, remaining),
    "scan" => await ScanCommand.RunAsync(services, remaining.FirstOrDefault()),
    "doctor" or "check" => await DoctorCommand.RunAsync(services),
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
        AnsiConsole.MarkupLine("\n  [dim]No projects yet.[/] Run [green]dev add[/] to register your first project.\n");
        return 0;
    }

    var lookup = new Dictionary<string, string>();
    foreach (var p in projects.OrderByDescending(p => p.IsFavorite).ThenBy(p => p.Name))
    {
        var display = p.IsFavorite ? $"[yellow]*[/] {p.Name}" : $"  {p.Name}";
        if (p.Alias is not null) display += $" [dim]({p.Alias})[/]";
        if (p.Tags.Count > 0) display += $" [dim]{string.Join(" ", p.Tags.Select(t => $"#{t}"))}[/]";
        display += $"  [dim]| {p.Ide}[/]";
        lookup[display] = p.Name;
    }

    var selected = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("\n  [bold]Pick a project[/]")
            .PageSize(15)
            .HighlightStyle(new Style(Color.Green))
            .AddChoices(lookup.Keys));

    return await OpenCommand.RunAsync(services, lookup[selected], run: false);
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
            AnsiConsole.MarkupLine($"[red]  Unknown IDE '{Markup.Escape(flags[ideIndex + 1])}'.[/] Available: {string.Join(", ", Enum.GetNames<Ide>())}");
            return 1;
        }
    }

    return await OpenCommand.RunAsync(services, name, run, ideOverride, shell);
}

static int ShowHelp()
{
    AnsiConsole.WriteLine();
    AnsiConsole.Write(new FigletText("devonic").Color(Color.Blue).LeftJustified());
    AnsiConsole.MarkupLine("  [dim]Your projects, one command away.[/]\n");

    WriteSection("Launch", [
        ("dev", "Interactive project picker"),
        ("dev [green]<project>[/]", "Open in configured IDE"),
        ("dev [green]<project>[/] --run", "Open IDE + run dev server"),
        ("dev [green]<project>[/] --ide [dim]<ide>[/]", "Override IDE for this launch"),
        ("dev [green]<project>[/] --shell", "Open terminal in project dir"),
    ]);

    WriteSection("Manage", [
        ("dev add", "Register a new project"),
        ("dev remove [dim]<name>[/]", "Unregister a project"),
        ("dev edit [dim]<name>[/]", "Update project settings"),
        ("dev list [dim]--tag <tag>[/]", "Show all projects"),
    ]);

    WriteSection("Discover", [
        ("dev search [dim]<text>[/]", "Find by name, alias, or tag"),
        ("dev recent", "Last opened projects"),
        ("dev fav", "Your starred projects"),
    ]);

    WriteSection("Automate", [
        ("dev scan [dim]<dir>[/]", "Detect & bulk-register projects"),
        ("dev clone [dim]<url>[/]", "Git clone + auto-register"),
    ]);

    WriteSection("Maintain", [
        ("dev stats", "Usage dashboard"),
        ("dev doctor", "Health check all projects"),
        ("dev config", "Global settings"),
    ]);

    return 0;
}

static void WriteSection(string title, (string cmd, string desc)[] rows)
{
    AnsiConsole.MarkupLine($"  [bold]{title}[/]");
    foreach (var (cmd, desc) in rows)
        AnsiConsole.MarkupLine($"    {cmd,-34} [dim]{desc}[/]");
    AnsiConsole.WriteLine();
}

static int ShowVersion()
{
    AnsiConsole.MarkupLine("[bold blue]devonic[/] [dim]v0.2.1[/]");
    return 0;
}

static int Fail(string message)
{
    AnsiConsole.MarkupLine($"[red]  {Markup.Escape(message)}[/]");
    return 1;
}
