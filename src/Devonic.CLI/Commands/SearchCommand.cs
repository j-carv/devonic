using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class SearchCommand
{
    public static async Task<int> RunAsync(ServiceLocator services, string query)
    {
        var results = await services.ProjectRepository.SearchAsync(query);

        if (results.Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]  No projects matching '{Markup.Escape(query)}'.[/]");
            return 0;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]Name[/]")
            .AddColumn("[bold]IDE[/]")
            .AddColumn("[bold]Path[/]");

        foreach (var p in results)
        {
            table.AddRow(
                p.IsFavorite ? $"[yellow]{Markup.Escape(p.Name)}[/]" : Markup.Escape(p.Name),
                p.Ide.ToString(),
                Markup.Escape(p.Path));
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"\n  [dim]{results.Count} result(s)[/]");
        return 0;
    }
}
