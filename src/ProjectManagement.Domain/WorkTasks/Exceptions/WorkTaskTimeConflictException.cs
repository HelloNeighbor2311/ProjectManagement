using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp;

namespace ProjectManagement.WorkTasks
{
    public class WorkTaskTimeConflictException: BusinessException
    {
        public WorkTaskTimeConflictException(DateTime startedTime, DateTime endedTime): base(ProjectManagementDomainErrorCodes.WorkTaskTimeConflict)
        {
            WithData("StartedTime", startedTime);
            WithData("EndedTime", endedTime);
        }
    }
}
