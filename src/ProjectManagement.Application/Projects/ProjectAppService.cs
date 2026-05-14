using ProjectManagement.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ProjectManagement.Projects
{
    public class ProjectAppService : CrudAppService<Project, ProjectDto, Guid, ProjectPagedAndSortedResultRequestDto, CreateUpdateProjectDto>, IProjectAppService
    {
        public ProjectAppService(IRepository<Project, Guid> repository) : base(repository)
        {
            GetPolicyName = ProjectManagementPermissions.Projects.Default;
            GetListPolicyName = ProjectManagementPermissions.Projects.Default;
            CreatePolicyName = ProjectManagementPermissions.Projects.Create;
            UpdatePolicyName = ProjectManagementPermissions.Projects.Edit;
            DeletePolicyName = ProjectManagementPermissions.Projects.Delete;
        }

        public override async Task<PagedResultDto<ProjectDto>> GetListAsync(ProjectPagedAndSortedResultRequestDto input)
        {
            await CheckGetListPolicyAsync();

            input ??= new ProjectPagedAndSortedResultRequestDto();

            var query = await Repository.GetQueryableAsync();

            var filter = input.Filter?.Trim();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                query = query.Where(
                    x => (x.Name != null && x.Name.Contains(filter)) ||
                         (x.Description != null && x.Description.Contains(filter))
                );
            }

            var totalCount = await AsyncExecuter.CountAsync(query);

            query = ApplySorting(query, input);
            query = ApplyPaging(query, input);

            var entities = await AsyncExecuter.ToListAsync(query);
            var dtos = await MapToGetListOutputDtosAsync(entities);

            return new PagedResultDto<ProjectDto>(totalCount, dtos);
        }
    }
}
