using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Domain.Entities.Auditing;

namespace ProjectManagement.Priorities
{
    public class Priority: AuditedAggregateRoot<Guid>
    {
        public string Title { get; set; } = string.Empty;
    }
}
