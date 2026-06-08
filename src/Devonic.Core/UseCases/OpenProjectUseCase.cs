using System.Diagnostics;
using Devonic.Core.Interfaces;
using Devonic.Core.Results;

namespace Devonic.Core.UseCases;

public sealed class OpenProjectUseCase(
    IProjectRepository repository,
    IIdeOpener ideOpener,
    IUsageTracker usageTracker)
{
    public async Task<Result> ExecuteAsync(string projectName, bool run = false)
    {
        var project = await repository.GetByNameAsync(projectName);

        if (project is null)
            return Result.Failure($"Project '{projectName}' not found.");

        if (!Directory.Exists(project.Path))
            return Result.Failure($"Path '{project.Path}' does not exist.");

        var openResult = await ideOpener.OpenAsync(project);
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
}
