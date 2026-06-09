using Devonic.Core.Entities;
using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class AddCommand
{
    public static async Task<int> RunAsync(ServiceLocator services)
    {
        AnsiConsole.MarkupLine("\n  [bold]New project[/]\n");

        var name = AnsiConsole.Ask<string>("  [green]Name:[/]");
        var path = AnsiConsole.Ask<string>("  [green]Path:[/]");
        var ide = AnsiConsole.Prompt(
            new SelectionPrompt<Ide>()
                .Title("  [green]IDE:[/]")
                .AddChoices(Enum.GetValues<Ide>())
                .UseConverter(i => i.ToString()));

        var alias = AnsiConsole.Prompt(
            new TextPrompt<string>("  [green]Alias[/] [dim](optional):[/]")
                .AllowEmpty());

        string? runCommand = null;
        if (AnsiConsole.Confirm("  Set a run command?", defaultValue: false))
            runCommand = AnsiConsole.Ask<string>("  [green]Command:[/]");

        var tagsInput = AnsiConsole.Prompt(
            new TextPrompt<string>("  [green]Tags[/] [dim](comma-separated, optional):[/]")
                .AllowEmpty());
        var tags = string.IsNullOrWhiteSpace(tagsInput)
            ? new List<string>()
            : tagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        var isFavorite = AnsiConsole.Confirm("  Star this project?", defaultValue: false);

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
            AnsiConsole.MarkupLine($"\n  [green]+[/] [bold]{Markup.Escape(name)}[/] registered. Open it with [green]dev {Markup.Escape(alias ?? name)}[/]\n");
            return 0;
        }

        AnsiConsole.MarkupLine($"\n  [red]x[/] {Markup.Escape(result.Error!)}\n");
        return 1;
    }
}
