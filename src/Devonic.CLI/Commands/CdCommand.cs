using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class CdCommand
{
    public static async Task<int> RunAsync(ServiceLocator services, string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            AnsiConsole.MarkupLine("\n  [red]x[/] Usage: [green]dev cd <project>[/]\n");
            return 1;
        }

        var project = await services.ProjectRepository.GetByNameOrAliasAsync(name);
        if (project is null)
        {
            AnsiConsole.MarkupLine($"\n  [red]x[/] Project '{Markup.Escape(name)}' not found.\n");
            return 1;
        }

        Console.WriteLine(project.Path);
        return 0;
    }
}
