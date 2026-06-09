namespace Devonic.Core.Interfaces;

public interface IGroupRepository
{
    Task<Dictionary<string, List<string>>> GetAllAsync();
    Task SaveAsync(string name, List<string> projectNames);
    Task<bool> RemoveAsync(string name);
}
