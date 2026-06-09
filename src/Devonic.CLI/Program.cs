using Devonic.CLI;
using Devonic.CLI.Commands;
using Spectre.Console;

var services = new ServiceLocator();

if (args.Length == 0)
{
    ShowHelp();
    return 0;
}

var command = args[0].ToLowerInvariant();
var remaining = args.Length > 1 ? args[1..] : [];

return command switch
{
    "add" => await AddCommand.RunAsync(services),
    "remove" => await RemoveCommand.RunAsync(services, remaining.FirstOrDefault()),
    "edit" => await EditCommand.RunAsync(services, remaining.FirstOrDefault()),
    "list" or "ls" => await ListCommand.RunAsync(services),
    "search" when remaining.Length > 0 => await SearchCommand.RunAsync(services, string.Join(" ", remaining)),
    "search" => Error("Usage: dev search <query>"),
    "recent" => await RecentCommand.RunAsync(services),
    "favorites" or "fav" => await FavoritesCommand.RunAsync(services),
    "clone" => await CloneCommand.RunAsync(services, remaining.FirstOrDefault()),
    "config" => await ConfigCommand.RunAsync(services, remaining),
    "--help" or "-h" or "help" => ShowHelp(),
    "--version" or "-v" => ShowVersion(),
    _ => await OpenProject(services, command, remaining)
};

static async Task<int> OpenProject(ServiceLocator services, string name, string[] flags)
{
    var run = flags.Contains("--run");
    return await OpenCommand.RunAsync(services, name, run);
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

    table.AddRow("[green]dev <project>[/]", "Open a project in its configured IDE");
    table.AddRow("[green]dev <project> --run[/]", "Open project and run its configured command");
    table.AddRow("", "");
    table.AddRow("[yellow]dev add[/]", "Register a new project");
    table.AddRow("[yellow]dev remove[/] [dim]<name>[/]", "Remove a project");
    table.AddRow("[yellow]dev edit[/] [dim]<name>[/]", "Edit a project");
    table.AddRow("[yellow]dev list[/]", "List all projects");
    table.AddRow("", "");
    table.AddRow("[cyan]dev search <text>[/]", "Search projects by name");
    table.AddRow("[cyan]dev recent[/]", "Show recently opened projects");
    table.AddRow("[cyan]dev favorites[/]", "Show favorite projects");
    table.AddRow("", "");
    table.AddRow("[magenta]dev clone <url>[/]", "Clone a repo and register it");
    table.AddRow("[magenta]dev config[/]", "Show or set global configuration");
    table.AddRow("", "");
    table.AddRow("[dim]dev --version[/]", "Show version");
    table.AddRow("[dim]dev --help[/]", "Show this help");

    AnsiConsole.Write(table);
    AnsiConsole.WriteLine();
    return 0;
}

static int ShowVersion()
{
    AnsiConsole.MarkupLine("[bold]devonic[/] v0.1.2");
    return 0;
}

static int Error(string message)
{
    AnsiConsole.MarkupLine($"[red]  {Markup.Escape(message)}[/]");
    return 1;
}
