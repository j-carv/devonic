using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class ListCommand
{
    public static async Task<int> RunAsync(ServiceLocator services, string? tagFilter = null, string? sortBy = null)
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

        var title = tagFilter is not null
            ? $"#{tagFilter}"
            : $"{projects.Count} {(projects.Count == 1 ? "project" : "projects")}";
        AnsiConsole.WriteLine();

        var usages = sortBy is "opens" or "recent"
            ? await services.UsageTracker.GetRecentAsync(100)
            : null;

        var sorted = sortBy switch
        {
            "opens" => projects
                .OrderByDescending(p => usages!.FirstOrDefault(u => u.ProjectName.Equals(p.Name, StringComparison.OrdinalIgnoreCase))?.OpenCount ?? 0)
                .ThenBy(p => p.Name),
            "recent" => projects
                .OrderByDescending(p => usages!.FirstOrDefault(u => u.ProjectName.Equals(p.Name, StringComparison.OrdinalIgnoreCase))?.LastOpened ?? DateTime.MinValue)
                .ThenBy(p => p.Name),
            _ => projects.OrderByDescending(p => p.IsFavorite).ThenBy(p => p.Name)
        };

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title($"[bold]{Markup.Escape(title)}[/]")
            .AddColumn("[bold]Name[/]")
            .AddColumn("[bold]Alias[/]")
            .AddColumn("[bold]IDE[/]")
            .AddColumn("[bold]Path[/]")
            .AddColumn("[bold]Tags[/]");

        foreach (var p in sorted)
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
