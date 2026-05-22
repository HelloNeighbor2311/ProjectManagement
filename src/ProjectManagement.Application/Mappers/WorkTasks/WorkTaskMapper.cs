using Riok.Mapperly.Abstractions;
using Volo.Abp.DependencyInjection;

namespace ProjectManagement.WorkTasks
{
    [Mapper]
    public partial class WorkTaskMapper: ITransientDependency
    {
        public partial WorkTaskDto ToDto(WorkTask entity);

        [MapProperty(nameof(WorkTask.Title), nameof(WorkTaskDetailDto.Title))]
        [MapProperty(nameof(WorkTask.StatusId), nameof(WorkTaskDetailDto.StatusId))]
        [MapProperty(nameof(WorkTask.ProjectId), nameof(WorkTaskDetailDto.ProjectId))]
        [MapProperty(nameof(WorkTask.PriorityId), nameof(WorkTaskDetailDto.PriorityId))]
        [MapProperty(nameof(WorkTask.AssigneeId), nameof(WorkTaskDetailDto.AssigneeId))]
        public partial WorkTaskDetailDto ToDetailDto(WorkTask entity);

        public WorkTaskDetailDto MapWithDates(WorkTask workTask)
        {
            var dto = ToDetailDto(workTask);
            dto.StartedDate = workTask.StartedTime;
            dto.EndedDate = workTask.EndedTime;
            return dto;
        }
       
        public WorkTaskDetailDto ToDetailDtoWithNames(WorkTask workTask, string projectName, string statusName, string priorityName, string assigneeName)
        {
            var dto = ToDetailDto(workTask);
            // Map date/time fields from entity to DTO so list endpoints include them
            dto.StartedDate = workTask.StartedTime;
            dto.EndedDate = workTask.EndedTime;
            dto.StatusName = statusName;
            dto.ProjectName = projectName;
            dto.PriorityName = priorityName;
            dto.AssigneeName = assigneeName ?? string.Empty;
            return dto;
        }
    }
}
