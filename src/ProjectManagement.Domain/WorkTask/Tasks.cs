using ProjectManagement.Board;
using ProjectManagement.Enums;
using ProjectManagement.Projects;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Domain.Entities.Auditing;

namespace ProjectManagement.WorkTask
{
    public class Tasks: AuditedAggregateRoot<Guid>
    {
        public string Title { get; set; } = string.Empty;
        public Project Project { get; set; }
        public Assignees Assignees { get; set; }
        public DateTimeOffset StartedTime { get; set; }
        public DateTimeOffset EndedTime { get; set; }
        public Status Status { get; set; }

        public Boards Board { get; set; }

    }
}
