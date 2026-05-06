using ProjectManagement.Samples;
using Xunit;

namespace ProjectManagement.EntityFrameworkCore.Domains;

[Collection(ProjectManagementTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<ProjectManagementEntityFrameworkCoreTestModule>
{

}
