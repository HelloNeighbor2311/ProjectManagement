using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp;

namespace ProjectManagement.TeamMembers
{
    public class TeamMemberAlreadyExistedException: BusinessException
    {
        public TeamMemberAlreadyExistedException(string name)
        : base(ProjectManagementDomainErrorCodes.TeamMemberAlreadyExisted)
        {
            WithData("name", name);
        }
    }
}
