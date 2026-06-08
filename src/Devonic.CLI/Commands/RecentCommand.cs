using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class RecentCommand
{
    public static async Task<int> RunAsync(ServiceLocator services)
    {
        var usages = await services.UsageTracker.GetRecentAsync();

        if (usages.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]  No recent activity.[/]");
            return 0;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]Name[/]")
            .AddColumn("[bold]Last Opened[/]")
            .AddColumn("[bold]Opens[/]");

        foreach (var u in usages)
        {
            table.AddRow(
                Markup.Escape(u.ProjectName),
                u.LastOpened.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                u.OpenCount.ToString());
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(table);
        return 0;
    }
}
