using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace ProjectManagement.Statuses
{
    public class StatusAppService (IStatusRepository _statusRepository, StatusManager _statusManager): ProjectManagementAppService, IStatusAppService
    {
        public async Task<StatusDto> CreateStatusAsync(CreateStatusDto input)
        {
            var status = await _statusManager.CreateStatusAsync(input.Title);
            await _statusRepository.InsertAsync(status);
            return ObjectMapper.Map<Status, StatusDto>(status);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _statusRepository.DeleteAsync(id);
        }

        public async Task<PagedResultDto<StatusDto>> GetListStatusAsync(StatusPagedAndSortedResultRequestDto input)
        {
            if (input.Sorting.IsNullOrWhiteSpace())
            {
                input.Sorting = nameof(Status.Title);
            }
            var statuses = await _statusRepository.GetListStatusAsync(input.SkipCount, input.MaxResultCount, input.Sorting, input.Filter);

            var totalCount = input.Filter == null ? await _statusRepository.CountAsync() : await _statusRepository.CountAsync(s => s.Title.Contains(input.Filter));

            return new PagedResultDto<StatusDto>(totalCount, ObjectMapper.Map<List<Status>, List<StatusDto>>(statuses));
        }

        public async Task<StatusDto> GetStatusAsync(Guid id)
        {
            var status = await _statusRepository.GetAsync(id);
            return ObjectMapper.Map<Status, StatusDto>(status);
        }
    }
}
