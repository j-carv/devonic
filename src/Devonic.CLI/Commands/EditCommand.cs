using Devonic.Core.Entities;
using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class EditCommand
{
    public static async Task<int> RunAsync(ServiceLocator services, string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            var projects = await services.ProjectRepository.GetAllAsync();
            if (projects.Count == 0)
            {
                AnsiConsole.MarkupLine("\n  [dim]Nothing to edit — no projects registered.[/]\n");
                return 1;
            }

            name = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\n  Select project to [green]edit[/]:")
                    .AddChoices(projects.Select(p => p.Name)));
        }

        var existing = await services.ProjectRepository.GetByNameAsync(name);
        if (existing is null)
        {
            AnsiConsole.MarkupLine($"\n  [red]x[/] Project '{Markup.Escape(name)}' not found.\n");
            return 1;
        }

        AnsiConsole.MarkupLine($"\n  [bold]Editing {Markup.Escape(name)}[/] [dim]— press Enter to keep current value[/]\n");

        var path = AnsiConsole.Prompt(
            new TextPrompt<string>($"  [green]Path[/] [dim]({Markup.Escape(existing.Path)}):[/]")
                .AllowEmpty());
        if (string.IsNullOrWhiteSpace(path)) path = existing.Path;

        var ide = AnsiConsole.Prompt(
            new SelectionPrompt<Ide>()
                .Title($"  [green]IDE[/] [dim](current: {existing.Ide}):[/]")
                .AddChoices(Enum.GetValues<Ide>())
                .UseConverter(i => i.ToString()));

        var alias = AnsiConsole.Prompt(
            new TextPrompt<string>($"  [green]Alias[/] [dim]({Markup.Escape(existing.Alias ?? "none")}):[/]")
                .AllowEmpty());
        if (string.IsNullOrWhiteSpace(alias)) alias = existing.Alias;

        var runCommand = AnsiConsole.Prompt(
            new TextPrompt<string>($"  [green]Run command[/] [dim]({Markup.Escape(existing.RunCommand ?? "none")}):[/]")
                .AllowEmpty());
        if (string.IsNullOrWhiteSpace(runCommand)) runCommand = existing.RunCommand;

        var tagsStr = existing.Tags.Count > 0 ? string.Join(", ", existing.Tags) : "none";
        var tagsInput = AnsiConsole.Prompt(
            new TextPrompt<string>($"  [green]Tags[/] [dim]({Markup.Escape(tagsStr)}):[/]")
                .AllowEmpty());
        var tags = string.IsNullOrWhiteSpace(tagsInput)
            ? existing.Tags
            : tagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        var isFavorite = AnsiConsole.Confirm("  Starred?", defaultValue: existing.IsFavorite);

        var updated = new Project
        {
            Name = name,
            Path = path,
            Ide = ide,
            Alias = alias,
            RunCommand = runCommand,
            IsFavorite = isFavorite,
            Tags = tags
        };

        var result = await services.EditProject.ExecuteAsync(updated);

        if (result.IsSuccess)
        {
            AnsiConsole.MarkupLine($"\n  [green]>[/] [bold]{Markup.Escape(name)}[/] updated.\n");
            return 0;
        }

        AnsiConsole.MarkupLine($"\n  [red]x[/] {Markup.Escape(result.Error!)}\n");
        return 1;
    }
}
