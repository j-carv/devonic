using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class NotesCommand
{
    private static readonly string NotesDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".devonic", "notes");

    public static async Task<int> RunAsync(ServiceLocator services, string? name, string? note)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            AnsiConsole.MarkupLine("\n  [red]x[/] Usage: [green]dev notes <project>[/] or [green]dev notes <project> \"note text\"[/]\n");
            return 1;
        }

        var project = await services.ProjectRepository.GetByNameOrAliasAsync(name);
        if (project is null)
        {
            AnsiConsole.MarkupLine($"\n  [red]x[/] Project '{Markup.Escape(name)}' not found.\n");
            return 1;
        }

        var filePath = Path.Combine(NotesDir, $"{project.Name}.txt");

        if (note is not null)
            return await AddNote(filePath, project.Name, note);

        return await ShowNotes(filePath, project.Name);
    }

    private static async Task<int> AddNote(string filePath, string projectName, string note)
    {
        if (!Directory.Exists(NotesDir))
            Directory.CreateDirectory(NotesDir);

        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        var line = $"[{timestamp}] {note}\n";
        await File.AppendAllTextAsync(filePath, line);

        AnsiConsole.MarkupLine($"\n  [green]+[/] Note added to [bold]{Markup.Escape(projectName)}[/]\n");
        return 0;
    }

    private static async Task<int> ShowNotes(string filePath, string projectName)
    {
        if (!File.Exists(filePath))
        {
            AnsiConsole.MarkupLine($"\n  [dim]No notes for[/] [bold]{Markup.Escape(projectName)}[/][dim]. Add one with[/] [green]dev notes {Markup.Escape(projectName)} \"your note\"[/]\n");
            return 0;
        }

        var content = await File.ReadAllTextAsync(filePath);

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[bold]{Markup.Escape(projectName)}[/]").LeftJustified());
        AnsiConsole.WriteLine();

        foreach (var line in content.TrimEnd().Split('\n'))
        {
            if (line.Length > 0 && line[0] == '[')
            {
                var closeBracket = line.IndexOf(']');
                if (closeBracket > 0)
                {
                    var ts = line[1..closeBracket];
                    var text = line[(closeBracket + 2)..];
                    AnsiConsole.MarkupLine($"  [dim]{Markup.Escape(ts)}[/]  {Markup.Escape(text)}");
                    continue;
                }
            }
            AnsiConsole.MarkupLine($"  {Markup.Escape(line)}");
        }
        AnsiConsole.WriteLine();
        return 0;
    }
}
