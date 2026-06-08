using Devonic.Core.Entities;

namespace Devonic.Core.Interfaces;

public interface IUsageTracker
{
    Task RecordUsageAsync(string projectName);
    Task<IReadOnlyList<ProjectUsage>> GetRecentAsync(int count = 10);
}
