using Devonic.Core.Entities;
using Devonic.Core.Interfaces;
using Devonic.Core.Results;

namespace Devonic.Core.UseCases;

public sealed class EditProjectUseCase(IProjectRepository repository)
{
    public async Task<Result> ExecuteAsync(Project project)
    {
        if (!Directory.Exists(project.Path))
            return Result.Failure($"Path '{project.Path}' does not exist.");

        var updated = await repository.UpdateAsync(project);
        return updated
            ? Result.Success()
            : Result.Failure($"Project '{project.Name}' not found.");
    }
}
