using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp;

namespace ProjectManagement.WorkTasks
{
    public class WorkTaskAlreadyExistedException: BusinessException
    {
        public WorkTaskAlreadyExistedException(string title): base(ProjectManagementDomainErrorCodes.WorkTaskAlreadyExisted)
        {
            WithData("title", title);
        }
    }
}
