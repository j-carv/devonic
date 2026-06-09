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
    "doctor" or "check" => await DoctorCommand.RunAsync(services, fix: remaining.Contains("--fix")),
    "stats" => await StatsCommand.RunAsync(services),
    "cd" => await CdCommand.RunAsync(services, remaining.FirstOrDefault()),
    "init" => await InitCommand.RunAsync(services),
    "group" or "g" => await GroupCommand.RunAsync(services, remaining),
    "notes" or "note" => await HandleNotesAsync(services, remaining),
    "completions" => await CompletionsCommand.RunAsync(services, remaining.FirstOrDefault()),
    "--last" => await OpenLastAsync(services),
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
        display += $"  [dim]{p.Ide}[/]";
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
    string? sort = null;
    for (var i = 0; i < remaining.Length - 1; i++)
    {
        if (remaining[i] is "--tag" or "-t")
            tag = remaining[i + 1];
        if (remaining[i] is "--sort" or "-s")
            sort = remaining[i + 1];
    }
    return await ListCommand.RunAsync(services, tag, sort);
}

static async Task<int> HandleNotesAsync(ServiceLocator services, string[] remaining)
{
    var name = remaining.FirstOrDefault();
    var note = remaining.Length > 1 ? string.Join(" ", remaining[1..]) : null;
    return await NotesCommand.RunAsync(services, name, note);
}

static async Task<int> OpenLastAsync(ServiceLocator services)
{
    var usages = await services.UsageTracker.GetRecentAsync(1);
    if (usages.Count == 0)
    {
        AnsiConsole.MarkupLine("\n  [dim]No history yet.[/]\n");
        return 1;
    }
    return await OpenCommand.RunAsync(services, usages[0].ProjectName, run: false);
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
    AnsiConsole.WriteLine();

    WriteSection("Open", [
        ("dev", "Pick from registered projects"),
        ("dev [green]<name>[/]", "Open in configured IDE"),
        ("dev [green]<name>[/] --run", "Open + run dev command"),
        ("dev [green]<name>[/] --ide [dim]<ide>[/]", "One-off IDE override"),
        ("dev [green]<name>[/] --shell", "Terminal at project root"),
        ("dev --last", "Reopen last project"),
    ]);

    WriteSection("Manage", [
        ("dev add", "Register a project"),
        ("dev init", "Register current directory"),
        ("dev remove [dim]<name>[/]", "Unregister"),
        ("dev edit [dim]<name>[/]", "Change settings"),
        ("dev list [dim]--tag <t> --sort <s>[/]", "All projects (sort: opens, recent)"),
    ]);

    WriteSection("Find", [
        ("dev search [dim]<text>[/]", "Search by name, alias, or tag"),
        ("dev recent", "Recently opened"),
        ("dev fav", "Starred projects"),
        ("dev cd [dim]<name>[/]", "Print project path"),
    ]);

    WriteSection("Setup", [
        ("dev scan [dim]<dir>[/]", "Auto-detect and bulk-register"),
        ("dev clone [dim]<url>[/]", "Clone + register in one step"),
        ("dev group [dim]<create|open|rm>[/]", "Manage project groups"),
    ]);

    WriteSection("Info", [
        ("dev stats", "Usage stats"),
        ("dev doctor [dim]--fix[/]", "Check paths and git status"),
        ("dev config", "View or change settings"),
        ("dev notes [dim]<name> \"text\"[/]", "Per-project notes"),
        ("dev completions [dim]<shell>[/]", "Tab completion script"),
    ]);

    return 0;
}

static void WriteSection(string title, (string cmd, string desc)[] rows)
{
    AnsiConsole.MarkupLine($"  [bold]{title}[/]");
    foreach (var (cmd, desc) in rows)
        AnsiConsole.MarkupLine($"    {cmd,-38} [dim]{desc}[/]");
    AnsiConsole.WriteLine();
}

static int ShowVersion()
{
    AnsiConsole.MarkupLine("[bold blue]devonic[/] [dim]v0.3.0[/]");
    return 0;
}

static int Fail(string message)
{
    AnsiConsole.MarkupLine($"[red]  {Markup.Escape(message)}[/]");
    return 1;
}
