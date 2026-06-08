using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class ListCommand
{
    public static async Task<int> RunAsync(ServiceLocator services)
    {
        var projects = await services.ProjectRepository.GetAllAsync();

        if (projects.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]  No projects registered. Use 'dev add' to add one.[/]");
            return 0;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]Name[/]")
            .AddColumn("[bold]IDE[/]")
            .AddColumn("[bold]Path[/]")
            .AddColumn("[bold]Run[/]")
            .AddColumn("[bold]Fav[/]");

        foreach (var p in projects.OrderBy(p => p.Name))
        {
            table.AddRow(
                p.IsFavorite ? $"[yellow]{Markup.Escape(p.Name)}[/]" : Markup.Escape(p.Name),
                p.Ide.ToString(),
                Markup.Escape(p.Path),
                Markup.Escape(p.RunCommand ?? "-"),
                p.IsFavorite ? "[yellow]*[/]" : "-");
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"\n  [dim]{projects.Count} project(s)[/]");
        return 0;
    }
}
