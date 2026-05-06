using ProjectManagement.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace ProjectManagement.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(ProjectManagementEntityFrameworkCoreModule),
    typeof(ProjectManagementApplicationContractsModule)
    )]
public class ProjectManagementDbMigratorModule : AbpModule
{
}
