using System.Text.Json;
using Devonic.Core.Entities;
using Devonic.Core.Interfaces;

namespace Devonic.Infrastructure.Persistence;

public sealed class JsonConfigRepository(string filePath) : IConfigRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public async Task<AppConfig> GetAsync()
    {
        if (!File.Exists(filePath))
            return new AppConfig();

        var json = await File.ReadAllTextAsync(filePath);
        var dto = JsonSerializer.Deserialize<ConfigDto>(json, JsonOptions);
        if (dto is null)
            return new AppConfig();

        Ide? defaultIde = null;
        if (dto.DefaultIde is not null && Enum.TryParse<Ide>(dto.DefaultIde, ignoreCase: true, out var parsed))
            defaultIde = parsed;

        var idePaths = new Dictionary<Ide, string>();
        foreach (var (key, value) in dto.IdePaths)
        {
            if (Enum.TryParse<Ide>(key, ignoreCase: true, out var ide))
                idePaths[ide] = value;
        }

        return new AppConfig
        {
            DefaultIde = defaultIde,
            DefaultProjectsPath = dto.DefaultProjectsPath,
            IdePaths = idePaths
        };
    }

    public async Task SaveAsync(AppConfig config)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (dir is not null && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var dto = new ConfigDto
        {
            DefaultIde = config.DefaultIde?.ToString().ToLowerInvariant(),
            DefaultProjectsPath = config.DefaultProjectsPath,
            IdePaths = config.IdePaths.ToDictionary(
                kv => kv.Key.ToString().ToLowerInvariant(),
                kv => kv.Value)
        };

        var json = JsonSerializer.Serialize(dto, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }
}
