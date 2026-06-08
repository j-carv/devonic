using Devonic.Core.Results;

namespace Devonic.Core.Interfaces;

public interface IGitService
{
    Task<Result<string>> CloneAsync(string url, string targetPath);
}
