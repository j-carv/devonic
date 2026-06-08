using Devonic.Core.Entities;

namespace Devonic.Core.Interfaces;

public interface IConfigRepository
{
    Task<AppConfig> GetAsync();
    Task SaveAsync(AppConfig config);
}
