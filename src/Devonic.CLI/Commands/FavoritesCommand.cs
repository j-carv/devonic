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
            AnsiConsole.MarkupLine("\n  [dim]No starred projects. Use[/] [green]dev edit <name>[/] [dim]to star one.[/]\n");
            return 0;
        }

        AnsiConsole.WriteLine();
        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title($"[bold yellow]Starred ({favorites.Count})[/]")
            .AddColumn("[bold]Name[/]")
            .AddColumn("[bold]Alias[/]")
            .AddColumn("[bold]IDE[/]")
            .AddColumn("[bold]Path[/]");

        foreach (var p in favorites.OrderBy(p => p.Name))
        {
            table.AddRow(
                $"[yellow]*[/] [bold]{Markup.Escape(p.Name)}[/]",
                p.Alias is not null ? $"[cyan]{Markup.Escape(p.Alias)}[/]" : "[dim]-[/]",
                p.Ide.ToString(),
                $"[dim]{Markup.Escape(p.Path)}[/]");
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        return 0;
    }
}
