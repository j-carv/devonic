using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class FavoritesCommand
{
    public static async Task<int> RunAsync(ServiceLocator services)
    {
        var projects = await services.ProjectRepository.GetAllAsync();
        var favorites = projects.Where(p => p.IsFavorite).ToList();

        if (favorites.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]  No favorites. Use 'dev edit <name>' to mark a project as favorite.[/]");
            return 0;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold yellow]Name[/]")
            .AddColumn("[bold]IDE[/]")
            .AddColumn("[bold]Path[/]");

        foreach (var p in favorites.OrderBy(p => p.Name))
        {
            table.AddRow(
                $"[yellow]{Markup.Escape(p.Name)}[/]",
                p.Ide.ToString(),
                Markup.Escape(p.Path));
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"\n  [dim]{favorites.Count} favorite(s)[/]");
        return 0;
    }
}
