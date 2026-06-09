using Devonic.Core.Entities;
using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class CloneCommand
{
    public static async Task<int> RunAsync(ServiceLocator services, string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            AnsiConsole.MarkupLine("\n  [red]x[/] Missing URL. Usage: [green]dev clone <git-url>[/]\n");
            return 1;
        }

        var config = await services.ConfigRepository.GetAsync();

        string? targetPath = null;
        if (config.DefaultProjectsPath is null)
        {
            targetPath = AnsiConsole.Prompt(
                new TextPrompt<string>("  [green]Clone to:[/]")
                    .DefaultValue(Directory.GetCurrentDirectory()));
        }

        var ide = config.DefaultIde ?? AnsiConsole.Prompt(
            new SelectionPrompt<Ide>()
                .Title("  [green]IDE for this project:[/]")
                .AddChoices(Enum.GetValues<Ide>())
                .UseConverter(i => i.ToString()));

        var result = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Cloning...", async _ =>
                await services.CloneProject.ExecuteAsync(url, ide, targetPath));

        if (result.IsSuccess)
        {
            AnsiConsole.MarkupLine($"\n  [green]+[/] Cloned and registered. Open it with [green]dev {Markup.Escape(ExtractRepoName(url))}[/]\n");
            return 0;
        }

        AnsiConsole.MarkupLine($"\n  [red]x[/] {Markup.Escape(result.Error!)}\n");
        return 1;
    }

    private static string ExtractRepoName(string url)
    {
        var name = url.Split('/').Last();
        if (name.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            name = name[..^4];
        return name;
    }
}
