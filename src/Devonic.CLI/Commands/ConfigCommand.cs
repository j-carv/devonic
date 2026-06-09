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
            "set" when args.Length >= 3 => await SetAsync(services, args[1], string.Join(" ", args[2..])),
            "set" when args.Length == 2 => await SetInteractiveAsync(services, args[1]),
            "set" => ShowSetUsage(),
            _ => ShowUsage()
        };
    }

    private static async Task<int> ShowAsync(ServiceLocator services)
    {
        var config = await services.ConfigRepository.GetAsync();

        AnsiConsole.WriteLine();
        var grid = new Grid().AddColumn().AddColumn();
        grid.AddRow("[bold]Default IDE[/]", config.DefaultIde?.ToString() ?? "[dim]not set[/]");
        grid.AddRow("[bold]Projects path[/]", config.DefaultProjectsPath is not null ? Markup.Escape(config.DefaultProjectsPath) : "[dim]not set[/]");

        if (config.IdePaths.Count > 0)
            foreach (var (ide, path) in config.IdePaths)
                grid.AddRow($"[bold]{ide} path[/]", Markup.Escape(path));
        else
            grid.AddRow("[bold]IDE paths[/]", "[dim]using defaults[/]");

        AnsiConsole.Write(new Panel(grid).Border(BoxBorder.Rounded).Header("[bold]Configuration[/]").Expand());
        AnsiConsole.WriteLine();
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
                    AnsiConsole.MarkupLine($"\n  [red]x[/] Unknown IDE. Available: {string.Join(", ", Enum.GetNames<Ide>())}\n");
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
                        AnsiConsole.MarkupLine($"\n  [red]x[/] Unknown IDE. Available: {string.Join(", ", Enum.GetNames<Ide>())}\n");
                        return 1;
                    }
                    idePaths[targetIde] = value;
                }
                else
                {
                    AnsiConsole.MarkupLine($"\n  [red]x[/] Unknown key '{Markup.Escape(key)}'.\n");
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
        AnsiConsole.MarkupLine($"\n  [green]>[/] [bold]{Markup.Escape(key)}[/] set to [cyan]{Markup.Escape(value)}[/]\n");
        return 0;
    }

    private static async Task<int> SetInteractiveAsync(ServiceLocator services, string key)
    {
        return key.ToLowerInvariant() switch
        {
            "defaultide" => await SetAsync(services, key, AnsiConsole.Prompt(
                new SelectionPrompt<Ide>()
                    .Title("  Select default [green]IDE[/]:")
                    .AddChoices(Enum.GetValues<Ide>())
                    .UseConverter(i => i.ToString())).ToString()),
            _ => ShowSetUsage()
        };
    }

    private static int ShowSetUsage()
    {
        AnsiConsole.MarkupLine("\n  [bold]Usage:[/] dev config set [green]<key>[/] [green]<value>[/]");
        AnsiConsole.MarkupLine("  [dim]Keys: defaultIde, defaultPath, idePath.<ide>[/]");
        AnsiConsole.MarkupLine("  [dim]Example: dev config set defaultIde vscode[/]");
        AnsiConsole.MarkupLine("  [dim]Example: dev config set idePath.rider C:\\tools\\rider.exe[/]\n");
        return 1;
    }

    private static int ShowUsage()
    {
        AnsiConsole.MarkupLine("\n  [bold]Usage:[/] dev config [dim][show | set <key> <value>][/]\n");
        return 1;
    }
}
