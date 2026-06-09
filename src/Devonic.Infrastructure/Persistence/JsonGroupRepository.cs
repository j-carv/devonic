using System.Text.Json;
using Devonic.Core.Interfaces;

namespace Devonic.Infrastructure.Persistence;

public sealed class JsonGroupRepository(string filePath) : IGroupRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public async Task<Dictionary<string, List<string>>> GetAllAsync()
    {
        if (!File.Exists(filePath))
            return new();

        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json, JsonOptions) ?? new();
    }

    public async Task SaveAsync(string name, List<string> projectNames)
    {
        var groups = await GetAllAsync();
        groups[name] = projectNames;
        await WriteAsync(groups);
    }

    public async Task<bool> RemoveAsync(string name)
    {
        var groups = await GetAllAsync();
        if (!groups.Remove(name)) return false;
        await WriteAsync(groups);
        return true;
    }

    private async Task WriteAsync(Dictionary<string, List<string>> groups)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (dir is not null && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(groups, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }
}
