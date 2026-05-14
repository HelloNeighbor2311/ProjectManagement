using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ProjectManagement.Projects
{
    public interface IProjectAppService : ICrudAppService<ProjectDto, Guid, ProjectPagedAndSortedResultRequestDto, CreateUpdateProjectDto>
    {
    }
}
