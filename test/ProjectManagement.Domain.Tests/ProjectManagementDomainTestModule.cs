using Volo.Abp.Modularity;

namespace ProjectManagement;

[DependsOn(
    typeof(ProjectManagementDomainModule),
    typeof(ProjectManagementTestBaseModule)
)]
public class ProjectManagementDomainTestModule : AbpModule
{

}
