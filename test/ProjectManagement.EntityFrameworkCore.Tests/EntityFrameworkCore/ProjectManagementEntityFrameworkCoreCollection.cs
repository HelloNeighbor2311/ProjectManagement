using Xunit;

namespace ProjectManagement.EntityFrameworkCore;

[CollectionDefinition(ProjectManagementTestConsts.CollectionDefinitionName)]
public class ProjectManagementEntityFrameworkCoreCollection : ICollectionFixture<ProjectManagementEntityFrameworkCoreFixture>
{

}
