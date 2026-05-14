using ProjectManagement.Board;
using ProjectManagement.Enums;
using ProjectManagement.Projects;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Domain.Entities.Auditing;

namespace ProjectManagement.WorkTask
{
    public class WorkTask: AuditedAggregateRoot<Guid>
    {
        public string Title { get; set; } = string.Empty;
        public Guid  ProjectId { get; set; }
        public DateTimeOffset StartedTime { get; set; }
        public DateTimeOffset EndedTime { get; set; }

    }
}
