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
                AnsiConsole.MarkupLine("\n  [dim]Nothing to remove — no projects registered.[/]\n");
                return 1;
            }

            name = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\n  Select project to [red]remove[/]:")
                    .AddChoices(projects.Select(p => p.Name)));
        }

        var existing = await services.ProjectRepository.GetByNameAsync(name);
        if (existing is not null)
        {
            AnsiConsole.MarkupLine($"\n  [dim]{existing.Ide} | {Markup.Escape(existing.Path)}[/]");
        }

        if (!AnsiConsole.Confirm($"  Remove [red]{Markup.Escape(name)}[/]?"))
        {
            AnsiConsole.MarkupLine("  [dim]Cancelled.[/]\n");
            return 0;
        }

        var result = await services.RemoveProject.ExecuteAsync(name);

        if (result.IsSuccess)
        {
            AnsiConsole.MarkupLine($"\n  [red]-[/] [bold]{Markup.Escape(name)}[/] removed.\n");
            return 0;
        }

        AnsiConsole.MarkupLine($"\n  [red]x[/] {Markup.Escape(result.Error!)}\n");
        return 1;
    }
}
