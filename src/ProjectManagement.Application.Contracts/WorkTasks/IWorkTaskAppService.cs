using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ProjectManagement.WorkTasks
{
    public interface IWorkTaskAppService: IApplicationService
    {
        Task<PagedResultDto<WorkTaskDetailDto>> GetListWorkTaskAsync(WorkTaskPagedAndSortedResultRequestDto input);
        Task<List<WorkTaskShortDetailDto>> GetListWorkTaskByTeamMemberAsync(Guid teamMemberId);
        Task<WorkTaskDetailDto> CreateWorkTaskAsync(CreateWorkTaskDto input);
        Task<WorkTaskDetailDto> GetWorkTaskDetailAsync(Guid id);
        Task UpdateWorkTaskAsync(Guid id, UpdateWorkTaskDto input);
        Task DeleteWorkTaskAsync(Guid id);
    }
}
