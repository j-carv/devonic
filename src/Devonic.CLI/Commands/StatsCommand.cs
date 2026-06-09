using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class StatsCommand
{
    public static async Task<int> RunAsync(ServiceLocator services)
    {
        var projects = await services.ProjectRepository.GetAllAsync();
        var usages = await services.UsageTracker.GetRecentAsync(50);

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold blue]devonic stats[/]").LeftJustified());
        AnsiConsole.WriteLine();

        // Overview
        var overviewTable = new Table().Border(TableBorder.None).HideHeaders().AddColumn("").AddColumn("");
        overviewTable.AddRow("[bold]Total projects[/]", projects.Count.ToString());
        overviewTable.AddRow("[bold]Favorites[/]", projects.Count(p => p.IsFavorite).ToString());
        overviewTable.AddRow("[bold]Total opens[/]", usages.Sum(u => u.OpenCount).ToString());

        var tags = projects.SelectMany(p => p.Tags).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        overviewTable.AddRow("[bold]Tags[/]", tags.Count > 0 ? string.Join(", ", tags) : "[dim]none[/]");
        AnsiConsole.Write(overviewTable);

        // Usage chart
        if (usages.Count > 0)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("[dim]Most opened[/]").LeftJustified());
            AnsiConsole.WriteLine();

            var chart = new BarChart().Width(60);
            var colors = new[] { Color.Green, Color.Blue, Color.Cyan1, Color.Yellow, Color.Magenta1, Color.Orange1 };

            foreach (var (usage, i) in usages.OrderByDescending(u => u.OpenCount).Take(10).Select((u, i) => (u, i)))
                chart.AddItem(usage.ProjectName, usage.OpenCount, colors[i % colors.Length]);

            AnsiConsole.Write(chart);
        }

        // IDE distribution
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[dim]IDEs[/]").LeftJustified());
        AnsiConsole.WriteLine();

        var ideGroups = projects.GroupBy(p => p.Ide).OrderByDescending(g => g.Count());
        var ideChart = new BarChart().Width(60);
        foreach (var group in ideGroups)
            ideChart.AddItem(group.Key.ToString(), group.Count(), Color.Cyan1);

        AnsiConsole.Write(ideChart);
        AnsiConsole.WriteLine();

        return 0;
    }
}
