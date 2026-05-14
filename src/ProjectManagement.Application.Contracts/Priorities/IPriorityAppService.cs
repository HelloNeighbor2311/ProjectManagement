using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ProjectManagement.Priorities
{
    public interface IPriorityAppService: ICrudAppService<PriorityDto, Guid, PagedAndSortedResultRequestDto,CreateUpdatePriorityDto>
    {
    }
}
