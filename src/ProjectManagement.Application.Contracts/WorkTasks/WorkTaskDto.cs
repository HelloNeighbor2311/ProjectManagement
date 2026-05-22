using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectManagement.WorkTasks
{
    public class WorkTaskDto
    {
        public string Title { get; set; } = string.Empty;
        public DateTimeOffset StartedTime { get; set; }
        public DateTimeOffset EndedTime { get; set; }
        public Guid ProjectId { get; set; }
        public Guid StatusId { get; set; }
        public Guid PriorityId { get; set; }
        public Guid? AssigneeId { get; set; }
    }
}
