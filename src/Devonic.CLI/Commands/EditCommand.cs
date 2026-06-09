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
                AnsiConsole.MarkupLine("[yellow]  No projects registered.[/]");
                return 1;
            }

            name = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("  Select project to [green]edit[/]:")
                    .AddChoices(projects.Select(p => p.Name)));
        }

        var existing = await services.ProjectRepository.GetByNameAsync(name);
        if (existing is null)
        {
            AnsiConsole.MarkupLine($"[red]  Error: Project '{Markup.Escape(name)}' not found.[/]");
            return 1;
        }

        AnsiConsole.MarkupLine($"  Editing [green]{Markup.Escape(name)}[/] [dim](Enter to keep current)[/]\n");

        var path = AnsiConsole.Prompt(
            new TextPrompt<string>($"  Path [dim]({Markup.Escape(existing.Path)})[/]:")
                .AllowEmpty());
        if (string.IsNullOrWhiteSpace(path)) path = existing.Path;

        var ide = AnsiConsole.Prompt(
            new SelectionPrompt<Ide>()
                .Title($"  IDE [dim](current: {existing.Ide})[/]:")
                .AddChoices(Enum.GetValues<Ide>())
                .UseConverter(i => i.ToString()));

        var currentAlias = existing.Alias ?? "(none)";
        var alias = AnsiConsole.Prompt(
            new TextPrompt<string>($"  Alias [dim]({Markup.Escape(currentAlias)})[/]:")
                .AllowEmpty());
        if (string.IsNullOrWhiteSpace(alias)) alias = existing.Alias;

        var currentRun = existing.RunCommand ?? "(none)";
        var runCommand = AnsiConsole.Prompt(
            new TextPrompt<string>($"  Run command [dim]({Markup.Escape(currentRun)})[/]:")
                .AllowEmpty());
        if (string.IsNullOrWhiteSpace(runCommand)) runCommand = existing.RunCommand;

        var currentTags = existing.Tags.Count > 0 ? string.Join(", ", existing.Tags) : "(none)";
        var tagsInput = AnsiConsole.Prompt(
            new TextPrompt<string>($"  Tags [dim]({Markup.Escape(currentTags)})[/]:")
                .AllowEmpty());
        var tags = string.IsNullOrWhiteSpace(tagsInput)
            ? existing.Tags
            : tagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        var isFavorite = AnsiConsole.Confirm("  Favorite?", defaultValue: existing.IsFavorite);

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
            AnsiConsole.MarkupLine($"\n[green]  -> Project '{Markup.Escape(name)}' updated.[/]");
            return 0;
        }

        AnsiConsole.MarkupLine($"\n[red]  Error: {Markup.Escape(result.Error!)}[/]");
        return 1;
    }
}
