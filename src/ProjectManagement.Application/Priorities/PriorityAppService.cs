using ProjectManagement.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ProjectManagement.Priorities
{
    public class PriorityAppService: CrudAppService<Priority, PriorityDto, Guid, PriorityPagedAndSortedResultRequestDto,CreateUpdatePriorityDto>, IPriorityAppService
    {
        public PriorityAppService(IRepository<Priority,Guid> _priorityRepository): base(_priorityRepository)
        {
            GetPolicyName = ProjectManagementPermissions.Priorities.Default;
            GetListPolicyName = ProjectManagementPermissions.Priorities.Default;
            CreatePolicyName = ProjectManagementPermissions.Priorities.Create;
            UpdatePolicyName = ProjectManagementPermissions.Priorities.Edit;
            DeletePolicyName = ProjectManagementPermissions.Priorities.Delete;
        }

        public override async Task<PagedResultDto<PriorityDto>> GetListAsync(PriorityPagedAndSortedResultRequestDto input)
        {
            await CheckGetListPolicyAsync();
            input ??= new PriorityPagedAndSortedResultRequestDto();
            var query = await Repository.GetQueryableAsync();
            var filter = input.Filter?.Trim();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                query = query.Where(x => x.Title != null && x.Title.Contains(filter));
            }

            var totalCount = await AsyncExecuter.CountAsync(query);

            query = ApplySorting(query, input);
            query = ApplyPaging(query, input);
            var entities = await AsyncExecuter.ToListAsync(query);
            var dtos = await MapToGetListOutputDtosAsync(entities);

            return new PagedResultDto<PriorityDto>(totalCount, dtos);

        }
    }
}
