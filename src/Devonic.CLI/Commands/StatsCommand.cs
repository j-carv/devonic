using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class StatsCommand
{
    public static async Task<int> RunAsync(ServiceLocator services)
    {
        var projects = await services.ProjectRepository.GetAllAsync();
        var usages = await services.UsageTracker.GetRecentAsync(50);
        var totalOpens = usages.Sum(u => u.OpenCount);

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold]Stats[/]").LeftJustified());
        AnsiConsole.WriteLine();

        var grid = new Grid().AddColumn().AddColumn();
        grid.AddRow("  [bold]Projects[/]", $"[green]{projects.Count}[/]");
        grid.AddRow("  [bold]Starred[/]", $"[yellow]{projects.Count(p => p.IsFavorite)}[/]");
        grid.AddRow("  [bold]Opens[/]", totalOpens.ToString());

        var tags = projects.SelectMany(p => p.Tags).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (tags.Count > 0)
            grid.AddRow("  [bold]Tags[/]", string.Join("  ", tags.Select(t => $"[dim]#{t}[/]")));

        var ideGroups = projects.GroupBy(p => p.Ide).OrderByDescending(g => g.Count()).ToList();
        grid.AddRow("  [bold]IDEs[/]", string.Join("  ", ideGroups.Select(g => $"{g.Key} [dim]({g.Count()})[/]")));

        AnsiConsole.Write(grid);

        if (usages.Count > 0)
        {
            AnsiConsole.WriteLine();
            var chart = new BarChart().Width(60).Label("  [bold]Most opened[/]");
            var colors = new[] { Color.Green, Color.Cyan1, Color.Blue, Color.Yellow, Color.Magenta1, Color.Orange1 };

            foreach (var (usage, i) in usages.OrderByDescending(u => u.OpenCount).Take(8).Select((u, i) => (u, i)))
                chart.AddItem(usage.ProjectName, usage.OpenCount, colors[i % colors.Length]);

            AnsiConsole.Write(chart);
        }

        AnsiConsole.WriteLine();
        return 0;
    }
}
