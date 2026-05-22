using Xunit;

namespace ProjectManagement.EntityFrameworkCore.Applications.WorkTasks
{
    [Collection(ProjectManagementTestConsts.CollectionDefinitionName)]
    public class EfCoreWorkTaskAppService_Tests : WorkTaskAppService_Tests<ProjectManagementEntityFrameworkCoreTestModule>
    {
    }
}
