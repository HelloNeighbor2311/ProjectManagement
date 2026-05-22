using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace ProjectManagement.Statuses
{
    public class StatusDto: AuditedEntityDto<Guid>
    {
        public string Title { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }
}
