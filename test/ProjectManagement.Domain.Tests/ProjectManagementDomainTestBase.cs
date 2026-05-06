using Volo.Abp.Modularity;

namespace ProjectManagement;

/* Inherit from this class for your domain layer tests. */
public abstract class ProjectManagementDomainTestBase<TStartupModule> : ProjectManagementTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
