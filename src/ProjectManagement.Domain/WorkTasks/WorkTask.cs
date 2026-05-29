
using ProjectManagement.Projects;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace ProjectManagement.WorkTasks
{
    public class WorkTask: AuditedAggregateRoot<Guid>
    {
        public string Title { get; private set; } = string.Empty;
        public DateTime StartedTime { get; private set; }
        public DateTime EndedTime { get; private set; }
        public Guid  ProjectId { get; private set; }
        public Guid StatusId { get; private set; }
        public Guid PriorityId { get; private set; }
        public Guid? AssigneeId { get; private set; }
        private WorkTask()
        {
            
        }
        internal WorkTask(Guid id, string title, DateTime startedtime, DateTime endedtime, Guid projectId, Guid statusId, Guid priorityId, Guid? assigneeId)
        {
            if (startedtime > endedtime)
            {
                throw new WorkTaskTimeConflictException(startedtime, endedtime);
            }
            Id = id;
            Title = title;
            StartedTime = startedtime;
            EndedTime = endedtime;
            ProjectId = projectId;
            StatusId = statusId;
            PriorityId = priorityId;
            AssigneeId = assigneeId;
        }
        internal void ChangeProjectId(Guid newProjectId)
        {
            if (ProjectId == newProjectId || newProjectId == Guid.Empty)
            {
                return; // No change
            }
            ProjectId = newProjectId;
        }
        internal void ChangeTitle(string newTitle)
        {
            if (string.IsNullOrWhiteSpace(newTitle) || Title == newTitle)
            {
                return; // No change or invalid
            }
            Title = newTitle;
        }
        internal void AssignTo(Guid newAssigneeId)
        {
            if (AssigneeId == newAssigneeId || newAssigneeId == Guid.Empty)
            {
                return; // No change
            }
            AssigneeId = newAssigneeId;
        }

        internal void ChangeStatus(Guid newStatusId)
        {
            if (StatusId == newStatusId || newStatusId == Guid.Empty)
            {
                return;
            }
            StatusId = newStatusId;
        }
        internal void ChangePriority(Guid newPriorityId)
        {
            if (PriorityId == newPriorityId || newPriorityId == Guid.Empty)
            {
                return;
            }
            PriorityId = newPriorityId;
        }
        internal void UpdateTimeRange(DateTime startedTime, DateTime endedTime)
        {
            if (endedTime <= startedTime)
            {
                throw new BusinessException("WorkTask:InvalidTimeRange")
                    .WithData("StartedTime", startedTime)
                    .WithData("EndedTime", endedTime);
            }

            StartedTime = startedTime;
            EndedTime = endedTime;
        }
    }
    
}
