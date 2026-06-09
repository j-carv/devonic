using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class RecentCommand
{
    public static async Task<int> RunAsync(ServiceLocator services)
    {
        var usages = await services.UsageTracker.GetRecentAsync();

        if (usages.Count == 0)
        {
            AnsiConsole.MarkupLine("\n  [dim]No history yet. Open a project to start tracking.[/]\n");
            return 0;
        }

        AnsiConsole.WriteLine();
        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold]Recently opened[/]")
            .AddColumn("[bold]Project[/]")
            .AddColumn("[bold]Last opened[/]")
            .AddColumn("[bold]Opens[/]");

        foreach (var u in usages)
        {
            table.AddRow(
                $"[bold]{Markup.Escape(u.ProjectName)}[/]",
                FormatRelativeTime(u.LastOpened),
                u.OpenCount.ToString());
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        return 0;
    }

    private static string FormatRelativeTime(DateTime utcTime)
    {
        var diff = DateTime.UtcNow - utcTime;

        return diff.TotalMinutes switch
        {
            < 1 => "just now",
            < 60 => $"{(int)diff.TotalMinutes}m ago",
            < 1440 => $"{(int)diff.TotalHours}h ago",
            < 10080 => $"{(int)diff.TotalDays}d ago",
            _ => utcTime.ToLocalTime().ToString("MMM dd")
        };
    }
}
