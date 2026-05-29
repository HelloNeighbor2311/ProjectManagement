using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace ProjectManagement.WorkTasks
{
    public class WorkTaskManager(IWorkTaskRepository _workTaskRepository): DomainService
    {
        public async Task<WorkTask> CreateWorkTaskAsync(string title, DateTime startedTime, DateTime endedTime, Guid projectId, Guid statusId, Guid priorityId, Guid? assigneeId ) {
            Check.NotNullOrWhiteSpace(title, nameof(title));
            var existedWorkTask = await _workTaskRepository.GetWorkTaskByName(title);
            if(existedWorkTask != null)
            {
                throw new WorkTaskAlreadyExistedException(title);
            }
            var workTask = new WorkTask(GuidGenerator.Create(), title, startedTime, endedTime, projectId, statusId, priorityId, assigneeId);
            return workTask;
        }
        public async Task UpdateWorkTaskAsync(WorkTask workTask, string title, DateTime startedTime, DateTime endedTime, Guid projectId, Guid statusId, Guid priorityId, Guid? assigneeId)
        {
            Check.NotNull(workTask, nameof(workTask));
            Check.NotNullOrWhiteSpace(title, nameof(title));
            var existedWorkTask = await _workTaskRepository.GetWorkTaskByName(title);
            if (existedWorkTask != null && existedWorkTask.Id != workTask.Id)
            {
                throw new WorkTaskAlreadyExistedException(title);
            }
            // Update title on aggregate
            workTask.ChangeTitle(title);
            workTask.ChangeProjectId(projectId);
            workTask.UpdateTimeRange(startedTime, endedTime);
            workTask.ChangeStatus(statusId);
            workTask.ChangePriority(priorityId);
            workTask.AssignTo(assigneeId ?? Guid.Empty);
        }

        public void ChangeWorkTaskStatus(WorkTask workTask, Guid statusId)
        {
            Check.NotNull(workTask, nameof(workTask));
            workTask.ChangeStatus(statusId);
        }
    }
}
