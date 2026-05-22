using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ProjectManagement.Statuses
{
    public interface IStatusAppService: IApplicationService
    {
        Task<StatusDto> GetStatusAsync(Guid id);
        Task<PagedResultDto<StatusDto>> GetListStatusAsync(StatusPagedAndSortedResultRequestDto input);
        Task<StatusDto> CreateStatusAsync(CreateStatusDto input);
        Task DeleteAsync(Guid id);
    }
}
