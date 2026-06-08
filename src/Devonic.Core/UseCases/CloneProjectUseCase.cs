using Devonic.Core.Entities;
using Devonic.Core.Interfaces;
using Devonic.Core.Results;

namespace Devonic.Core.UseCases;

public sealed class CloneProjectUseCase(
    IGitService gitService,
    IProjectRepository repository,
    IConfigRepository configRepository)
{
    public async Task<Result> ExecuteAsync(string url, Ide ide, string? targetPath = null)
    {
        var config = await configRepository.GetAsync();
        var basePath = targetPath
            ?? config.DefaultProjectsPath
            ?? Directory.GetCurrentDirectory();

        if (!Directory.Exists(basePath))
            Directory.CreateDirectory(basePath);

        var cloneResult = await gitService.CloneAsync(url, basePath);
        if (!cloneResult.IsSuccess)
            return Result.Failure(cloneResult.Error!);

        var repoPath = cloneResult.Value!;
        var projectName = Path.GetFileName(repoPath);

        var project = new Project
        {
            Name = projectName,
            Path = repoPath,
            Ide = ide
        };

        var existing = await repository.GetByNameAsync(projectName);
        if (existing is not null)
            return Result.Failure($"Project '{projectName}' was cloned but already exists in devonic. Use 'dev edit {projectName}' to update it.");

        await repository.AddAsync(project);
        return Result.Success();
    }
}
