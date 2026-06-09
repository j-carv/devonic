using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class GroupCommand
{
    public static async Task<int> RunAsync(ServiceLocator services, string[] args)
    {
        if (args.Length == 0)
            return await ListAsync(services);

        return args[0].ToLowerInvariant() switch
        {
            "create" or "add" when args.Length >= 3 => await CreateAsync(services, args[1], args[2..]),
            "create" or "add" => ShowUsage("Usage: dev group create <name> <project1> <project2> ..."),
            "open" when args.Length >= 2 => await OpenAsync(services, args[1]),
            "open" => ShowUsage("Usage: dev group open <name>"),
            "remove" or "rm" when args.Length >= 2 => await RemoveAsync(services, args[1]),
            "remove" or "rm" => ShowUsage("Usage: dev group remove <name>"),
            "list" or "ls" => await ListAsync(services),
            _ => await OpenAsync(services, args[0])
        };
    }

    private static async Task<int> CreateAsync(ServiceLocator services, string name, string[] projectNames)
    {
        var allProjects = await services.ProjectRepository.GetAllAsync();
        var valid = new List<string>();

        foreach (var pName in projectNames)
        {
            var project = allProjects.FirstOrDefault(p =>
                p.Name.Equals(pName, StringComparison.OrdinalIgnoreCase) ||
                (p.Alias is not null && p.Alias.Equals(pName, StringComparison.OrdinalIgnoreCase)));

            if (project is not null)
                valid.Add(project.Name);
            else
                AnsiConsole.MarkupLine($"  [yellow]!![/] '{Markup.Escape(pName)}' not found, skipping");
        }

        if (valid.Count == 0)
        {
            AnsiConsole.MarkupLine("\n  [red]x[/] No valid projects to group.\n");
            return 1;
        }

        await services.GroupRepository.SaveAsync(name, valid);
        AnsiConsole.MarkupLine($"\n  [green]+[/] Group [bold]{Markup.Escape(name)}[/] created with {valid.Count} {(valid.Count == 1 ? "project" : "projects")}.\n");
        return 0;
    }

    private static async Task<int> OpenAsync(ServiceLocator services, string name)
    {
        var groups = await services.GroupRepository.GetAllAsync();
        if (!groups.TryGetValue(name, out var projectNames))
        {
            AnsiConsole.MarkupLine($"\n  [red]x[/] Group '{Markup.Escape(name)}' not found.\n");
            return 1;
        }

        var opened = 0;
        foreach (var pName in projectNames)
        {
            var result = await services.OpenProject.ExecuteAsync(pName, run: false);
            if (result.IsSuccess)
            {
                opened++;
                AnsiConsole.MarkupLine($"  [green]>[/] {Markup.Escape(pName)}");
            }
            else
            {
                AnsiConsole.MarkupLine($"  [red]x[/] {Markup.Escape(pName)}: {Markup.Escape(result.Error!)}");
            }
        }

        AnsiConsole.MarkupLine($"\n  [green]{opened}[/] opened.\n");
        return 0;
    }

    private static async Task<int> RemoveAsync(ServiceLocator services, string name)
    {
        var removed = await services.GroupRepository.RemoveAsync(name);
        if (removed)
        {
            AnsiConsole.MarkupLine($"\n  [red]-[/] Group [bold]{Markup.Escape(name)}[/] removed.\n");
            return 0;
        }

        AnsiConsole.MarkupLine($"\n  [red]x[/] Group '{Markup.Escape(name)}' not found.\n");
        return 1;
    }

    private static async Task<int> ListAsync(ServiceLocator services)
    {
        var groups = await services.GroupRepository.GetAllAsync();

        if (groups.Count == 0)
        {
            AnsiConsole.MarkupLine("\n  [dim]No groups. Create one with[/] [green]dev group create <name> <projects...>[/]\n");
            return 0;
        }

        AnsiConsole.WriteLine();
        foreach (var (name, projects) in groups.OrderBy(g => g.Key))
        {
            AnsiConsole.MarkupLine($"  [bold]{Markup.Escape(name)}[/]  [dim]{string.Join("  ", projects.Select(Markup.Escape))}[/]");
        }
        AnsiConsole.WriteLine();
        return 0;
    }

    private static int ShowUsage(string message)
    {
        AnsiConsole.MarkupLine($"\n  [red]x[/] {message}\n");
        return 1;
    }
}
