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
            
        }
    }
}
