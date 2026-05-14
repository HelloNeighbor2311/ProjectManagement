using ProjectManagement.Permissions;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ProjectManagement.Priorities
{
    public class PriorityAppService: CrudAppService<Priority, PriorityDto, Guid, PagedAndSortedResultRequestDto,CreateUpdatePriorityDto>, IPriorityAppService
    {
        public PriorityAppService(IRepository<Priority,Guid> _priorityRepository): base(_priorityRepository)
        {
            GetPolicyName = ProjectManagementPermissions.Priorities.Default;
            GetListPolicyName = ProjectManagementPermissions.Priorities.Default;
            CreatePolicyName = ProjectManagementPermissions.Priorities.Create;
            UpdatePolicyName = ProjectManagementPermissions.Priorities.Edit;
            DeletePolicyName = ProjectManagementPermissions.Priorities.Delete;
        }
    }
}
