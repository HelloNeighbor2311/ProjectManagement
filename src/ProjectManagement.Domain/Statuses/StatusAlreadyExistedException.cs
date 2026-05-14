using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp;

namespace ProjectManagement.Statuses
{
    public class StatusAlreadyExistedException: BusinessException
    {
        public StatusAlreadyExistedException(string title)
        : base(ProjectManagementDomainErrorCodes.StatusAlreadyExisted)
        {
            WithData("title", title);
        }
    }
}
