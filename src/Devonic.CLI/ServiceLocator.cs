using Devonic.Core.Interfaces;
using Devonic.Core.UseCases;
using Devonic.Infrastructure.Git;
using Devonic.Infrastructure.IdeIntegration;
using Devonic.Infrastructure.Persistence;

namespace Devonic.CLI;

internal sealed class ServiceLocator
{
    private static readonly string BasePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".devonic");

    public IProjectRepository ProjectRepository { get; } = new JsonProjectRepository(Path.Combine(BasePath, "projects.json"));
    public IConfigRepository ConfigRepository { get; } = new JsonConfigRepository(Path.Combine(BasePath, "config.json"));
    public IUsageTracker UsageTracker { get; } = new JsonUsageTracker(Path.Combine(BasePath, "usage.json"));
    public IGitService GitService { get; } = new GitService();
    public IIdeOpener IdeOpener { get; }

    public OpenProjectUseCase OpenProject { get; }
    public AddProjectUseCase AddProject { get; }
    public RemoveProjectUseCase RemoveProject { get; }
    public EditProjectUseCase EditProject { get; }
    public CloneProjectUseCase CloneProject { get; }

    public ServiceLocator()
    {
        IdeOpener = new IdeOpener(ConfigRepository);
        OpenProject = new OpenProjectUseCase(ProjectRepository, IdeOpener, UsageTracker);
        AddProject = new AddProjectUseCase(ProjectRepository);
        RemoveProject = new RemoveProjectUseCase(ProjectRepository);
        EditProject = new EditProjectUseCase(ProjectRepository);
        CloneProject = new CloneProjectUseCase(GitService, ProjectRepository, ConfigRepository);
    }
}
