using Devonic.Core.Entities;
using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class ConfigCommand
{
    public static async Task<int> RunAsync(ServiceLocator services, string[] args)
    {
        if (args.Length == 0)
            return await ShowAsync(services);

        return args[0].ToLowerInvariant() switch
        {
            "show" => await ShowAsync(services),
            "set" when args.Length >= 3 => await SetAsync(services, args[1], args[2]),
            "set" => ShowSetUsage(),
            _ => ShowUsage()
        };
    }

    private static async Task<int> ShowAsync(ServiceLocator services)
    {
        var config = await services.ConfigRepository.GetAsync();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]Setting[/]")
            .AddColumn("[bold]Value[/]");

        table.AddRow("Default IDE", config.DefaultIde?.ToString() ?? "[dim](not set)[/]");
        table.AddRow("Default projects path", Markup.Escape(config.DefaultProjectsPath ?? "(not set)"));

        foreach (var (ide, path) in config.IdePaths)
            table.AddRow($"IDE path: {ide}", Markup.Escape(path));

        if (config.IdePaths.Count == 0)
            table.AddRow("IDE paths", "[dim](using defaults)[/]");

        AnsiConsole.WriteLine();
        AnsiConsole.Write(table);
        return 0;
    }

    private static async Task<int> SetAsync(ServiceLocator services, string key, string value)
    {
        var config = await services.ConfigRepository.GetAsync();

        var idePaths = new Dictionary<Ide, string>(config.IdePaths);
        Ide? defaultIde = config.DefaultIde;
        string? defaultProjectsPath = config.DefaultProjectsPath;

        switch (key.ToLowerInvariant())
        {
            case "defaultide":
                if (!Enum.TryParse<Ide>(value, ignoreCase: true, out var ide))
                {
                    AnsiConsole.MarkupLine($"[red]  Invalid IDE. Options: {string.Join(", ", Enum.GetNames<Ide>())}[/]");
                    return 1;
                }
                defaultIde = ide;
                break;

            case "defaultpath":
                defaultProjectsPath = value;
                break;

            default:
                if (key.StartsWith("idepath.", StringComparison.OrdinalIgnoreCase))
                {
                    var ideName = key["idepath.".Length..];
                    if (!Enum.TryParse<Ide>(ideName, ignoreCase: true, out var targetIde))
                    {
                        AnsiConsole.MarkupLine($"[red]  Invalid IDE. Options: {string.Join(", ", Enum.GetNames<Ide>())}[/]");
                        return 1;
                    }
                    idePaths[targetIde] = value;
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]  Unknown setting '{Markup.Escape(key)}'.[/]");
                    return ShowSetUsage();
                }
                break;
        }

        var updated = new AppConfig
        {
            DefaultIde = defaultIde,
            DefaultProjectsPath = defaultProjectsPath,
            IdePaths = idePaths
        };

        await services.ConfigRepository.SaveAsync(updated);
        AnsiConsole.MarkupLine($"[green]  -> Configuration updated.[/]");
        return 0;
    }

    private static int ShowSetUsage()
    {
        AnsiConsole.MarkupLine("[yellow]  Usage: dev config set <key> <value>[/]");
        AnsiConsole.MarkupLine("[dim]  Keys: defaultIde, defaultPath, idePath.<ide>[/]");
        AnsiConsole.MarkupLine("[dim]  Example: dev config set defaultIde vscode[/]");
        AnsiConsole.MarkupLine("[dim]  Example: dev config set idePath.rider C:\\tools\\rider.exe[/]");
        return 1;
    }

    private static int ShowUsage()
    {
        AnsiConsole.MarkupLine("[yellow]  Usage: dev config [show|set <key> <value>][/]");
        return 1;
    }
}
