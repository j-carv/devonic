using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class RemoveCommand
{
    public static async Task<int> RunAsync(ServiceLocator services, string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            var projects = await services.ProjectRepository.GetAllAsync();
            if (projects.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]  No projects registered.[/]");
                return 1;
            }

            name = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("  Select project to [red]remove[/]:")
                    .AddChoices(projects.Select(p => p.Name)));
        }

        if (!AnsiConsole.Confirm($"  Remove project [red]'{Markup.Escape(name)}'[/]?"))
            return 0;

        var result = await services.RemoveProject.ExecuteAsync(name);

        if (result.IsSuccess)
        {
            AnsiConsole.MarkupLine($"[green]  -> Project '{Markup.Escape(name)}' removed.[/]");
            return 0;
        }

        AnsiConsole.MarkupLine($"[red]  Error: {Markup.Escape(result.Error!)}[/]");
        return 1;
    }
}
