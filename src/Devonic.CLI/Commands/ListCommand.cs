using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class ListCommand
{
    public static async Task<int> RunAsync(ServiceLocator services, string? tagFilter = null)
    {
        var projects = tagFilter is not null
            ? await services.ProjectRepository.GetByTagAsync(tagFilter)
            : await services.ProjectRepository.GetAllAsync();

        if (projects.Count == 0)
        {
            var msg = tagFilter is not null
                ? $"No projects with tag '{Markup.Escape(tagFilter)}'."
                : "No projects registered. Use 'dev add' to add one.";
            AnsiConsole.MarkupLine($"[yellow]  {msg}[/]");
            return 0;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]Name[/]")
            .AddColumn("[bold]Alias[/]")
            .AddColumn("[bold]IDE[/]")
            .AddColumn("[bold]Path[/]")
            .AddColumn("[bold]Tags[/]")
            .AddColumn("[bold]Fav[/]");

        foreach (var p in projects.OrderBy(p => p.Name))
        {
            var nameCol = p.IsFavorite ? $"[yellow]{Markup.Escape(p.Name)}[/]" : Markup.Escape(p.Name);
            var tagsCol = p.Tags.Count > 0 ? Markup.Escape(string.Join(", ", p.Tags)) : "[dim]-[/]";

            table.AddRow(
                nameCol,
                Markup.Escape(p.Alias ?? "-"),
                p.Ide.ToString(),
                Markup.Escape(p.Path),
                tagsCol,
                p.IsFavorite ? "[yellow]*[/]" : "-");
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(table);

        if (tagFilter is not null)
            AnsiConsole.MarkupLine($"\n  [dim]{projects.Count} project(s) tagged '{Markup.Escape(tagFilter)}'[/]");
        else
            AnsiConsole.MarkupLine($"\n  [dim]{projects.Count} project(s)[/]");

        return 0;
    }
}
