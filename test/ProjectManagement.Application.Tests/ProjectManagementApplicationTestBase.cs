using Volo.Abp.Modularity;

namespace ProjectManagement;

public abstract class ProjectManagementApplicationTestBase<TStartupModule> : ProjectManagementTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
