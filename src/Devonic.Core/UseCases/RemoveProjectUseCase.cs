using Devonic.Core.Interfaces;
using Devonic.Core.Results;

namespace Devonic.Core.UseCases;

public sealed class RemoveProjectUseCase(IProjectRepository repository)
{
    public async Task<Result> ExecuteAsync(string projectName)
    {
        var removed = await repository.RemoveAsync(projectName);
        return removed
            ? Result.Success()
            : Result.Failure($"Project '{projectName}' not found.");
    }
}
