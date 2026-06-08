using Devonic.Core.Entities;
using Devonic.Core.Results;

namespace Devonic.Core.Interfaces;

public interface IIdeOpener
{
    Task<Result> OpenAsync(Project project);
}
