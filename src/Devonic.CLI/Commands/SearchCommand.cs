using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class SearchCommand
{
    public static async Task<int> RunAsync(ServiceLocator services, string query)
    {
        var results = await services.ProjectRepository.SearchAsync(query);

        if (results.Count == 0)
        {
            AnsiConsole.MarkupLine($"\n  [dim]No matches for[/] [bold]'{Markup.Escape(query)}'[/]\n");
            return 0;
        }

        AnsiConsole.WriteLine();
        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title($"[bold]{results.Count} match(es) for '{Markup.Escape(query)}'[/]")
            .AddColumn("[bold]Name[/]")
            .AddColumn("[bold]Alias[/]")
            .AddColumn("[bold]IDE[/]")
            .AddColumn("[bold]Tags[/]");

        foreach (var p in results)
        {
            var tagsCol = p.Tags.Count > 0
                ? string.Join(" ", p.Tags.Select(t => $"[dim]#{Markup.Escape(t)}[/]"))
                : "[dim]-[/]";

            table.AddRow(
                p.IsFavorite ? $"[yellow]*[/] [bold]{Markup.Escape(p.Name)}[/]" : $"  [bold]{Markup.Escape(p.Name)}[/]",
                p.Alias is not null ? $"[cyan]{Markup.Escape(p.Alias)}[/]" : "[dim]-[/]",
                p.Ide.ToString(),
                tagsCol);
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        return 0;
    }
}
