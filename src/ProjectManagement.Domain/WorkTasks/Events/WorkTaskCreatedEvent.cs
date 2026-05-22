using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectManagement.WorkTasks.Events
{
    public class WorkTaskCreatedEvent
    {
        public Guid WorkTaskId { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? AssigneeId { get; set; }
        public string Title { get; set; }
        public DateTimeOffset StartedTime { get; set; }

        public WorkTaskCreatedEvent(Guid workTaskId, Guid projectId, Guid? assigneeId, string title, DateTimeOffset startedTime)
        {
            WorkTaskId = workTaskId;
            ProjectId = projectId;
            AssigneeId = assigneeId;
            Title = title;
            StartedTime = startedTime;
        }
    }
}
