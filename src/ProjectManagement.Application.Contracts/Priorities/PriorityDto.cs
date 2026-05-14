using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace ProjectManagement.Priorities
{
    public class PriorityDto: AuditedEntityDto<Guid>
    {
        public string Title { get; set; } = string.Empty;
    }
}
