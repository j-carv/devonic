using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class StatsCommand
{
    public static async Task<int> RunAsync(ServiceLocator services)
    {
        var projects = await services.ProjectRepository.GetAllAsync();
        var usages = await services.UsageTracker.GetRecentAsync(50);

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new FigletText("stats").Color(Color.Blue).LeftJustified());

        var grid = new Grid().AddColumn().AddColumn();
        grid.AddRow("[bold]Projects[/]", $"[green]{projects.Count}[/]");
        grid.AddRow("[bold]Starred[/]", $"[yellow]{projects.Count(p => p.IsFavorite)}[/]");
        grid.AddRow("[bold]Total opens[/]", usages.Sum(u => u.OpenCount).ToString());

        var tags = projects.SelectMany(p => p.Tags).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        grid.AddRow("[bold]Tags[/]", tags.Count > 0
            ? string.Join("  ", tags.Select(t => $"[dim]#{t}[/]"))
            : "[dim]none[/]");

        AnsiConsole.Write(new Panel(grid).Border(BoxBorder.Rounded).Header("[bold]Overview[/]").Expand());

        if (usages.Count > 0)
        {
            AnsiConsole.WriteLine();
            var chart = new BarChart().Width(60).Label("[bold]Most opened[/]");
            var colors = new[] { Color.Green, Color.Cyan1, Color.Blue, Color.Yellow, Color.Magenta1, Color.Orange1 };

            foreach (var (usage, i) in usages.OrderByDescending(u => u.OpenCount).Take(10).Select((u, i) => (u, i)))
                chart.AddItem(usage.ProjectName, usage.OpenCount, colors[i % colors.Length]);

            AnsiConsole.Write(chart);
        }

        var ideGroups = projects.GroupBy(p => p.Ide).OrderByDescending(g => g.Count()).ToList();
        if (ideGroups.Count > 0)
        {
            AnsiConsole.WriteLine();
            var ideChart = new BarChart().Width(60).Label("[bold]IDE usage[/]");
            foreach (var group in ideGroups)
                ideChart.AddItem(group.Key.ToString(), group.Count(), Color.Cyan1);
            AnsiConsole.Write(ideChart);
        }

        AnsiConsole.WriteLine();
        return 0;
    }
}
