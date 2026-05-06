using ProjectManagement.Samples;
using Xunit;

namespace ProjectManagement.EntityFrameworkCore.Applications;

[Collection(ProjectManagementTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<ProjectManagementEntityFrameworkCoreTestModule>
{

}
