using Devonic.Core.Entities;
using Devonic.Core.Interfaces;
using Devonic.Core.Results;

namespace Devonic.Core.UseCases;

public sealed class AddProjectUseCase(IProjectRepository repository)
{
    public async Task<Result> ExecuteAsync(Project project)
    {
        var existing = await repository.GetByNameAsync(project.Name);
        if (existing is not null)
            return Result.Failure($"Project '{project.Name}' already exists.");

        if (!Directory.Exists(project.Path))
            return Result.Failure($"Path '{project.Path}' does not exist.");

        await repository.AddAsync(project);
        return Result.Success();
    }
}
