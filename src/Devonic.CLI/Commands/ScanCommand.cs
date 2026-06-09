using Devonic.Core.Entities;
using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class ScanCommand
{
    private static readonly Dictionary<string, (string Label, string Tag)> ProjectIndicators = new()
    {
        ["*.sln"] = (".NET Solution", "dotnet"),
        ["*.slnx"] = (".NET Solution", "dotnet"),
        ["*.csproj"] = (".NET Project", "dotnet"),
        ["package.json"] = ("Node.js", "node"),
        ["Cargo.toml"] = ("Rust", "rust"),
        ["go.mod"] = ("Go", "go"),
        ["pom.xml"] = ("Java (Maven)", "java"),
        ["build.gradle"] = ("Java (Gradle)", "java"),
        ["pyproject.toml"] = ("Python", "python"),
        ["requirements.txt"] = ("Python", "python"),
        ["Gemfile"] = ("Ruby", "ruby"),
        ["composer.json"] = ("PHP", "php"),
    };

    public static async Task<int> RunAsync(ServiceLocator services, string? directory)
    {
        var scanPath = directory ?? Directory.GetCurrentDirectory();

        if (!Directory.Exists(scanPath))
        {
            AnsiConsole.MarkupLine($"[red]  Directory '{Markup.Escape(scanPath)}' does not exist.[/]");
            return 1;
        }

        AnsiConsole.MarkupLine($"\n  Scanning [cyan]{Markup.Escape(scanPath)}[/]...\n");

        var existing = await services.ProjectRepository.GetAllAsync();
        var existingPaths = existing.Select(p => p.Path.TrimEnd('\\', '/')).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var found = new List<(string Path, string Label, string Tag)>();

        foreach (var dir in Directory.GetDirectories(scanPath))
        {
            var normalizedDir = dir.TrimEnd('\\', '/');
            if (existingPaths.Contains(normalizedDir))
                continue;

            foreach (var (pattern, (label, tag)) in ProjectIndicators)
            {
                if (Directory.GetFiles(dir, pattern, SearchOption.TopDirectoryOnly).Length > 0)
                {
                    found.Add((dir, label, tag));
                    break;
                }
            }
        }

        if (found.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]  No new projects found.[/]");
            return 0;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]#[/]")
            .AddColumn("[bold]Directory[/]")
            .AddColumn("[bold]Type[/]");

        for (var i = 0; i < found.Count; i++)
            table.AddRow((i + 1).ToString(), Markup.Escape(Path.GetFileName(found[i].Path)), found[i].Label);

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"\n  [dim]{found.Count} {(found.Count == 1 ? "project" : "projects")} found[/]\n");

        var selected = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("  Select projects to [green]register[/]:")
                .InstructionsText("[dim](Space to toggle, Enter to confirm)[/]")
                .AddChoices(found.Select(f => Path.GetFileName(f.Path))));

        if (selected.Count == 0) return 0;

        var config = await services.ConfigRepository.GetAsync();
        var ide = config.DefaultIde ?? AnsiConsole.Prompt(
            new SelectionPrompt<Ide>()
                .Title("  Select [green]IDE[/] for these projects:")
                .AddChoices(Enum.GetValues<Ide>())
                .UseConverter(i => i.ToString()));

        var registered = 0;
        foreach (var name in selected)
        {
            var match = found.First(f => Path.GetFileName(f.Path) == name);
            var project = new Project
            {
                Name = name.ToLowerInvariant(),
                Path = match.Path,
                Ide = ide,
                Tags = [match.Tag]
            };

            var result = await services.AddProject.ExecuteAsync(project);
            if (result.IsSuccess)
            {
                registered++;
                AnsiConsole.MarkupLine($"  [green]+[/] {Markup.Escape(name)}");
            }
            else
            {
                AnsiConsole.MarkupLine($"  [red]x[/] {Markup.Escape(name)}: {Markup.Escape(result.Error!)}");
            }
        }

        AnsiConsole.MarkupLine($"\n  [green]+[/] {registered} registered.");
        return 0;
    }
}
