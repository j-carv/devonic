using Devonic.Core.Entities;
using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class AddCommand
{
    public static async Task<int> RunAsync(ServiceLocator services)
    {
        var name = AnsiConsole.Ask<string>("  Project [green]name[/]:");
        var path = AnsiConsole.Ask<string>("  Project [green]path[/]:");
        var ide = AnsiConsole.Prompt(
            new SelectionPrompt<Ide>()
                .Title("  Select [green]IDE[/]:")
                .AddChoices(Enum.GetValues<Ide>())
                .UseConverter(i => i.ToString()));

        var alias = AnsiConsole.Prompt(
            new TextPrompt<string>("  [green]Alias[/] [dim](short name, optional)[/]:")
                .AllowEmpty());

        string? runCommand = null;
        if (AnsiConsole.Confirm("  Add a [green]run command[/]?", defaultValue: false))
            runCommand = AnsiConsole.Ask<string>("  Run command:");

        var tagsInput = AnsiConsole.Prompt(
            new TextPrompt<string>("  [green]Tags[/] [dim](comma-separated, optional)[/]:")
                .AllowEmpty());
        var tags = string.IsNullOrWhiteSpace(tagsInput)
            ? new List<string>()
            : tagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        var isFavorite = AnsiConsole.Confirm("  Mark as [yellow]favorite[/]?", defaultValue: false);

        var project = new Project
        {
            Name = name,
            Path = path,
            Ide = ide,
            Alias = string.IsNullOrWhiteSpace(alias) ? null : alias,
            RunCommand = runCommand,
            IsFavorite = isFavorite,
            Tags = tags
        };

        var result = await services.AddProject.ExecuteAsync(project);

        if (result.IsSuccess)
        {
            AnsiConsole.MarkupLine($"[green]  -> Project '{Markup.Escape(name)}' added.[/]");
            return 0;
        }

        AnsiConsole.MarkupLine($"[red]  Error: {Markup.Escape(result.Error!)}[/]");
        return 1;
    }
}
