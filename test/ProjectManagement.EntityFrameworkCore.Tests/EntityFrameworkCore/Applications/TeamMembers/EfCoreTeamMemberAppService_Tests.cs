using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using ProjectManagement.TeamMembers;

namespace ProjectManagement.EntityFrameworkCore.Applications.TeamMembers
{
    [Collection(ProjectManagementTestConsts.CollectionDefinitionName)]
    public class EfCoreTeamMemberAppService_Tests: TeamMemberAppService_Tests<ProjectManagementEntityFrameworkCoreTestModule>
    {
        
    }
}
