using System.Diagnostics;
using Devonic.Core.Entities;
using Devonic.Core.Interfaces;
using Devonic.Core.Results;

namespace Devonic.Core.UseCases;

public sealed class OpenProjectUseCase(
    IProjectRepository repository,
    IIdeOpener ideOpener,
    IUsageTracker usageTracker)
{
    public async Task<Result> ExecuteAsync(string projectName, bool run = false, Ide? ideOverride = null, bool shell = false)
    {
        var project = await repository.GetByNameOrAliasAsync(projectName);

        if (project is null)
            return Result.Failure($"Project '{projectName}' not found.");

        if (!Directory.Exists(project.Path))
            return Result.Failure($"Path '{project.Path}' does not exist.");

        if (shell)
        {
            OpenShell(project.Path);
            await usageTracker.RecordUsageAsync(project.Name);
            return Result.Success();
        }

        var target = ideOverride.HasValue
            ? new Project { Name = project.Name, Path = project.Path, Ide = ideOverride.Value, RunCommand = project.RunCommand, IsFavorite = project.IsFavorite, Tags = project.Tags }
            : project;

        var openResult = await ideOpener.OpenAsync(target);
        if (!openResult.IsSuccess)
            return openResult;

        await usageTracker.RecordUsageAsync(project.Name);

        if (run && !string.IsNullOrWhiteSpace(project.RunCommand))
        {
            var isWindows = OperatingSystem.IsWindows();
            Process.Start(new ProcessStartInfo
            {
                FileName = isWindows ? "cmd.exe" : "/bin/sh",
                Arguments = isWindows
                    ? $"/c {project.RunCommand}"
                    : $"-c \"{project.RunCommand}\"",
                WorkingDirectory = project.Path,
                UseShellExecute = true
            });
        }

        return Result.Success();
    }

    private static void OpenShell(string path)
    {
        if (OperatingSystem.IsWindows())
            Process.Start(new ProcessStartInfo { FileName = "cmd.exe", Arguments = $"/k cd /d \"{path}\"", UseShellExecute = true });
        else
            Process.Start(new ProcessStartInfo { FileName = "/bin/sh", WorkingDirectory = path, UseShellExecute = true });
    }
}
