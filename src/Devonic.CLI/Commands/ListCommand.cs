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
            if (tagFilter is not null)
                AnsiConsole.MarkupLine($"\n  [dim]No projects tagged[/] [yellow]#{Markup.Escape(tagFilter)}[/]\n");
            else
                AnsiConsole.MarkupLine("\n  [dim]No projects yet.[/] Run [green]dev add[/] or [green]dev scan[/] to get started.\n");
            return 0;
        }

        var title = tagFilter is not null ? $"Projects tagged #{tagFilter}" : $"{projects.Count} project(s)";
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title($"[bold]{Markup.Escape(title)}[/]")
            .AddColumn("[bold]Name[/]")
            .AddColumn("[bold]Alias[/]")
            .AddColumn("[bold]IDE[/]")
            .AddColumn("[bold]Path[/]")
            .AddColumn("[bold]Tags[/]");

        foreach (var p in projects.OrderByDescending(p => p.IsFavorite).ThenBy(p => p.Name))
        {
            var star = p.IsFavorite ? "[yellow]*[/] " : "  ";
            var tagsCol = p.Tags.Count > 0
                ? string.Join(" ", p.Tags.Select(t => $"[dim]#{Markup.Escape(t)}[/]"))
                : "[dim]-[/]";

            table.AddRow(
                $"{star}[bold]{Markup.Escape(p.Name)}[/]",
                p.Alias is not null ? $"[cyan]{Markup.Escape(p.Alias)}[/]" : "[dim]-[/]",
                p.Ide.ToString(),
                $"[dim]{Markup.Escape(p.Path)}[/]",
                tagsCol);
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        return 0;
    }
}
