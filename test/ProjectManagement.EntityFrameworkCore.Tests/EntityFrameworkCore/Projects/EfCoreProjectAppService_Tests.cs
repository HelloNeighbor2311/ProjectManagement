using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ProjectManagement.EntityFrameworkCore.Projects
{
    [Collection(ProjectManagementTestConsts.CollectionDefinitionName)]
    public class EfCoreProjectAppService_Tests: ProjectAppService_Tests<ProjectManagementEntityFrameworkCoreTestModule>
    {

    }
}
