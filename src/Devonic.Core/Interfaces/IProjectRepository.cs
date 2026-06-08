using Devonic.Core.Entities;

namespace Devonic.Core.Interfaces;

public interface IProjectRepository
{
    Task<IReadOnlyList<Project>> GetAllAsync();
    Task<Project?> GetByNameAsync(string name);
    Task AddAsync(Project project);
    Task<bool> RemoveAsync(string name);
    Task<bool> UpdateAsync(Project project);
    Task<IReadOnlyList<Project>> SearchAsync(string query);
}
