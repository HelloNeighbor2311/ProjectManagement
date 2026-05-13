using ProjectManagement.Permissions;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ProjectManagement.Projects
{
    public class ProjectAppService: CrudAppService<Project, ProjectDto,Guid, PagedAndSortedResultRequestDto, CreateUpdateProjectDto>, IProjectAppService
    {
        public ProjectAppService(IRepository<Project, Guid> repository): base(repository)
        {
            GetPolicyName = ProjectManagementPermissions.Projects.Default;
            GetListPolicyName = ProjectManagementPermissions.Projects.Default;
            CreatePolicyName = ProjectManagementPermissions.Projects.Create;
            UpdatePolicyName = ProjectManagementPermissions.Projects.Edit;
            DeletePolicyName = ProjectManagementPermissions.Projects.Delete;
        }
    }
}
